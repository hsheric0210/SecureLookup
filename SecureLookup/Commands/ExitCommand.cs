using SecureLookup.Parameter;

namespace SecureLookup.Commands;

internal class ExitCommandParameter
{
	[ParameterAlias("nosave", "d")]
	[ParameterDescription("Discard all changes and exit.")]
	public bool? Discard { get; set; }
}

internal class ExitCommand : AbstractCommand
{
	public override string Description => "Exits the program. Saves the database if it isn't saved yet.";

	public override string HelpMessage => "";

	public ExitCommand(Program instance) : base(instance, "exit")
	{
	}

	protected override bool Execute(string[] args)
	{
		if (!ParameterDeserializer.TryParse(out ExitCommandParameter param, args))
			return false;

		Instance.Exit(param.Discard == true);
		return true;
	}
}
