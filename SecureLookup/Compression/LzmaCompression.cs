using SharpCompress.Compressors.LZMA;
using System.Buffers.Binary;

namespace SecureLookup.Compression;
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

	public override Stream Compress(Stream outStream, IReadOnlyDictionary<string, string> props)
	{
		return new LzmaStream(CreateEncoderProperties(
			int.Parse(props[DictionarySizeProp]),
			props[MatchFinderProp],
			int.Parse(props[NumFastBytesProp]),
			int.Parse(props[LiteralContextBitsProp]),
			int.Parse(props[LiteralPosBitsProp]),
			int.Parse(props[PosStateBitsProp])), false, outStream);
	}

	public override Stream Decompress(Stream inStream, IReadOnlyDictionary<string, string> props)
	{
		return new LzmaStream(CreateDecoderProperties(
			uint.Parse(props[DictionarySizeProp]),
			int.Parse(props[LiteralContextBitsProp]),
			int.Parse(props[LiteralPosBitsProp]),
			int.Parse(props[PosStateBitsProp])), inStream);
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
