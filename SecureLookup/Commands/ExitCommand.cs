namespace SecureLookup.Commands;
internal class ExitCommand : AbstractCommand
{
	public override string Description => "Exits the program. Saves the database if it isn't saved yet.";

	public override string HelpMessage => "";

	public ExitCommand(Program instance) : base(instance, "exit")
	{
	}

	protected override bool Execute(string[] args)
	{
		if (Instance.EncryptedDb.Dirty)
			Instance.SaveDb();
		Instance.Exit();
		return true;
	}
}
