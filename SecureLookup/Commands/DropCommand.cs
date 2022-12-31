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

	protected override string AdditionalHelpMessage => ParameterSerializer.GetHelpMessage<DropCommandParameter>("Drop parameters");

	public DropCommand(Program instance) : base(instance, "drop")
	{
	}

	protected override bool ExecuteForEntries(string[] args, IList<DbEntry> entries)
	{
		Instance.EncryptedDb.MarkDirty();

		var builder = new StringBuilder();
		builder.Append("*** Total ").Append(entries.Count).AppendLine(" entries dropped.");
		foreach (DbEntry entry in entries)
		{
			AppendEntry(builder, entry);
			Instance.Db.Entries.Remove(entry);
		}
		Console.WriteLine(builder.ToString());
		return true;
	}
}
