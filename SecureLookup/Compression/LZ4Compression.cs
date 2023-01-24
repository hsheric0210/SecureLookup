using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;

namespace SecureLookup.Compression;
/*
 FIXME: Decompression failure

 d:\Repo\SecureLookup\SecureLookup\bin\Debug\net6.0>securelookup -db=test_PPMd -calg=PPMd -cprop=mem=16777216;o=6 -pass=abcd
Failed to load the database file. Maybe mismatched key?
System.AggregateException: Deserialization failure (There is an error in XML document (1, 185).)
 ---> System.InvalidOperationException: There is an error in XML document (1, 185).
 ---> System.Xml.XmlException: Unexpected end of file while parsing Name has occurred. Line 1, position 185.
   at System.Xml.XmlTextReaderImpl.Throw(Exception e)
   at System.Xml.XmlTextReaderImpl.Throw(String res, String arg)
   at System.Xml.XmlTextReaderImpl.Throw(Int32 pos, String res, String arg)
   at System.Xml.XmlTextReaderImpl.ParseQName(Boolean isQName, Int32 startOffset, Int32& colonPos)
   at System.Xml.XmlTextReaderImpl.ThrowTagMismatch(NodeData startTag)
   at System.Xml.XmlTextReaderImpl.ParseEndElement()
   at System.Xml.XmlTextReaderImpl.ParseElementContent()
   at System.Xml.XmlTextReaderImpl.Read()
   at System.Xml.XmlTextReaderImpl.Skip()
   at Microsoft.Xml.Serialization.GeneratedAssembly.XmlSerializationReaderDbInnerRoot.Read3_DbInnerRoot(Boolean isNullable, Boolean checkType)
   at Microsoft.Xml.Serialization.GeneratedAssembly.XmlSerializationReaderDbInnerRoot.Read4_root()
   --- End of inner exception stack trace ---
   at System.Xml.Serialization.XmlSerializer.Deserialize(XmlReader xmlReader, String encodingStyle, XmlDeserializationEvents events)
   at System.Xml.Serialization.XmlSerializer.Deserialize(Stream stream)
   at SecureLookup.Db.DatabaseLoader.DeserializeInner(Byte[] inner) in D:\Repo\SecureLookup\SecureLookup\Db\DatabaseLoader.cs:line 95
   --- End of inner exception stack trace ---
   at SecureLookup.Db.DatabaseLoader.DeserializeInner(Byte[] inner) in D:\Repo\SecureLookup\SecureLookup\Db\DatabaseLoader.cs:line 99
   at SecureLookup.Db.DatabaseLoader.Run(String source, Byte[] password) in D:\Repo\SecureLookup\SecureLookup\Db\DatabaseLoader.cs:line 21
   at SecureLookup.Program..ctor(String dbFile, String password, Boolean loop, String[] args) in D:\Repo\SecureLookup\SecureLookup\Program.cs:line 127
 */
internal class LZ4Compression : AbstractStreamCompression
{
	protected const string ChainBlocks = "cb";
	protected const string BlockSize = "bs";
	protected const string ContentChecksum = "cc";
	protected const string BlockChecksum = "bc";
	protected const string CompressionLevel = "x";
	protected const string ExtraMemory = "mem";

	public override IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>()
	{
		[ChainBlocks] = "true",
		[BlockSize] = "65536",
		[ContentChecksum] = "true",
		[BlockChecksum] = "true",
		[CompressionLevel] = ((int)LZ4Level.L12_MAX).ToString(),
		[ExtraMemory] = "256"
	};

	public LZ4Compression() : base("LZ4")
	{
	}

	public override Stream Compress(Stream uncompressed, IReadOnlyDictionary<string, string> props)
	{
		var settings = new LZ4EncoderSettings()
		{
			ContentLength = uncompressed.Length,
			ChainBlocks = bool.Parse(props[ChainBlocks]),
			BlockSize = int.Parse(props[BlockSize]),
			ContentChecksum = bool.Parse(props[ContentChecksum]),
			BlockChecksum = bool.Parse(props[BlockChecksum]),
			CompressionLevel = (LZ4Level)int.Parse(props[ChainBlocks]),
			ExtraMemory = int.Parse(props[ExtraMemory])
		};
		var outStream = new MemoryStream((int)uncompressed.Length);
		outStream.Write(BitConverter.GetBytes(settings.ExtraMemory));
		outStream.Write(BitConverter.GetBytes(uncompressed.Length));
		using LZ4EncoderStream compress = LZ4Stream.Encode(outStream, settings);
		uncompressed.CopyTo(compress);
		return outStream;
	}

	public override Stream Decompress(Stream compressed)
	{
		var settings = new LZ4DecoderSettings()
		{
			ExtraMemory = (int)compressed.ReadLong()
		};

		var uncompressedLen = compressed.ReadLong();
		var outStream = new MemoryStream((int)uncompressedLen);
		using LZ4DecoderStream decompress = LZ4Stream.Decode(compressed, settings);
		decompress.CopyTo(outStream, uncompressedLen);
		return outStream;
	}

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props)
	{
		return props.ContainsKey(ChainBlocks)
			&& props.ContainsKey(BlockSize)
			&& props.ContainsKey(ContentChecksum)
			&& props.ContainsKey(BlockChecksum)
			&& props.ContainsKey(CompressionLevel)
			&& props.ContainsKey(ExtraMemory)
			&& bool.TryParse(props[ChainBlocks], out _)
			&& int.TryParse(props[BlockSize], out _)
			&& bool.TryParse(props[ContentChecksum], out _)
			&& bool.TryParse(props[BlockChecksum], out _)
			&& ushort.TryParse(props[CompressionLevel], out _)
			&& int.TryParse(props[ExtraMemory], out _);
	}
}
