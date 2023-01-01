using SecureLookup.Db;
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

	protected override string AdditionalHelpMessage => ParameterDeserializer.GetHelpMessage<DropCommandParameter>("Drop parameters");

	public DropCommand(Program instance) : base(instance, "drop")
	{
	}

	protected override bool ExecuteForEntries(string[] args, IList<DbEntry> entries)
	{
		Console.WriteLine($"*** Total {entries.Count} entries selected.");
		if (entries.Count > 1 && !ConsoleUtils.CheckContinue("Multiple entries are selected."))
		{
			return true;
		}

		var builder = new StringBuilder();
		Instance.Database.MarkDirty();
		foreach (DbEntry entry in entries)
		{
			AppendEntry(builder, entry);
			DbRoot.Entries.Remove(entry);
		}
		Console.WriteLine(builder.ToString());
		return true;
	}
}
