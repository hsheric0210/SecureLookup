namespace SecureLookup.Commands;
internal class ImportCommand : AbstractCommand
{
	public override string HelpMessage => "<batchList_filePath> [separator]";

	public override string Description => "Imports entry list from the external source.";

	public ImportCommand(Program instance) : base(instance, "import")
	{
	}

	protected override bool Execute(string[] args) => throw new NotImplementedException();
}
