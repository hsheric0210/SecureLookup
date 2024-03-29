﻿using SharpCompress.Compressors.PPMd;
using System.Buffers.Binary;

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
internal class PPMdCompression : AbstractStreamCompression
{
	protected const string AllocatorSizeProp = "mem";
	protected const string ModelOrderProp = "o";

	public override IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>()
	{
		[AllocatorSizeProp] = "27",
		[ModelOrderProp] = "8"
	};

	public PPMdCompression() : base("PPMd")
	{
	}

	public override Stream Compress(Stream uncompressed, IReadOnlyDictionary<string, string> props)
	{
		var allocatorSize = int.Parse(props[AllocatorSizeProp]);
		if (allocatorSize <= 32)
			allocatorSize = 2 << allocatorSize;
		var modelOrder = int.Parse(props[ModelOrderProp]);
		var ppmdProps = new PpmdProperties(allocatorSize, modelOrder);
		var outStream = new MemoryStream();
		outStream.Write(ppmdProps.Properties);
		outStream.Write(BitConverter.GetBytes(uncompressed.Length));
		using var compress = new PpmdStream(ppmdProps, outStream, true);
		uncompressed.CopyTo(compress);
		return outStream;
	}

	public override Stream Decompress(Stream compressed)
	{
		var props = compressed.ReadBytes(2);
		var uncompressedLen = compressed.ReadLong();
		using var decompress = new PpmdStream(new PpmdProperties(props), compressed, false);
		var outStream = new MemoryStream((int)uncompressedLen);
		decompress.CopyTo(outStream, uncompressedLen);
		return outStream;
	}

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props)
	{
		return props.ContainsKey(AllocatorSizeProp)
			&& props.ContainsKey(ModelOrderProp)
			&& int.TryParse(props[AllocatorSizeProp], out _)
			&& int.TryParse(props[ModelOrderProp], out _);
	}
}
