﻿using System.Text;
using System.Xml.Serialization;

namespace SecureLookup;

[XmlRoot("root")]
public class DbInnerRoot
{
	[XmlArray("entries")]
	[XmlArrayItem("entry")]
	public List<DbEntry> Entries { get; set; } = new List<DbEntry>();
}

public class DbEntry
{
	[XmlElement("name")]
	public string Name { get; set; } = "";

	[XmlElement("originalFileName")]
	public string OriginalFileName { get; set; } = "";

	[XmlElement("archiveFileName")]
	public string ArchiveFileName { get; set; } = "";

	[XmlElement("password")]
	public string Password { get; set; } = "";

	[XmlArray("urls", IsNullable = true)]
	[XmlArrayItem("url")]
	public List<string>? Urls { get; set; }

	[XmlArray("notes", IsNullable = true)]
	[XmlArrayItem("note")]
	public List<string>? Notes { get; set; }
}
