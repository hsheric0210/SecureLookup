namespace SecureLookup.Commands;
public class CommandFactory
{
	public IReadOnlyList<AbstractCommand> RegisteredCommands { get; }
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
		cmdList.Add(new ExtractCommand(instance));
		cmdList.Add(new FindCommand(instance));
		cmdList.Add(new EditCommand(instance));
		cmdList.Add(new DropCommand(instance));
		cmdList.Add(new SaveCommand(instance));
		cmdList.Add(new ImportCommand(instance));
		cmdList.Add(new ChangePasswordCommand(instance));
		cmdList.Add(new CleanCommand(instance));
		cmdList.Add(new HelpCommand(instance));
		cmdList.Add(new ExitCommand(instance));
	}

	public AbstractCommand? FindCommand(string name) => RegisteredCommands.FirstOrDefault(cmd => cmd.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
