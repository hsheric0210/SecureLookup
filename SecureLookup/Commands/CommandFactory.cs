namespace SecureLookup.Commands;
public class CommandFactory
{
	private readonly IReadOnlyList<AbstractCommand> RegisteredCommands;
	private readonly Program instance;

	public CommandFactory(Program instance)
	{
		this.instance = instance;
		var cmdList = new List<AbstractCommand>();
		RegisterDefaultCommands(cmdList);
		RegisteredCommands = cmdList;
	}

	private void RegisterDefaultCommands(IList<AbstractCommand> cmdList)
	{
		cmdList.Add(new AddCommand(instance));
		cmdList.Add(new ImportCommand(instance));
		cmdList.Add(new ExitCommand(instance));
		cmdList.Add(new FindCommand(instance));
		cmdList.Add(new SaveCommand(instance));
		cmdList.Add(new DropCommand(instance));
		cmdList.Add(new ChangePasswordCommand(instance));
		// TODO: Export(Dump), Password, etc.
	}

	public AbstractCommand[] ListAvailableCommands() => RegisteredCommands.ToArray();

	public AbstractCommand? FindCommand(string name) => RegisteredCommands.FirstOrDefault(cmd => cmd.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
