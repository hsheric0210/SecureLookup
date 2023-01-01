using SecureLookup.Db;

namespace SecureLookup.Compression;
public static class CompressionFactory
{
	private static readonly ICollection<AbstractCompression> registeredCompressions = new List<AbstractCompression>()
	{
		new GzipCompression(),
		new DeflateCompression(),
		new LzmaCompression(),
		new PPMdCompression()
	};

	/// <summary>
	/// Returns a list of all available algorithm names
	/// </summary>
	public static ICollection<string> GetAvailableAlgorithms() => registeredCompressions.Select(h => h.AlgorithmName).ToList();

	public static Stream Compress(DbCompressionEntry entry, Stream outStream)
	{
		AbstractCompression compression = Lookup(entry.AlgorithmName);
		return compression.Compress(outStream, PropertiesUtils.Deserialize(entry.Properties));
	}

	public static Stream Decompress(DbCompressionEntry entry, Stream inStream)
	{
		AbstractCompression compression = Lookup(entry.AlgorithmName);
		return compression.Decompress(inStream, PropertiesUtils.Deserialize(entry.Properties));
	}

	public static AbstractCompression Lookup(string algorithmName) => registeredCompressions.FirstOrDefault(enc => enc.AlgorithmName.Equals(algorithmName, StringComparison.OrdinalIgnoreCase)) ?? throw new NotSupportedException("Unknown compression algorithm: " + algorithmName);
}
