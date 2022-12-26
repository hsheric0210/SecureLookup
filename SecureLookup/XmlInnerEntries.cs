﻿using System.Xml.Serialization;

namespace SecureLookup;

[XmlRoot("root"), ]
public class XmlInnerRootEntry
{
	[XmlArray("entries")]
	[XmlArrayItem("entry")]
	public List<XmlInnerEntry> Entries { get; set; } = new List<XmlInnerEntry>();
}

public class XmlInnerEntry
{
	[XmlElement("name")]
	public string Name { get; set; } = "";

	[XmlElement("originalFileName")]
	public string OriginalFileName { get; set; } = "";

	[XmlElement("encryptedFileName")]
	public string EncryptedFileName { get; set; } = "";

	[XmlElement("password")]
	public string Password { get; set; } = "";

	[XmlElement("id", IsNullable = true)]
	public string? Id { get; set; }

	[XmlArray("urls", IsNullable = true)]
	[XmlArrayItem("url")]
	public List<string>? Urls { get; set; }

	[XmlArray("notes", IsNullable = true)]
	[XmlArrayItem("note")]
	public List<string>? Notes { get; set; }
}
