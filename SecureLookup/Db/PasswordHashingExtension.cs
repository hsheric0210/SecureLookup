using SecureLookup.Encryption;
using SecureLookup.PasswordHash;

namespace SecureLookup.Db;
public static class PasswordHashingExtension
{
	public static ReadOnlySpan<byte> PrimaryHashPassword(this DbOuterRoot outer, ReadOnlySpan<byte> password)
	{
		return outer.PrimaryPasswordHashing.HashPassword(
			"Primary",
			password,
			outer.PrimaryPasswordHashSize);
	}

	public static ReadOnlySpan<byte> SecondaryHashPassword(this DbOuterRoot outer, ReadOnlySpan<byte> primaryHashed)
	{
		return outer.SecondaryPasswordHashing.HashPassword(
			"Secondary",
			primaryHashed,
			EncryptionFactory.Lookup(outer.Encryption.AlgorithmName).KeySize);
	}

	private static ReadOnlySpan<byte> HashPassword(this DbPasswordHashingEntry entry, string prefix, ReadOnlySpan<byte> data, int hashLength)
	{
		try
		{
			return PasswordHashFactory.Hash(entry, data, hashLength);
		}
		catch (Exception ex)
		{
			throw new AggregateException(prefix + " password hashing failure", ex);
		}
	}
}
