namespace SecureLookup.Compression;
public abstract class AbstractStreamCompression : AbstractCompression
{
	protected AbstractStreamCompression(string algorithmName) : base(algorithmName)
	{
	}

	public override ReadOnlySpan<byte> Compress(ReadOnlySpan<byte> uncompressed, IReadOnlyDictionary<string, string> props)
	{
		using var inStream = new MemoryStream(uncompressed.ToArray());
		using Stream outStream = Compress(inStream, props);
		if (outStream is MemoryStream outMs)
			return outMs.ToArray();
		using var outMemStream = new MemoryStream();
		outStream.CopyTo(outMemStream);
		return outMemStream.ToArray();
	}

	public override ReadOnlySpan<byte> Decompress(ReadOnlySpan<byte> compressed)
	{
		using var inStream = new MemoryStream(compressed.ToArray());
		using Stream outStream = Decompress(inStream);
		if (outStream is MemoryStream outMs)
			return outMs.ToArray();
		using var outMemStream = new MemoryStream();
		outStream.CopyTo(outMemStream);
		return outMemStream.ToArray();
	}

	public abstract Stream Compress(Stream uncompressed, IReadOnlyDictionary<string, string> props);
	public abstract Stream Decompress(Stream compressed);
}
