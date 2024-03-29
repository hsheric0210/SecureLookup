﻿using Org.BouncyCastle.Asn1.Crmf;
using SharpCompress.Compressors.LZMA;
using System.Buffers;
using System.Buffers.Binary;

namespace SecureLookup.Compression;

/// <summary>
/// <list type="bullet">
/// <item><see href="https://github.com/jljusten/LZMA-SDK/blob/781863cdf592da3e97420f50de5dac056ad352a5/DOC/lzma-specification.txt#L50"/></item>
/// <item><see href="https://github.com/adamhathcock/sharpcompress/blob/d1ea8517d22cbb3b4401485e543ce3db04f25516/src/SharpCompress/Compressors/LZMA/LzmaStream.cs#L116"/></item>
/// <item><see href="https://github.com/adamhathcock/sharpcompress/blob/d1ea8517d22cbb3b4401485e543ce3db04f25516/src/SharpCompress/Compressors/LZMA/LzmaEncoder.cs#L1644"/></item>
/// <item><see href="https://chomdoo.tistory.com/16"/></item>
/// <item><see href="https://github.com/adamhathcock/sharpcompress/blob/d1ea8517d22cbb3b4401485e543ce3db04f25516/tests/SharpCompress.Test/Streams/LzmaStreamTests.cs#L564"/></item>
/// </list>
/// </summary>
internal class LzmaCompression : AbstractStreamCompression
{
	protected const string DictionarySizeProp = "d";
	protected const string MatchFinderProp = "mf";
	protected const string NumFastBytesProp = "fb";
	protected const string LiteralContextBitsProp = "lc";
	protected const string LiteralPosBitsProp = "lp";
	protected const string PosStateBitsProp = "pb";

	public override IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>()
	{
		[DictionarySizeProp] = "27",
		[MatchFinderProp] = "bt4",
		[NumFastBytesProp] = "32",
		[LiteralContextBitsProp] = "3",
		[LiteralPosBitsProp] = "0",
		[PosStateBitsProp] = "2"
	};

	public LzmaCompression() : base("LZMA")
	{
	}

	public override Stream Compress(Stream uncompressed, IReadOnlyDictionary<string, string> props)
	{
		var dictSize = int.Parse(props[DictionarySizeProp]);
		if (dictSize <= 32)
			dictSize = 2 << dictSize;
		LzmaEncoderProperties prop = CreateEncoderProperties(
			dictSize,
			props[MatchFinderProp],
			int.Parse(props[NumFastBytesProp]),
			int.Parse(props[LiteralContextBitsProp]),
			int.Parse(props[LiteralPosBitsProp]),
			int.Parse(props[PosStateBitsProp]));
		var outStream = new MemoryStream((int)uncompressed.Length);
		using var compress = new LzmaStream(prop, false, outStream);
		outStream.Write(compress.Properties);
		outStream.Write(BitConverter.GetBytes(uncompressed.Length));
		uncompressed.CopyTo(compress);
		return outStream;
	}

	public override Stream Decompress(Stream compressed)
	{
		var props = compressed.ReadBytes(5);
		var uncompressedLen = compressed.ReadLong();
		using var decompress = new LzmaStream(props, compressed, compressed.Length, -1, null, false);
		var ms = new MemoryStream((int)uncompressedLen);
		decompress.CopyTo(ms, uncompressedLen);
		return ms;
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
