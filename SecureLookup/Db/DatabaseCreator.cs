﻿using SecureLookup.Compression;
using SecureLookup.Encryption;
using SecureLookup.Hash;
using SecureLookup.PasswordHash;

namespace SecureLookup.Db;
public static class DatabaseCreator
{
	public static Database? Create(string destination, byte[] password, params string[] args)
	{
		if (!ParameterDeserializer.TryParse(out DatabaseCreationParameter param, args))
			return null;
		DbOuterRoot outer = PrepareOuter(param);

		var pwHash = outer.PrimaryHashPassword(password);
		var db = new Database()
		{
			OuterRoot = outer,
			InnerRoot = new DbInnerRoot(),
			PasswordHash = pwHash
		};
		db.Save();
		return db;
	}

	private static DbOuterRoot PrepareOuter(DatabaseCreationParameter param)
	{
		var outer = new DbOuterRoot();

		// Password hashing
		outer.PrimaryPasswordHashing = CreatePasswordHashing(param.PrimaryPasswordHashingAlgorithm, param.PrimaryPasswordHashingProperties);
		outer.PrimaryPasswordHashSize = param.PrimaryPasswordHashSize;
		outer.SecondaryPasswordHashing = CreatePasswordHashing(param.SecondaryPasswordHashingAlgorithm, param.SecondaryPasswordHashingProperties);

		// Compression
		outer.Compression = CreateCompression(param.DatabaseCompressionAlgorithm, param.DatabaseCompressionProperties);

		// Hash
		outer.Hash = CreateHash(param.DatabaseHashingAlgorithm);

		// Encryption
		outer.Encryption = CreateEncryption(param.DatabaseEncryptionAlgorithm);
		return outer;
	}

	private static DbPasswordHashingEntry CreatePasswordHashing(string algorithmName, string? props)
	{
		AbstractPasswordHash hash = PasswordHashFactory.Lookup(algorithmName);
		if (props.)
		if (!hash.IsPropertiesValid(PropertiesUtils.Deserialize(props)))
			throw new ArgumentException("Invalid properties: " + props);
		return new DbPasswordHashingEntry
		{
			AlgorithmName = algorithmName,
			Properties = props
		};
	}

	private static DbCompressionEntry CreateCompression(string algorithmName, string props)
	{
		AbstractCompression compression = CompressionFactory.Lookup(algorithmName);
		if (!compression.IsPropertiesValid(PropertiesUtils.Deserialize(props)))
			throw new ArgumentException("Invalid properties: " + props);
		return new DbCompressionEntry
		{
			AlgorithmName = algorithmName,
			Properties = props
		};
	}

	private static DbHashEntry CreateHash(string algorithmName)
	{
		_ = HashFactory.Lookup(algorithmName);
		return new DbHashEntry
		{
			AlgorithmName = algorithmName
		};
	}

	private static DbEncryptionEntry CreateEncryption(string algorithmName)
	{
		_ = EncryptionFactory.Lookup(algorithmName);
		return new DbEncryptionEntry
		{
			AlgorithmName = algorithmName
		};
	}
}