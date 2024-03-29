﻿using SecureLookup.Compression;
using SecureLookup.Encryption;
using SecureLookup.Hash;
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
	public static Database Load(string source, ReadOnlySpan<byte> password)
	{
		DbOuterRoot outer = LoadOuter(source);
		CheckDbIntegrity(outer);
		var primaryHashed = outer.PrimaryHashPassword(password);
		DbInnerRoot inner = DeserializeInner(Decompress(outer.Compression, Decrypt(outer, outer.SecondaryHashPassword(primaryHashed))));

		return new Database()
		{
			Source = source,
			OuterRoot = outer,
			InnerRoot = inner,
			PasswordHash = primaryHashed.ToArray()
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
			if (!HashFactory.Verify(outer.Hash, outer.Encryption.DataBytes, out var calculated))
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
	private static ReadOnlySpan<byte> Decrypt(DbOuterRoot outer, ReadOnlySpan<byte> key)
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

	private static ReadOnlySpan<byte> Decompress(DbCompressionEntry entry, ReadOnlySpan<byte> compressed)
	{
		try
		{
			return CompressionFactory.Decompress(entry, compressed);
		}
		catch (Exception ex)
		{
			throw new AggregateException("Decompression failure", ex);
		}
	}

	private static DbInnerRoot DeserializeInner(ReadOnlySpan<byte> inner)
	{
		try
		{
			using var ms = new MemoryStream(inner.ToArray());
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
