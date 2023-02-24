using System.Text;

namespace SecureLookup.Db;
public static class DbEntryExtension
{
	public static void AppendEntry(this DbEntry entry, StringBuilder builder)
	{
		builder.AppendLine().AppendLine("***");
		builder.Append("Name: ").AppendLine(entry.Name);
		builder.Append("Original file name: ").AppendLine(entry.OriginalFileName);
		builder.Append("Encrypted file name: ").AppendLine(entry.ArchiveFileName);
		builder.Append("Password: ").AppendLine(entry.Password);
		builder.Append("Last modified date: ").AppendLine(entry.LastModified?.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
		builder.Append("Created date: ").AppendLine(entry.Created?.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
		if (entry.Urls is not null && entry.Urls.Count > 0)
		{
			builder.AppendLine("Urls:");
			foreach (var url in entry.Urls)
				builder.Append("* ").AppendLine(url);
		}
		if (entry.Notes is not null && entry.Notes.Count > 0)
		{
			builder.AppendLine("Notes:");
			foreach (var notes in entry.Notes)
				builder.Append("* ").AppendLine(notes);
		}
		builder.AppendLine("***");
	}
}
