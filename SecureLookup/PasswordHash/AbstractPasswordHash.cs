namespace SecureLookup.PasswordHash;
public abstract class AbstractPasswordHash
{
	public string AlgorithmName { get; }
	public abstract int SaltSize { get; }

	protected AbstractPasswordHash(string algorithmName) => AlgorithmName = algorithmName;

	public abstract ReadOnlySpan<byte> Hash(ReadOnlySpan<byte> password, int desiredLength, ReadOnlySpan<byte> salt, IReadOnlyDictionary<string, string> props);
	public abstract bool IsPropertiesValid(IReadOnlyDictionary<string, string> props);
}

public sealed record EncryptedData(byte[] Data, byte[] Seed, byte[] Tag);