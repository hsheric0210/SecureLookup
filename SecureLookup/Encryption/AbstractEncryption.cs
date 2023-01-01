namespace SecureLookup.Encryption;
public abstract class AbstractEncryption
{
	public string AlgorithmName { get; }

	#region Abstract properties
	public abstract int KeySize { get; }
	public abstract int SeedSize { get; }
	public abstract int TagSize { get; }
	#endregion

	protected AbstractEncryption(string algorithmName) => AlgorithmName = algorithmName;

	public EncryptedData TryEncrypt(byte[] plaintext, byte[] key)
	{
		ValidateKeyLength(key.Length);
		return Encrypt(plaintext, key);
	}

	public byte[] TryDecrypt(EncryptedData encrypted, byte[] key)
	{
		ValidateKeyLength(key.Length);
		if (encrypted.Seed.Length != SeedSize)
			throw new ArgumentException($"Seed size mismatch: expected={SeedSize}, actual={encrypted.Seed.Length}");
		if (encrypted.Tag.Length != TagSize)
			throw new ArgumentException($"Tag size mismatch: expected={TagSize}, actual={encrypted.Tag.Length}");
		return Decrypt(encrypted, key);
	}

	private void ValidateKeyLength(int keyLength)
	{
		if (keyLength != KeySize)
			throw new ArgumentException($"Key size mismatch: expected={KeySize}, actual={keyLength}");
	}

	#region Abstract methods
	protected abstract EncryptedData Encrypt(byte[] plaintext, byte[] key);

	protected abstract byte[] Decrypt(EncryptedData encrypted, byte[] key);
	#endregion
}

public sealed record EncryptedData(byte[] Data, byte[] Seed, byte[] Tag);