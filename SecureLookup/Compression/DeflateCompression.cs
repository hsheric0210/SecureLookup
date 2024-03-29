﻿using SharpCompress.Compressors.Deflate;
using System.Text;

namespace SecureLookup.Compression;
internal class DeflateCompression : AbstractStreamCompression
{
	protected const string CompressionLevelProp = "x";

	public override IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>()
	{
		[CompressionLevelProp] = "9"
	};

	public DeflateCompression() : base("Deflate")
	{
	}

	public override Stream Compress(Stream uncompressed, IReadOnlyDictionary<string, string> props)
	{
		return new DeflateStream(
			uncompressed,
			SharpCompress.Compressors.CompressionMode.Compress,
			(CompressionLevel)int.Parse(props[CompressionLevelProp]),
			Encoding.UTF8);
	}

	public override Stream Decompress(Stream compressed)
	{
		return new DeflateStream(
			compressed,
			SharpCompress.Compressors.CompressionMode.Decompress,
			CompressionLevel.Default,
			Encoding.UTF8);
	}

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => props.ContainsKey(CompressionLevelProp)
		&& int.TryParse(props[CompressionLevelProp], out _);
}
