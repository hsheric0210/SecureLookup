namespace SecureLookup.Compression;
public abstract class AbstractCompression
{
	public string AlgorithmName { get; }

	protected AbstractCompression(string algorithmName) => AlgorithmName = algorithmName;

	public abstract byte[] Compress(byte[] uncompressed, IReadOnlyDictionary<string, string> props);
	public abstract byte[] Decompress(byte[] compressed);
	public abstract bool IsPropertiesValid(IReadOnlyDictionary<string, string> props);
}
