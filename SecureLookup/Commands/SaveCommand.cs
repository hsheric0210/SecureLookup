namespace SecureLookup.Commands;
internal class SaveCommand : AbstractCommand
{
	public SaveCommand(Program instance) : base(instance, "save")
	{
	}

	protected override string ParameterExplain => "";

	protected override bool Execute(string[] args)
	{
		Instance.SaveDb();
		return true;
	}
}
