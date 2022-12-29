using System.Text;

namespace SecureLookup.Commands;
internal class FindCommand : AbstractFilterCommand
{
	public override string Description => "Finds the entries matching the filter rules.";

	public FindCommand(Program instance) : base(instance, "find")
	{
	}

	protected override bool ExecuteForEntries(string[] args, IList<DbEntry> entries)
	{
		var builder = new StringBuilder();
		builder.Append("*** Total ").Append(entries.Count).AppendLine(" entries found.");
		foreach (DbEntry entry in entries)
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
		}
		Console.WriteLine(builder.ToString());
		return true;
	}
}
