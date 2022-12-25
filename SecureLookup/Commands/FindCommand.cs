namespace SecureLookup.Commands;
internal class FindCommand : AbstractCommand
{
	public FindCommand(Program instance) : base(instance, "find")
	{
	}

	protected override string Usage => "<equals(e)/contains(c)/startsWith(s)/endsWith(e)/regex(r)> <keyword/regular>";

	protected override int MandatoryParameterCount => 2;

	protected override bool Execute(string[] args)
	{
		return true;
	}
}
