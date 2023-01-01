using System.Text;
using System.Text.RegularExpressions;

namespace SecureLookup.Commands;
internal class HelpCommandParameter
{
	[ParameterAlias("cmd", "c")]
	[ParameterDescription("Target command to print description.")]
	public string? Command { get; set; }
}

internal class HelpCommand : AbstractCommand
{
	public HelpCommand(Program instance) : base(instance, "help")
	{
	}

	public override string Description => "Lists all available commands, or prints the help message for specific command.";

	public override string HelpMessage => ParameterDeserializer.GetHelpMessage<HelpCommandParameter>();


	protected override bool Execute(string[] args)
	{
		if (!ParameterDeserializer.TryParse(out HelpCommandParameter param, args))
			return false;

		AbstractCommand? command;
		if (string.IsNullOrWhiteSpace(param.Command) || (command = Instance.CommandFactory.FindCommand(param.Command)) is null)
		{
			var builder = new StringBuilder();
			builder.AppendLine("List of available commands: ");
			foreach (AbstractCommand cmd in Instance.CommandFactory.RegisteredCommands)
				builder.Append("  * ").Append(cmd.Name).Append("    - ").AppendLine(cmd.Description);

			Console.WriteLine(builder.ToString());
			Console.WriteLine("help -Command=<command name> to see command-specific help message");
		}
		else
		{
			Console.WriteLine(command.Name + " - " + command.Description);
			Console.WriteLine();
			Console.WriteLine(command.HelpMessage);
		}
		return true;
	}
}
