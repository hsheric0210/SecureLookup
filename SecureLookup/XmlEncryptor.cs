using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace SecureLookup;
internal static class XmlEncryptor
{
	/// <summary>
	/// Saves the xml document in encrypted form
	/// </summary>
	/// <param name="doc">The XML document</param>
	/// <param name="dest">The destination file</param>
	/// <param name="salt">The destination file</param>
	/// <param name="cparam">Cipher parameters to encrypt <paramref name="doc"/></param>
	public static void Save(XmlDocument doc, string dest, string password)
	{
		byte[] salt = CreateSalt();

		using FileStream fs = File.Open(dest, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
		using var xw = XmlWriter.Create(fs);
		xw.WriteStartDocument();
		xw.WriteStartElement("SecureLookup");
		xw.WriteAttributeString("salt", Convert.ToBase64String(salt));
		var encrypted = Encrypt(doc, salt, password);
		xw.WriteAttributeString("hash", Convert.ToBase64String(Hash(encrypted)));
		xw.WriteString(encrypted);
		xw.WriteEndElement();
	}

	private static byte[] CreateSalt()
	{
		// Argon2 recommends 128-bits of salt
		var salt = new byte[16];
		using var sr = RandomNumberGenerator.Create();
		sr.GetBytes(salt);
		return salt;
	}

	private static string Encrypt(XmlDocument doc, byte[] salt, string password)
	{
		using var ms = new MemoryStream();
		using (Aes cipher = CreateCipher(salt, password))
		{
			using var cs = new CryptoStream(ms, cipher.CreateEncryptor(), CryptoStreamMode.Write);
			using var sw = new StreamWriter(cs);
			using var xw = XmlWriter.Create(sw);
			doc.Save(xw);
			xw.Flush();
		}
		var res = Convert.ToBase64String(ms.ToArray());
		return res;
	}

	/// <summary>
	/// Loads the encrypted xml document
	/// </summary>
	/// <param name="src">The encrypted xml file</param>
	/// <param name="cparam">Cipher parameters to decrypt <paramref name="src"/></param>
	public static XmlDocument Load(string src, string password)
	{
		using FileStream fs = File.Open(src, FileMode.Open, FileAccess.Read, FileShare.Read);
		var doc = new XmlDocument();
		doc.Load(fs);
		XmlElement? elem = doc["SecureLookup"];
		if (elem is null)
			throw new XmlException("There is no node named 'SecureLookup' in database");
		Span<byte> salt = stackalloc byte[16];
		if (!elem.HasAttribute("salt") || !(Convert.TryFromBase64String(elem.GetAttribute("salt"), salt, out var saltBytes) && saltBytes == 16))
			throw new XmlException("There are no attribute named 'salt' in 'SecureLookup' node at database");

		Span<byte> savedHash = stackalloc byte[64]; // SHA2-512 uses 64 bytes
		if (!elem.HasAttribute("hash") || !(Convert.TryFromBase64String(elem.GetAttribute("hash"), savedHash, out var hashBytes) && hashBytes == 64))
			throw new XmlException("There are no attribute named 'hash' in 'SecureLookup' node at database");
		var encrypted = elem.InnerText;
		var calcHash = new Span<byte>(Hash(encrypted));
		if (!savedHash.SequenceEqual(calcHash))
			throw new CryptographicException("Encrypted data hash does not match with calculated one");

		return Decrypt(encrypted, salt.ToArray(), password);
	}

	private static XmlDocument Decrypt(string encrypted, byte[] salt, string password)
	{
		using var ms = new MemoryStream(Convert.FromBase64String(encrypted));
		using Aes cipher = CreateCipher(salt, password);
		using var cs = new CryptoStream(ms, cipher.CreateDecryptor(), CryptoStreamMode.Read);
		var doc = new XmlDocument();
		doc.Load(cs);
		return doc;
	}

	private static Aes CreateCipher(byte[] salt, string password)
	{
		var hasher = new Argon2id(Encoding.UTF8.GetBytes(password))
		{
			Salt = salt,
			DegreeOfParallelism = 12,
			MemorySize = 65536,
			Iterations = 64
		};
		var hash = hasher.GetBytes(48 /*32(key)+16(iv) bytes*/);

		// use first 32-bits as key
		var key = new byte[32];
		Array.ConstrainedCopy(hash, 0, key, 0, 32);

		// use second 32-bits as iv
		var iv = new byte[16];
		Array.ConstrainedCopy(hash, 32, iv, 0, 16);

		var cipher = Aes.Create();
		cipher.KeySize = 256; // AES-256
		cipher.Key = key;
		cipher.IV = iv;
		cipher.Mode = CipherMode.CBC;
		cipher.Padding = PaddingMode.Zeros;
		return cipher;
	}

	private static byte[] Hash(string content)
	{
		HashAlgorithm hash = SHA512.Create();
		return hash.ComputeHash(Encoding.UTF8.GetBytes(content));
	}
}
