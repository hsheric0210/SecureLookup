using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SecureLookup;
public class XmlOuterDb
{
	public const string OuterRootNodeName = "encrypted";
	public const string SaltAttributeName = "salt";
	public const string HashAttributeName = "hash";

	private readonly string fileName;
	private readonly byte[] password;
	public XmlOuterDb(string fileName, byte[] password)
	{
		this.fileName = fileName;
		this.password = password;
	}

	/// <summary>
	/// Saves the xml document in encrypted form
	/// </summary>
	/// <param name="doc">The XML document</param>
	/// <param name="dest">The destination file</param>
	/// <param name="salt">The destination file</param>
	/// <param name="cparam">Cipher parameters to encrypt <paramref name="doc"/></param>
	public void Save(XmlInnerRootEntry innerRoot)
	{
		var enc = new Encryption(password);

		using FileStream fs = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
		using var xw = XmlWriter.Create(fs);

		var encrypted = enc.Encrypt(innerRoot);
		var xdoc = new XDocument(
			new XDeclaration("1.0", "utf-8", null),
			new XElement(OuterRootNodeName,
				new XAttribute(SaltAttributeName, Convert.ToBase64String(enc.Salt)),
				new XAttribute(HashAttributeName, Hasher.Sha3(encrypted)),
				new XText(encrypted)));
		xdoc.Save(xw);
	}

	

	/// <summary>
	/// Loads the encrypted xml document
	/// </summary>
	/// <param name="src">The encrypted xml file</param>
	/// <param name="cparam">Cipher parameters to decrypt <paramref name="src"/></param>
	public XmlInnerRootEntry Load()
	{
		using FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		var doc = new XmlDocument();
		doc.Load(fs);

		XmlElement? elem = doc["encrypted"];
		if (elem is null)
			throw new XmlException($"There is no outer root node named '{OuterRootNodeName}' in database.");
		if (!elem.HasAttribute("salt"))
			throw new XmlException($"There are no salt attribute named '{SaltAttributeName}' in root node '{OuterRootNodeName}' at database.");
		if (!elem.HasAttribute("hash"))
			throw new XmlException($"There are no hash attribute named '{HashAttributeName}' in root node '{OuterRootNodeName}' at database.");

		// Read salt
		Span<byte> salt = stackalloc byte[16];
		if (!Convert.TryFromBase64String(elem.GetAttribute("salt"), salt, out var saltBytes) && saltBytes == 16)
			throw new CryptographicException($"Salt is not 16 bytes! (actual={saltBytes})");

		var encrypted = elem.InnerText;
		var enc = new Encryption(password, salt.ToArray());

		// Compare hash
		var expectedHash = Hasher.Sha3(encrypted);
		var actualHash = elem.GetAttribute("hash");
		if (!expectedHash.Equals(actualHash, StringComparison.OrdinalIgnoreCase))
			throw new CryptographicException($"Hash mismatch! (expected={expectedHash}, actual={actualHash})");

		return enc.Decrypt(encrypted);
	}
}
