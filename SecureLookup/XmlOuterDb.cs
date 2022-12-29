﻿using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

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
	public void Save(DbInnerRoot innerRoot)
	{
		var enc = new Encryption(password);


		var encrypted = enc.Encrypt(innerRoot);
		var outer = new DbOuterRoot()
		{
			Kdf = new DbKdfEntry
			{
				Salt = Convert.ToBase64String(enc.Salt)
			},
			Hash = Hasher.Sha3(encrypted),
			Data = encrypted
		};

		var serializer = new XmlSerializer(typeof(DbOuterRoot));
		using FileStream fs = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
		using var xw = XmlWriter.Create(fs, new XmlWriterSettings
		{
			Encoding = Encoding.UTF8,
			Indent = true
		});
		serializer.Serialize(xw, outer);
	}

	/// <summary>
	/// Loads the encrypted xml document
	/// </summary>
	/// <param name="src">The encrypted xml file</param>
	/// <param name="cparam">Cipher parameters to decrypt <paramref name="src"/></param>
	public DbInnerRoot Load()
	{
		using FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		var serializer = new XmlSerializer(typeof(DbOuterRoot));
		var outer = (DbOuterRoot)serializer.Deserialize(fs)!;

		// Read salt
		Span<byte> salt = stackalloc byte[16];
		if (!Convert.TryFromBase64String(outer.Kdf.Salt, salt, out var saltBytes) && saltBytes == 16)
			throw new CryptographicException($"Salt is not 16 bytes! (actual={saltBytes})");

		var data = outer.Data;
		var enc = new Encryption(password, salt.ToArray());

		// Compare hash
		var expectedHash = Hasher.Sha3(data);
		var actualHash = outer.Hash;
		if (!expectedHash.Equals(actualHash, StringComparison.OrdinalIgnoreCase))
			throw new CryptographicException($"Hash mismatch! (expected={expectedHash}, actual={actualHash})");

		return enc.Decrypt(data);
	}
}
