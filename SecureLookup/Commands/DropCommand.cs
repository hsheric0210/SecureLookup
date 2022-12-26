using System.Text;
using System.Text.RegularExpressions;

namespace SecureLookup.Commands;
internal class DropCommand : AbstractFilterCommand
{
	public DropCommand(Program instance) : base(instance, "drop")
	{
	}

	protected override bool ExecuteForEntries(IList<XmlInnerEntry> entries)
	{
		var builder = new StringBuilder();
		builder.Append("*** Total ").Append(entries.Count).AppendLine(" entries dropped.");
		foreach (XmlInnerEntry entry in entries)
		{
			builder.AppendLine().AppendLine("***");
			builder.Append("Name: ").AppendLine(entry.Name);
			builder.Append("Original file name: ").AppendLine(entry.OriginalFileName);
			builder.Append("Encrypted file name: ").AppendLine(entry.EncryptedFileName);
			builder.Append("Password: ").AppendLine(entry.Password);
			if (!string.IsNullOrWhiteSpace(entry.Id))
				builder.Append("Id: ").AppendLine(entry.Id);
			if (entry.Urls is not null && entry.Urls.Count > 0)
			{
				builder.AppendLine("Urls:");
				foreach (var url in entry.Urls)
					builder.Append("* ").AppendLine(url);
			}
			if (entry.Notes is not null && entry.Notes.Count > 0)
			{
				builder.AppendLine("Urls:");
				foreach (var notes in entry.Notes)
					builder.Append("* ").AppendLine(notes);
			}
			builder.AppendLine("***");
			Instance.Db.Entries.Remove(entry);
		}
		Console.WriteLine(builder.ToString());
		return true;
	}
}
