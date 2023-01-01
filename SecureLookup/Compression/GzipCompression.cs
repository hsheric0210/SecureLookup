using SharpCompress.Compressors.Deflate;

namespace SecureLookup.Compression;
internal class GzipCompression : AbstractCompression
{
	public GzipCompression() : base("GZip")
	{
	}

	public override Stream Compress(Stream outStream, IReadOnlyDictionary<string, string> props) => new GZipStream(outStream, SharpCompress.Compressors.CompressionMode.Compress);

	public override Stream Decompress(Stream inStream, IReadOnlyDictionary<string, string> props) => new GZipStream(inStream, SharpCompress.Compressors.CompressionMode.Decompress);

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => true;
}
