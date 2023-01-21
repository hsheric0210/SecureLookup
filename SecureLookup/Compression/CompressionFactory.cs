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
		new PPMdCompression()
	};

	/// <summary>
	/// Returns a list of all available algorithm names
	/// </summary>
	public static ICollection<string> GetAvailableAlgorithms() => registeredCompressions.Select(h => h.AlgorithmName).ToList();

	public static byte[] Compress(DbCompressionEntry entry, byte[] uncompressed) => Lookup(entry.AlgorithmName).Compress(uncompressed, PropertiesUtils.Deserialize(entry.Properties));

	public static byte[] Decompress(DbCompressionEntry entry, byte[] compressed) => Lookup(entry.AlgorithmName).Decompress(compressed);

	public static AbstractCompression Lookup(string algorithmName) => registeredCompressions.FirstOrDefault(enc => enc.AlgorithmName.Equals(algorithmName, StringComparison.OrdinalIgnoreCase)) ?? throw new NotSupportedException("Unknown compression algorithm: " + algorithmName);
}
