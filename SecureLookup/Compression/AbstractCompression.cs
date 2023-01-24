namespace SecureLookup.Compression;
public abstract class AbstractCompression
{
	public string AlgorithmName { get; }
	public abstract IReadOnlyDictionary<string, string>? DefaultProperties { get; }

	protected AbstractCompression(string algorithmName) => AlgorithmName = algorithmName;

	public abstract ReadOnlySpan<byte> Compress(ReadOnlySpan<byte> uncompressed, IReadOnlyDictionary<string, string> props);
	public abstract ReadOnlySpan<byte> Decompress(ReadOnlySpan<byte> compressed);
	public abstract bool IsPropertiesValid(IReadOnlyDictionary<string, string> props);
}
