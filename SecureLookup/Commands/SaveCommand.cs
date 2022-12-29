namespace SecureLookup.Commands;
internal class SaveCommand : AbstractCommand
{
	public override string HelpMessage => "";

	public override string Description => "Saves the database.";

	public SaveCommand(Program instance) : base(instance, "save")
	{
	}

	protected override bool Execute(string[] args)
	{
		Instance.SaveDb();
		return true;
	}
}
