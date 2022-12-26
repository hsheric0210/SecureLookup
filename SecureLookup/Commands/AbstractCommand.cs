namespace SecureLookup.Commands;
public abstract class AbstractCommand
{
	protected Program Instance { get; }

	internal string Name { get; }

	protected abstract string ParameterExplain { get; }

	protected AbstractCommand(Program instance, string name)
	{
		Instance = instance;
		Name = name;
	}

	public void TryExecute(string[] args)
	{
		if (!Execute(args))
			Console.WriteLine($"Available parameters for command '{Name}': {ParameterExplain}");
	}

	protected abstract bool Execute(string[] args);
}
