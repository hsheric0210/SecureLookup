using SecureLookup.Compression;
using SecureLookup.Encryption;
using SecureLookup.Hash;
using SecureLookup.Parameter;
using SecureLookup.PasswordHash;
using System.Security.Cryptography;
using ZstdNet;

namespace SecureLookup.Db;
public static class DatabaseCreator
{
	public static Database Create(string destination, byte[] password, DatabaseCreationParameter param)
	{
		DbOuterRoot outer = PrepareOuter(param);
		var db = new Database()
		{
			Source = destination,
			OuterRoot = outer,
			InnerRoot = new DbInnerRoot(),
			PasswordHash = outer.PrimaryHashPassword(password).ToArray()
		};
		db.Save();
		return db;
	}

	private static DbOuterRoot PrepareOuter(DatabaseCreationParameter param)
	{
		return new DbOuterRoot
		{
			// Password hashing
			PrimaryPasswordHashSize = param.PrimaryPasswordHashSize,
			PrimaryPasswordHashing = CreatePasswordHashing(param.PrimaryPasswordHashingAlgorithm, param.PrimaryPasswordHashingProperties),
			SecondaryPasswordHashing = CreatePasswordHashing(param.SecondaryPasswordHashingAlgorithm, param.SecondaryPasswordHashingProperties),

			// Compression
			Compression = CreateCompression(param.DatabaseCompressionAlgorithm, param.DatabaseCompressionProperties),

			// Hash
			Hash = CreateHash(param.DatabaseHashingAlgorithm),

			// Encryption
			Encryption = CreateEncryption(param.DatabaseEncryptionAlgorithm)
		};
	}

	private static DbPasswordHashingEntry CreatePasswordHashing(string algorithmName, string? props)
	{
		AbstractPasswordHash hash = PasswordHashFactory.Lookup(algorithmName);
		if (props is null)
		{
			props = hash.DefaultProperties is null ? "" : PropertiesUtils.Serialize(hash.DefaultProperties);
			if (hash.DefaultProperties is not null)
				Console.WriteLine($"Using default password hashing properties for {hash.AlgorithmName}: {props}");
		}
		if (!hash.IsPropertiesValid(PropertiesUtils.Deserialize(props)))
			throw new ArgumentException("Invalid properties: " + props);
		return new DbPasswordHashingEntry
		{
			AlgorithmName = hash.AlgorithmName,
			SaltBytes = RandomNumberGenerator.GetBytes(hash.SaltSize),
			Properties = props,
		};
	}

	private static DbCompressionEntry CreateCompression(string algorithmName, string? props)
	{
		AbstractCompression compression = CompressionFactory.Lookup(algorithmName);
		if (props is null)
		{
			props = compression.DefaultProperties is null ? "" : PropertiesUtils.Serialize(compression.DefaultProperties);
			if (compression.DefaultProperties is not null)
				Console.WriteLine("Using default compressor properties for " + compression.AlgorithmName + ": " + props);
		}
		if (!compression.IsPropertiesValid(PropertiesUtils.Deserialize(props)))
			throw new ArgumentException("Invalid properties: " + props);
		return new DbCompressionEntry
		{
			AlgorithmName = compression.AlgorithmName,
			Properties = props
		};
	}

	private static DbHashEntry CreateHash(string algorithmName)
	{
		AbstractHash hash = HashFactory.Lookup(algorithmName);
		return new DbHashEntry
		{
			AlgorithmName = hash.AlgorithmName
		};
	}

	private static DbEncryptionEntry CreateEncryption(string algorithmName)
	{
		AbstractEncryption encryption = EncryptionFactory.Lookup(algorithmName);
		return new DbEncryptionEntry
		{
			AlgorithmName = encryption.AlgorithmName
		};
	}
}