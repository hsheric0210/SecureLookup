using SecureLookup.Db;

namespace SecureLookup.Compression;
public static class CompressionFactory
{
	private static readonly ICollection<AbstractCompression> registeredCompressions = new List<AbstractCompression>()
	{
		new NoneCompression(),
		new GzipCompression(),
		new DeflateCompression(),
		new LzmaCompression(),
		new PPMdCompression(),
		new BrotliCompression(),
		new LZ4Compression(),
		new ZstdCompression()
	};

	/// <summary>
	/// Returns a list of all available algorithm names
	/// </summary>
	public static ICollection<string> GetAvailableAlgorithms() => registeredCompressions.Select(h => h.AlgorithmName).ToList();

	public static ReadOnlySpan<byte> Compress(DbCompressionEntry entry, ReadOnlySpan<byte> uncompressed) => Lookup(entry.AlgorithmName).Compress(uncompressed, PropertiesUtils.Deserialize(entry.Properties));

	public static ReadOnlySpan<byte> Decompress(DbCompressionEntry entry, ReadOnlySpan<byte> compressed) => Lookup(entry.AlgorithmName).Decompress(compressed);

	public static AbstractCompression Lookup(string algorithmName) => registeredCompressions.FirstOrDefault(enc => enc.AlgorithmName.Equals(algorithmName, StringComparison.OrdinalIgnoreCase)) ?? throw new NotSupportedException("Unknown compression algorithm: " + algorithmName);
}
