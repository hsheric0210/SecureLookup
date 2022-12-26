using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace SecureLookup;
internal class Encryption
{
	private readonly byte[] derivedKey;
	internal byte[] Salt { get; }

	public Encryption(byte[] password, byte[] salt)
	{
		Salt = salt;
		derivedKey = Crypto.DeriveKey(password, salt, 48); // 48 = 32(key) + 16(iv)
	}

	public Encryption(byte[] password) : this(password, Crypto.GenerateBytes(16))
	{
	}

	public string Encrypt(XmlInnerRootEntry root)
	{
		using var ms = new MemoryStream();
		using (Aes cipher = CreateCipher())
		{
			using var cs = new CryptoStream(ms, cipher.CreateEncryptor(), CryptoStreamMode.Write);
			using var sw = new StreamWriter(cs);
			using var xw = XmlWriter.Create(sw);
			var serializer = new XmlSerializer(typeof(XmlInnerRootEntry));
			var ns = new XmlSerializerNamespaces();
			ns.Add("", "");
			serializer.Serialize(xw, root, ns);
			xw.Flush();
		}
		return Convert.ToBase64String(ms.ToArray());
	}

	public XmlInnerRootEntry Decrypt(string encrypted)
	{
		using var ms = new MemoryStream(Convert.FromBase64String(encrypted));
		using Aes cipher = CreateCipher();
		using var cs = new CryptoStream(ms, cipher.CreateDecryptor(), CryptoStreamMode.Read);

		var serializer = new XmlSerializer(typeof(XmlInnerRootEntry));
		var obj = serializer.Deserialize(cs);
		if (obj is not XmlInnerRootEntry root)
			throw new InvalidOperationException("Deserialization result is not XmlInnerRoot.");
		return root;
	}

	private Aes CreateCipher()
	{
		// use first 32-bits as key
		var key = new byte[32];
		Array.ConstrainedCopy(derivedKey, 0, key, 0, 32);

		// use second 32-bits as iv
		var iv = new byte[16];
		Array.ConstrainedCopy(derivedKey, 32, iv, 0, 16);

		var cipher = Aes.Create();
		cipher.KeySize = 256; // AES-256
		cipher.Key = key;
		cipher.IV = iv;
		cipher.Mode = CipherMode.CBC;
		cipher.Padding = PaddingMode.PKCS7;
		return cipher;
	}
}
