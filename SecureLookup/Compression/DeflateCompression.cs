using SharpCompress.Compressors.Deflate;
using System.Text;

namespace SecureLookup.Compression;
internal class DeflateCompression : AbstractCompression
{
	protected const string CompressionLevelProp = "x";

	public DeflateCompression() : base("Deflate")
	{
	}

	public override Stream Compress(Stream outStream, IReadOnlyDictionary<string, string> props) => new DeflateStream(outStream, SharpCompress.Compressors.CompressionMode.Compress, (CompressionLevel)int.Parse(props[CompressionLevelProp]), Encoding.UTF8);

	public override Stream Decompress(Stream inStream, IReadOnlyDictionary<string, string> props) => new DeflateStream(inStream, SharpCompress.Compressors.CompressionMode.Compress, (CompressionLevel)int.Parse(props[CompressionLevelProp]), Encoding.UTF8);
	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => props.ContainsKey(CompressionLevelProp) && int.TryParse(props[CompressionLevelProp], out _);
}
