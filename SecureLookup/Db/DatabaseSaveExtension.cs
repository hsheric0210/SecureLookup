using SecureLookup.Compression;
using SecureLookup.Encryption;
using SecureLookup.Hash;
using SecureLookup.PasswordHash;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SecureLookup.Db;
public static class DatabaseSaveExtension
{
	/// <summary>
	/// Saves the specified database to specified file
	/// </summary>
	/// <param name="database">The database to save</param>
	/// <param name="destination">The database source file</param>
	/// <returns>Loaded <see cref="Database"/> instance</returns>
	/// <exception cref="AggregateException">Thrown if there are any exception occurs during database load</exception>
	public static void Save(this Database database)
	{
		DbOuterRoot outer = database.OuterRoot;
		RandomizeParameters(outer);
		var key = SecondaryPasswordHash(outer, database.PasswordHash);
		database.OuterRoot.Encryption = EncryptAndHash(outer, CompressInner(outer.Compression, SerializeInner(database.InnerRoot)), key);
		SerializeAndWriteOuter(database.OuterRoot, database.Source);
	}

	private static byte[] SerializeInner(DbInnerRoot inner)
	{
		try
		{
			using var ms = new MemoryStream();
			var serializer = new XmlSerializer(typeof(DbInnerRoot));
			serializer.Serialize(ms, inner);
			return ms.ToArray();
		}
		catch (Exception ex)
		{
			throw new AggregateException("Inner serialization failure", ex);
		}
	}

	private static byte[] CompressInner(DbCompressionEntry entry, byte[] inner)
	{
		try
		{
			Console.WriteLine("Uncompressed: " + inner.Length);
			var compressed = CompressionFactory.Compress(entry, inner);
			Console.WriteLine("Compressed: " + compressed.Length);
			return compressed;
		}
		catch (Exception ex)
		{
			throw new AggregateException("Inner compression failure", ex);
		}
	}

	private static void RandomizeParameters(DbOuterRoot outer)
	{
		try
		{
			outer.SecondaryPasswordHashing.SaltBytes = RandomNumberGenerator.GetBytes(PasswordHashFactory.Lookup(outer.SecondaryPasswordHashing.AlgorithmName).SaltSize);
			outer.Encryption.SeedBytes = RandomNumberGenerator.GetBytes(EncryptionFactory.Lookup(outer.Encryption.AlgorithmName).SeedSize);
		}
		catch (Exception ex)
		{
			throw new AggregateException("Secondary password hashing salt randomization failure", ex);
		}
	}

	private static byte[] SecondaryPasswordHash(DbOuterRoot outer, byte[] passwordHash)
	{
		try
		{
			return PasswordHashFactory.Hash(outer.SecondaryPasswordHashing, passwordHash, EncryptionFactory.Lookup(outer.Encryption.AlgorithmName).KeySize);
		}
		catch (Exception ex)
		{
			throw new AggregateException("Secondary password hashing failure", ex);
		}
	}

	private static DbEncryptionEntry EncryptAndHash(DbOuterRoot outer, byte[] plaintext, byte[] key)
	{
		try
		{
			DbEncryptionEntry encrypted = EncryptionFactory.Encrypt(outer.Encryption.AlgorithmName, plaintext, key, out byte[] data);
			outer.Hash.HashBytes = HashFactory.Hash(outer.Hash.AlgorithmName, data);
			return encrypted;
		}
		catch (Exception ex)
		{
			throw new AggregateException("Encryption and hashing failure", ex);
		}
	}

	private static void SerializeAndWriteOuter(DbOuterRoot outer, string destination)
	{
		try
		{
			var serializer = new XmlSerializer(typeof(DbOuterRoot));
			using FileStream fs = File.Open(destination, FileMode.Create, FileAccess.Write, FileShare.Read);
			using var xw = XmlWriter.Create(fs, new XmlWriterSettings
			{
				Indent = true,
				Encoding = new UTF8Encoding(false)
			});
			serializer.Serialize(xw, outer);
		}
		catch (Exception ex)
		{
			throw new AggregateException("Outer serialization and writing failure", ex);
		}
	}
}
