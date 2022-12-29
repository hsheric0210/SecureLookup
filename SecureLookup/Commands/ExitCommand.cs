namespace SecureLookup.Commands;
internal class ExitCommand : AbstractCommand
{
	public ExitCommand(Program instance) : base(instance, "exit")
	{
	}

	protected override string HelpMessage => "";

	protected override bool Execute(string[] args)
	{
		Instance.Exit();
		return true;
	}
}
