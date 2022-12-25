namespace SecureLookup.Commands;
internal class ExitCommand : AbstractCommand
{
	public ExitCommand(Program instance) : base(instance, "exit")
	{
	}

	protected override string Usage => "";

	protected override int MandatoryParameterCount => 0;

	protected override bool Execute(string[] args)
	{
		Instance.Exit();
		return true;
	}
}
