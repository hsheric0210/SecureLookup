namespace SecureLookup.Hash;
public abstract class AbstractHash
{
	public string AlgorithmName { get; }

	protected AbstractHash(string algorithmName) => AlgorithmName = algorithmName;

	public abstract byte[] Hash(byte[] data);
}
