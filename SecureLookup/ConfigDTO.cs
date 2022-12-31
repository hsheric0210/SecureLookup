using System.Xml.Serialization;

namespace SecureLookup;
[XmlRoot("config")]
public class ConfigRoot
{
	[XmlElement("archiver")]
	public string ArchiverExecutable { get; set; } = "7z.exe";

	[XmlElement("archiverParameter")]
	public string ArchiverParameter { get; set; } = "a -t7z -p\"{Password}\" -mhe -ms=1G -mqs -slp -bt -bb3 -bsp1 -sae -y -- \"{Archive}\" \"{Target}\"";

	[XmlElement("unarchiver")]
	public string UnarchiverExecutable { get; set; } = "7z.exe";

	[XmlElement("unarchiverParameter")]
	public string UnarchiverParameter { get; set; } = "x -t7z -p\"{Password}\" -o\"{Target}\" -slp -bt -bb3 -bsp1 -sae -y -- \"{Archive}\"";
}
