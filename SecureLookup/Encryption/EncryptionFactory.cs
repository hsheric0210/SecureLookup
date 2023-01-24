using SecureLookup.Db;

namespace SecureLookup.Encryption;
public static class EncryptionFactory
{
	private static readonly ICollection<AbstractEncryption> registeredEncryptions = new List<AbstractEncryption>()
	{
		new AesGcmEncryption(),
		new AesCbcEncryption(),
		new ChaCha20Poly1305Encryption(),
		new Cast5Encryption(),
		new Cast6Encryption(),
	};

	/// <summary>
	/// Returns a list of all available algorithm names
	/// </summary>
	public static ICollection<string> GetAvailableAlgorithms() => registeredEncryptions.Select(h => h.AlgorithmName).ToList();

	/// <summary>
	/// Encrypts specified message with specified encryption algorithm and key
	/// </summary>
	/// <param name="algorithmName">Name of encryption algorithm</param>
	/// <param name="plaintext">Message to encrypt</param>
	/// <param name="key">Encryption key</param>
	/// <returns>Encrypted data in XML DTO form</returns>
	/// <exception cref="NotImplementedException">If there're no encryption named <paramref name="algorithmName"/> found</exception>
	public static DbEncryptionEntry Encrypt(string algorithmName, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> key, out ReadOnlySpan<byte> data)
	{
		AbstractEncryption enc = Lookup(algorithmName);
		EncryptedData encrypted = enc.TryEncrypt(plaintext, key);
		data = encrypted.Data;
		return new DbEncryptionEntry
		{
			AlgorithmName = enc.AlgorithmName,
			SeedBytes = encrypted.Seed,
			DataBytes = encrypted.Data,
			TagBytes = encrypted.Tag
		};
	}

	public static DbEncryptionEntry Encrypt(string algorithmName, ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> key) => Encrypt(algorithmName, plaintext, key, out _);

	/// <summary>
	/// Decrypts specified encrypted entry with specified key
	/// </summary>
	/// <param name="entry">Encrypted data in XML DTO form</param>
	/// <param name="key">Decryption key</param>
	/// <returns></returns>
	public static ReadOnlySpan<byte> Decrypt(DbEncryptionEntry entry, ReadOnlySpan<byte> key)
	{
		var encrypted = new EncryptedData(
			entry.DataBytes,
			entry.SeedBytes,
			entry.TagBytes);
		return Lookup(entry.AlgorithmName).TryDecrypt(encrypted, key);
	}

	public static AbstractEncryption Lookup(string algorithmName) => registeredEncryptions.FirstOrDefault(enc => enc.AlgorithmName.Equals(algorithmName, StringComparison.OrdinalIgnoreCase)) ?? throw new NotSupportedException("Unknown encryption algorithm: " + algorithmName);
}
