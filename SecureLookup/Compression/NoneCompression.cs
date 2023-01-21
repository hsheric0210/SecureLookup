namespace SecureLookup.Compression;
internal class NoneCompression : AbstractCompression
{
	public NoneCompression() : base("None")
	{
	}

	public override byte[] Compress(byte[] uncompressed, IReadOnlyDictionary<string, string> props) => uncompressed;

	public override byte[] Decompress(byte[] compressed) => compressed;

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => true;
}
