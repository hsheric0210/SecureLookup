namespace SecureLookup.Commands;
public abstract class AbstractCommand
{
	protected Program Instance { get; }

	internal string Name { get; }

	public abstract string Description { get; }

	public abstract string HelpMessage { get; }

	protected AbstractCommand(Program instance, string name)
	{
		Instance = instance;
		Name = name;
	}

	public void TryExecute(string[] args)
	{
		if (!Execute(args))
			Console.WriteLine(HelpMessage);
	}

	protected abstract bool Execute(string[] args);
}
