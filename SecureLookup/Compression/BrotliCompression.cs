using System.IO.Compression;

namespace SecureLookup.Compression;
internal class BrotliCompression : AbstractStreamCompression
{
	protected const string CompressionLevelProp = "x";

	public override IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>()
	{
		[CompressionLevelProp] = "3"
	};

	public BrotliCompression() : base("Brotli")
	{
	}

	public override Stream Compress(Stream uncompressed, IReadOnlyDictionary<string, string> props)
	{
		return new BrotliStream(
			uncompressed,
			(CompressionLevel)int.Parse(props[CompressionLevelProp]));
	}

	public override Stream Decompress(Stream compressed)
	{
		return new BrotliStream(
			compressed,
			CompressionMode.Decompress);
	}

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => props.ContainsKey(CompressionLevelProp)
		&& int.TryParse(props[CompressionLevelProp], out _);
}
