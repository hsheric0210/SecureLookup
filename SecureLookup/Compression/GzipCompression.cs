using System.IO.Compression;

namespace SecureLookup.Compression;
/*
 FIXME: Decompression failure
d:\Repo\SecureLookup\SecureLookup\bin\Debug\net6.0>securelookup -db=test_GZip -calg=gzip -pass=abcd
Failed to load the database file. Maybe mismatched key?
System.AggregateException: Deserialization failure (There is an error in XML document (0, 0).)
 ---> System.InvalidOperationException: There is an error in XML document (0, 0).
 ---> System.Xml.XmlException: Root element is missing.
   at System.Xml.XmlTextReaderImpl.Throw(Exception e)
   at System.Xml.XmlTextReaderImpl.ParseDocumentContent()
   at System.Xml.XmlTextReaderImpl.Read()
   at System.Xml.XmlReader.MoveToContent()
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
internal class GzipCompression : AbstractStreamCompression
{
	public override IReadOnlyDictionary<string, string>? DefaultProperties => null;

	public GzipCompression() : base("GZip")
	{
	}

	public override Stream Compress(Stream uncompressed, IReadOnlyDictionary<string, string> props) => new GZipStream(uncompressed, CompressionMode.Compress);

	public override Stream Decompress(Stream compressed) => new GZipStream(compressed, CompressionMode.Decompress);

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => true;
}
