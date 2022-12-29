using System.Xml.Serialization;

namespace SecureLookup;

[XmlRoot("encrypted")]
public class DbOuterRoot
{
	[XmlElement("kdf")]
	public DbKdfEntry Kdf { get; set; } = new DbKdfEntry();

	[XmlElement("hash")]
	public string Hash { get; set; } = "";

	[XmlElement("data")]
	public string Data { get; set; } = "";
}

public class DbKdfEntry
{
	[XmlElement("type")]
	public string Type { get; set; } = "Argon2id";

	[XmlElement("iterations")]
	public int Iterations { get; set; } = 48;


	[XmlElement("salt")]
	public string Salt { get; set; } = "";

	[XmlElement("memsize")]
	public int MemorySize { get; set; } = 65536;

	[XmlElement("parallelism")]
	public int Parallelism { get; set; } = 12;
}
