using SecureLookup.Db;
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
			entry.AppendEntry(builder);
		Console.WriteLine(builder.ToString());
		return true;
	}
}
