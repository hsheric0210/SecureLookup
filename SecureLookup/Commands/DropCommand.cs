using System.Text;

namespace SecureLookup.Commands;

internal class DropCommandParameter
{
	[ParameterAlias("dump", "dmp")]
	[ParameterDescription("Prints the data of dropped entries")]
	public bool? Print { get; set; }
}

internal class DropCommand : AbstractFilterCommand
{
	public override string Description => "Removes the entries matching the filter from the database.";

	protected override string AdditionalHelpMessage => ParameterSerializer.GetHelpMessage<DropCommandParameter>();

	public DropCommand(Program instance) : base(instance, "drop")
	{
	}

	protected override bool ExecuteForEntries(string[] args, IList<DbEntry> entries)
	{
		Instance.MarkDbDirty();

		var builder = new StringBuilder();
		builder.Append("*** Total ").Append(entries.Count).AppendLine(" entries dropped.");
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
			Instance.Db.Entries.Remove(entry);
		}
		Console.WriteLine(builder.ToString());
		return true;
	}
}
