using SharpCompress.Compressors.LZMA;
using System.Buffers.Binary;

namespace SecureLookup.Compression;
/*
 FIXME: Decompression error
d:\Repo\SecureLookup\SecureLookup\bin\Debug\net6.0>securelookup -db=test_LZMA -calg=LZMA -cprop="d=16777216;mf=bt4;fb=64;lc=4;lp=0;pb=2" -pass=abcd
Failed to load the database file. Maybe mismatched key?
System.AggregateException: Decompression failure (Data Error)
 ---> SharpCompress.Compressors.LZMA.DataErrorException: Data Error
   at SharpCompress.Compressors.LZMA.Decoder.Code(Int32 dictionarySize, OutWindow outWindow, Decoder rangeDecoder)
   at SharpCompress.Compressors.LZMA.LzmaStream.Read(Byte[] buffer, Int32 offset, Int32 count)
   at System.IO.Stream.CopyTo(Stream destination, Int32 bufferSize)
   at System.IO.Stream.CopyTo(Stream destination)
   at SecureLookup.Compression.LzmaCompression.Decompress(Byte[] compressed, IReadOnlyDictionary`2 props) in D:\Repo\SecureLookup\SecureLookup\Compression\LzmaCompression.cs:line 42
   at SecureLookup.Compression.CompressionFactory.Decompress(DbCompressionEntry entry, Byte[] compressed) in D:\Repo\SecureLookup\SecureLookup\Compression\CompressionFactory.cs:line 29
   at SecureLookup.Db.DatabaseLoader.Decompress(DbCompressionEntry entry, Byte[] compressed) in D:\Repo\SecureLookup\SecureLookup\Db\DatabaseLoader.cs:line 81
   --- End of inner exception stack trace ---
   at SecureLookup.Db.DatabaseLoader.Decompress(DbCompressionEntry entry, Byte[] compressed) in D:\Repo\SecureLookup\SecureLookup\Db\DatabaseLoader.cs:line 85
   at SecureLookup.Db.DatabaseLoader.Run(String source, Byte[] password) in D:\Repo\SecureLookup\SecureLookup\Db\DatabaseLoader.cs:line 21
   at SecureLookup.Program..ctor(String dbFile, String password, Boolean loop, String[] args) in D:\Repo\SecureLookup\SecureLookup\Program.cs:line 127
 */
internal class LzmaCompression : AbstractCompression
{
	protected const string DictionarySizeProp = "d";
	protected const string MatchFinderProp = "mf";
	protected const string NumFastBytesProp = "fb";
	protected const string LiteralContextBitsProp = "lc";
	protected const string LiteralPosBitsProp = "lp";
	protected const string PosStateBitsProp = "pb";

	public LzmaCompression() : base("LZMA")
	{
	}

	public override byte[] Compress(byte[] uncompressed, IReadOnlyDictionary<string, string> props)
	{
		using var inStream = new MemoryStream(uncompressed);
		using var outStream = new MemoryStream();
		using var compress = new LzmaStream(CreateEncoderProperties(
			int.Parse(props[DictionarySizeProp]),
			props[MatchFinderProp],
			int.Parse(props[NumFastBytesProp]),
			int.Parse(props[LiteralContextBitsProp]),
			int.Parse(props[LiteralPosBitsProp]),
			int.Parse(props[PosStateBitsProp])), false, outStream);
		inStream.CopyTo(compress);
		return outStream.ToArray();
	}

	public override byte[] Decompress(byte[] compressed, IReadOnlyDictionary<string, string> props)
	{
		using var inStream = new MemoryStream(compressed);
		using var decompress = new LzmaStream(CreateDecoderProperties(
			uint.Parse(props[DictionarySizeProp]),
			int.Parse(props[LiteralContextBitsProp]),
			int.Parse(props[LiteralPosBitsProp]),
			int.Parse(props[PosStateBitsProp])), inStream);
		using var outStream = new MemoryStream();
		decompress.CopyTo(outStream);
		return outStream.ToArray();

	}

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props)
	{
		return props.ContainsKey(DictionarySizeProp)
			&& props.ContainsKey(MatchFinderProp)
			&& props.ContainsKey(NumFastBytesProp)
			&& props.ContainsKey(LiteralContextBitsProp)
			&& props.ContainsKey(LiteralPosBitsProp)
			&& props.ContainsKey(PosStateBitsProp)
			&& uint.TryParse(props[DictionarySizeProp], out _)
			&& uint.TryParse(props[NumFastBytesProp], out _)
			&& uint.TryParse(props[LiteralContextBitsProp], out _)
			&& uint.TryParse(props[LiteralPosBitsProp], out _)
			&& uint.TryParse(props[PosStateBitsProp], out _);
	}

	/// <summary>
	/// https://github.com/jljusten/LZMA-SDK/blob/781863cdf592da3e97420f50de5dac056ad352a5/DOC/lzma-specification.txt#L50
	/// </summary>
	private byte[] CreateDecoderProperties(uint dictionarySize = 1 << 20, int literalContextBits = 3, int literalPosBits = 0, int posStateBits = 2)
	{
		var dictSize = new byte[8];
		BinaryPrimitives.WriteUInt32LittleEndian(dictSize, dictionarySize);

		var bytes = new byte[9];
		bytes[0] = (byte)((posStateBits * 5 + literalPosBits) * 9 + literalContextBits);
		Array.ConstrainedCopy(dictSize, 0, bytes, 1, 8);
		return bytes;
	}

	private LzmaEncoderProperties CreateEncoderProperties(
		int dictionary,
		string matchFinder,
		int fastBytes,
		int litCtxBits,
		int litPosBits,
		int posStateBits)
	{
		var props = new LzmaEncoderProperties();
		var propsObj = new object[]
		{
			dictionary,
			posStateBits,
			litCtxBits,
			litPosBits,
			2, // algorithm
			fastBytes,
			matchFinder,
			false // eos
		};
		typeof(LzmaEncoderProperties).GetField("_properties", System.Reflection.BindingFlags.NonPublic)?.SetValue(props, propsObj);
		return props;
	}
}
