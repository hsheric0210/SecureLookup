using SecureLookup.Db;

namespace SecureLookup.PasswordHash;
public static class PasswordHashFactory
{
	private static readonly ICollection<AbstractPasswordHash> registeredPasswordHashes = new List<AbstractPasswordHash>()
	{
		new Pbkdf2HmacSha1PasswordHash(),
		new Pbkdf2HmacSha256PasswordHash(),
		new Pbkdf2HmacSha512PasswordHash(),
		new Pbkdf2BcHmacSha3256PasswordHash(),
		new Pbkdf2BcHmacSha3512PasswordHash(),
		new Argon2iPasswordHash(),
		new Argon2dPasswordHash(),
		new Argon2idPasswordHash(),
		new BCryptPasswordHash(),
		new SCryptPasswordHash()
	};

	/// <summary>
	/// Returns a list of all available algorithm names
	/// </summary>
	public static ICollection<string> GetAvailableAlgorithms() => registeredPasswordHashes.Select(h => h.AlgorithmName).ToList();

	/// <summary>
	/// Encrypts specified message with specified encryption algorithm and key
	/// </summary>
	/// <param name="algorithmName">Name of encryption algorithm</param>
	/// <param name="plaintext">Message to encrypt</param>
	/// <param name="key">Encryption key</param>
	/// <returns>Encrypted data in XML DTO form</returns>
	/// <exception cref="NotImplementedException">If there're no encryption named <paramref name="algorithmName"/> found</exception>
	public static ReadOnlySpan<byte> Hash(DbPasswordHashingEntry entry, ReadOnlySpan<byte> password, int desiredLength)
	{
		AbstractPasswordHash hash = Lookup(entry.AlgorithmName);
		return hash.Hash(password, desiredLength, entry.SaltBytes, PropertiesUtils.Deserialize(entry.Properties));
	}

	public static AbstractPasswordHash Lookup(string algorithmName) => registeredPasswordHashes.FirstOrDefault(enc => enc.AlgorithmName.Equals(algorithmName, StringComparison.OrdinalIgnoreCase)) ?? throw new NotImplementedException("Unknown password hashing algorithm: " + algorithmName);
}
