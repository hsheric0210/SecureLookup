using SharpCompress.Compressors.PPMd;

namespace SecureLookup.Compression;
internal class PPMdCompression : AbstractCompression
{
	protected const string AllocatorSizeProp = "mem";
	protected const string ModelOrderProp = "o";


	public PPMdCompression() : base("PPMd")
	{
	}

	public override Stream Compress(Stream outStream, IReadOnlyDictionary<string, string> props) => new PpmdStream(new PpmdProperties(int.Parse(props[AllocatorSizeProp]), int.Parse(props[ModelOrderProp])), outStream, true);

	public override Stream Decompress(Stream inStream, IReadOnlyDictionary<string, string> props) => new PpmdStream(new PpmdProperties(int.Parse(props[AllocatorSizeProp]), int.Parse(props[ModelOrderProp])), inStream, false);
	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props)
	{
		return props.ContainsKey(AllocatorSizeProp)
			&& props.ContainsKey(ModelOrderProp)
			&& int.TryParse(props[AllocatorSizeProp], out _)
			&& int.TryParse(props[ModelOrderProp], out _);
	}
}
