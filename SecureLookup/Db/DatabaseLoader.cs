using SecureLookup.Encryption;
using SecureLookup.Hash;
using SimpleBase;
using System.Xml.Serialization;

namespace SecureLookup.Db;
public class DatabaseLoader
{
	/// <summary>
	/// Loads the database from specific source file with password
	/// </summary>
	/// <param name="source">The database source file</param>
	/// <param name="password">Database decryption password</param>
	/// <returns>Loaded <see cref="Database"/> instance</returns>
	/// <exception cref="AggregateException">Thrown if there are any exception occurs during database load</exception>
	public static Database Run(string source, byte[] password)
	{
		DbOuterRoot outer = LoadOuter(source);
		CheckDbIntegrity(outer);
		var primaryHashed = outer.PrimaryHashPassword(password);
		DbInnerRoot inner = DeserializeInner(Decrypt(outer, outer.SecondaryHashPassword(primaryHashed)));

		return new Database()
		{
			OuterRoot = outer,
			InnerRoot = inner,
			PasswordHash = primaryHashed
		};
	}

	#region Load Outer and Check Integrity
	private static DbOuterRoot LoadOuter(string source)
	{
		try
		{
			using FileStream fs = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.Read);
			var serializer = new XmlSerializer(typeof(DbOuterRoot));
			return (DbOuterRoot)serializer.Deserialize(fs)!;
		}
		catch (Exception ex)
		{
			throw new AggregateException("Outer database load or deserialization failure", ex);
		}
	}

	private static void CheckDbIntegrity(DbOuterRoot outer)
	{
		try
		{
			var calculated = HashFactory.Hash(outer.Hash.AlgorithmName, outer.Encryption.DataBytes);
			if (!HashFactory.Verify(outer.Hash, calculated))
				throw new AggregateException($"Hash mismatch: algorithm={outer.Hash.AlgorithmName}, expected={outer.Hash.Hash}, calculated={Convert.ToHexString(calculated)}");
		}
		catch (Exception ex)
		{
			throw new AggregateException("Hash calculation failure", ex);
		}
	}
	#endregion

	#region Password hashing

	#endregion

	#region Decryption and Deserialization
	private static byte[] Decrypt(DbOuterRoot outer, byte[] key)
	{
		try
		{
			return EncryptionFactory.Decrypt(outer.Encryption, key);
		}
		catch (Exception ex)
		{
			throw new AggregateException("Decryption failure", ex);
		}
	}

	private static DbInnerRoot DeserializeInner(byte[] inner)
	{
		try
		{
			using var ms = new MemoryStream(inner);
			var serializer = new XmlSerializer(typeof(DbInnerRoot));
			return (DbInnerRoot)serializer.Deserialize(ms)!;
		}
		catch (Exception ex)
		{
			throw new AggregateException("Deserialization failure", ex);
		}
	}
	#endregion
}
