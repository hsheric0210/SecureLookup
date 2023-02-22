using SecureLookup.Db;
using SecureLookup.Parameter;
using System.Text;

namespace SecureLookup.Commands;

internal class DropCommandParameter
{
	[ParameterAlias("ay", "y")]
	[ParameterDescription("Assume yes to all user input prompts")]
	public bool AssumeAllYes { get; set; }
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
		if (!ParameterDeserializer.TryParse(out DropCommandParameter param, args))
			return false;

		Console.WriteLine($"*** Total {entries.Count} entries selected.");
		if (!param.AssumeAllYes && entries.Count > 1 && !ConsoleUtils.CheckContinue("Multiple entries are selected."))
			return true;

		var builder = new StringBuilder();
		Instance.Database.MarkDirty();
		foreach (DbEntry entry in entries)
		{
			entry.AppendEntry(builder);
			DbRoot.Entries.Remove(entry);
		}
		Console.WriteLine(builder.ToString());
		return true;
	}
}
