using Org.BouncyCastle.Tls.Crypto.Impl.BC;

namespace SecureLookup.Compression;
internal class NoneCompression : AbstractCompression
{
	public override IReadOnlyDictionary<string, string>? DefaultProperties => null;

	public NoneCompression() : base("None")
	{
	}

	public override ReadOnlySpan<byte> Compress(ReadOnlySpan<byte> uncompressed, IReadOnlyDictionary<string, string> props) => uncompressed;

	public override ReadOnlySpan<byte> Decompress(ReadOnlySpan<byte> compressed) => compressed;

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => true;
}
