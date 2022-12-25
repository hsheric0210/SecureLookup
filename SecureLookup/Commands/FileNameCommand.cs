namespace SecureLookup.Commands;
internal class FileNameCommand : AbstractCommand
{
	public FileNameCommand(Program instance) : base(instance, "filename")
	{
	}

	protected override string Usage => "<name>";

	protected override int MandatoryParameterCount => 1;

	protected override bool Execute(string[] args)
	{
		return true;
	}
}
