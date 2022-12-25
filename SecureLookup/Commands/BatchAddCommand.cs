namespace SecureLookup.Commands;
internal class BatchAddCommand : AbstractCommand
{
	public BatchAddCommand(Program instance) : base(instance, "batchadd")
	{
	}

	protected override string Usage => "<batchList_filePath> [separator]";

	protected override int MandatoryParameterCount => 1;

	protected override bool Execute(string[] args) => throw new NotImplementedException();
}
