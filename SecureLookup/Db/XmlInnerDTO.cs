﻿using System.Xml.Serialization;

namespace SecureLookup.Db;

[XmlRoot("root")]
public class DbInnerRoot
{
	[XmlArray("entries")]
	[XmlArrayItem("entry")]
	public List<DbEntry> Entries { get; set; } = new List<DbEntry>();

	[XmlArray("generatedFileNames")]
	[XmlArrayItem("fileName")]
	public HashSet<string> GeneratedFileNames { get; set; } = new HashSet<string>();
}

public sealed class DbEntry : IEquatable<DbEntry?>
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

	[XmlElement("lastModifiedDate")]
	public DateTime? LastModified { get; set; }

	[XmlElement("createdDate")]
	public DateTime? Created { get; set; }

	[XmlElement("flags")]
	public int Flags { get; set; }

	public override bool Equals(object? obj) => Equals(obj as DbEntry);
	public bool Equals(DbEntry? other) => other is not null && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
	public override int GetHashCode() => string.GetHashCode(Name, StringComparison.OrdinalIgnoreCase); // Distinct by Name (ignore case)
}
