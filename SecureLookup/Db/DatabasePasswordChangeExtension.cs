using SecureLookup.Encryption;
using SecureLookup.PasswordHash;
using System.Security.Cryptography;

namespace SecureLookup.Db;
public static class DatabasePasswordChangeExtension
{
	public static void ChangePassword(this Database database, byte[] newPassword, string? newEncryptionAlgorithm)
	{
		DbOuterRoot outer = database.OuterRoot;
		RandomizeAnything(outer);
		TryChangeEncryptionAlgorithm(database, newPassword, newEncryptionAlgorithm, outer);

		database.Save();
	}

	private static void TryChangeEncryptionAlgorithm(Database database, byte[] newPassword, string? newEncryptionAlgorithm, DbOuterRoot outer)
	{
		if (string.IsNullOrWhiteSpace(newEncryptionAlgorithm))
			return;

		if (!EncryptionFactory.GetAvailableAlgorithms().Any(avail => avail.Equals(newEncryptionAlgorithm, StringComparison.OrdinalIgnoreCase)))
			throw new NotSupportedException("Unsupported encryption algorithm: " + newEncryptionAlgorithm);

		database.PasswordHash = outer.PrimaryHashPassword(newPassword);
	}

	private static void RandomizeAnything(this DbOuterRoot outer)
	{
		outer.PrimaryPasswordHashing.SaltBytes = RandomNumberGenerator.GetBytes(PasswordHashFactory.GetSaltSize(outer.PrimaryPasswordHashing));
		outer.PrimaryPasswordHashSize = RandomNumberGenerator.GetInt32(32, 129);
	}
}
