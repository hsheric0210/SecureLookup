using SecureLookup.Encryption;
using SecureLookup.PasswordHash;

namespace SecureLookup.Db;
public static class PasswordHashingExtension
{
	public static byte[] PrimaryHashPassword(this DbOuterRoot outer, byte[] password)
	{
		return outer.PrimaryPasswordHashing.HashPassword(
			"Primary",
			password,
			outer.PrimaryPasswordHashSize);
	}

	public static byte[] SecondaryHashPassword(this DbOuterRoot outer, byte[] primaryHashed)
	{
		return outer.SecondaryPasswordHashing.HashPassword(
			"Secondary",
			primaryHashed,
			EncryptionFactory.Lookup(outer.Encryption.AlgorithmName).KeySize);
	}

	private static byte[] HashPassword(this DbPasswordHashingEntry entry, string prefix, byte[] data, int hashLength)
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
