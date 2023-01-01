namespace SecureLookup.Compression;
public abstract class AbstractCompression
{
	public string AlgorithmName { get; }

	protected AbstractCompression(string algorithmName) => AlgorithmName = algorithmName;

	public abstract Stream Compress(Stream outStream, IReadOnlyDictionary<string, string> props);
	public abstract Stream Decompress(Stream inStream, IReadOnlyDictionary<string, string> props);
	public abstract bool IsPropertiesValid(IReadOnlyDictionary<string, string> props);
}
