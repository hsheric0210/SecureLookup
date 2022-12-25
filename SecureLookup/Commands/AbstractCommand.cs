namespace SecureLookup.Commands;
public abstract class AbstractCommand
{
	protected Program Instance { get; }

	internal string Name { get; }

	protected abstract string Usage { get; }
	protected abstract int MandatoryParameterCount { get; }

	protected AbstractCommand(Program instance, string name)
	{
		Instance = instance;
		Name = name;
	}

	public void TryExecute(string[] args)
	{
		if (args.Length < MandatoryParameterCount || !Execute(args))
			Console.WriteLine($"Usage: {Name} {Usage}");
	}

	protected abstract bool Execute(string[] args);
}
