namespace SecureLookup.Commands;
internal class SaveCommand : AbstractCommand
{
	public SaveCommand(Program instance) : base(instance, "save")
	{
	}

	protected override string Usage => "";

	protected override int MandatoryParameterCount => 0;

	protected override bool Execute(string[] args)
	{
		Instance.SaveDb();
		return true;
	}
}
