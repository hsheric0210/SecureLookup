using SecureLookup.Db;

namespace SecureLookup.Commands;
public abstract class AbstractCommand
{
	protected Program Instance { get; }

	internal string Name { get; }

	public DbInnerRoot DbRoot => Instance.Database.InnerRoot;

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

	protected string GenerateString(int nameLength, string? destFolder, string? dict, string? dictFile, bool isFileName = true)
	{
		var _dict = !string.IsNullOrWhiteSpace(dictFile) && new FileInfo(dictFile).Exists
					? DictionaryFileCache.ReadDictionary(dictFile)
					: dict;

		if (isFileName && string.IsNullOrWhiteSpace(destFolder))
			throw new ArgumentException(nameof(destFolder) + " cannot be empty if isFilename == true");

		if (string.IsNullOrWhiteSpace(_dict))
			throw new ArgumentException("Dictionary is empty");

		string str;
		do
		{
			str = RandomStringGenerator.RandomString(nameLength, _dict);
		}
		while (isFileName && (DbRoot.GeneratedFileNames.Contains(str) || new FileInfo(Path.Combine(destFolder, str)).Exists));
		if (isFileName)
		{
			Console.WriteLine($"New filename generated: '{str}'");
			DbRoot.GeneratedFileNames.Add(str);
		}
		return str;
	}
}
