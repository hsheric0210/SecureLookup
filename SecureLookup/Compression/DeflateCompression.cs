using SharpCompress.Compressors.Deflate;
using System.Text;

namespace SecureLookup.Compression;
internal class DeflateCompression : AbstractCompression
{
	protected const string CompressionLevelProp = "x";

	public DeflateCompression() : base("Deflate")
	{
	}

	public override byte[] Compress(byte[] uncompressed, IReadOnlyDictionary<string, string> props)
	{
		using var inStream = new MemoryStream(uncompressed);
		using var compress = new DeflateStream(inStream, SharpCompress.Compressors.CompressionMode.Compress, (CompressionLevel)int.Parse(props[CompressionLevelProp]), Encoding.UTF8);
		using var outStream = new MemoryStream();
		compress.CopyTo(outStream);
		return outStream.ToArray();
	}

	public override byte[] Decompress(byte[] compressed)
	{
		using var inStream = new MemoryStream(compressed);
		using var decompress = new DeflateStream(inStream, SharpCompress.Compressors.CompressionMode.Decompress, CompressionLevel.Default, Encoding.UTF8);
		using var outStream = new MemoryStream();
		decompress.CopyTo(outStream);
		return outStream.ToArray();
	}

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => props.ContainsKey(CompressionLevelProp) && int.TryParse(props[CompressionLevelProp], out _);
}
