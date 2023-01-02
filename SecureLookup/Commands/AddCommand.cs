using SecureLookup.Db;
using SecureLookup.Parameter;
using StringTokenFormatter;
using System.Diagnostics;

namespace SecureLookup.Commands;

internal class AddCommandParameter
{
	[ParameterDescription("Name of the entry; Entry with same name will be overwritten")]
	[MandatoryParameter]
	public string Name { get; set; } = "";

	[ParameterAlias("File", "Folder", "Source", "Src", "S")]
	[ParameterDescription("The path of file or folder that is going to be archived")]
	[MandatoryParameter]
	public string Path { get; set; } = "";

	[ParameterAlias("ArchiveRepo", "Repo", "Destination", "Dest")]
	[ParameterDescription("The folder where the archive files will be stored; same as original file or folder by default")]
	public string? ArchiveRepository { get; set; }

	[ParameterAlias("PassLen", "PSWLen", "PWLen", "PLen")]
	[ParameterDescription("Length of generated password")]
	public int PasswordLength { get; set; } = 64;

	[ParameterAlias("PassDict", "PSWDict", "PWDict", "PDict")]
	[ParameterDescription("Dictionary to generate new password; Predefined dictionary names are available at README")]
	public string PasswordDictionary { get; set; } = "SpecialAlphaNumeric";

	[ParameterAlias("Pass", "PSW", "PW")]
	[ParameterDescription("Use specific archive encryption password; character '|' is disallowed due its usage as separator")]
	public string? Password { get; set; }

	[ParameterAlias("ANameLen", "ALen", "AL")]
	[ParameterDescription("Length of generated archive file name")]
	public int ArchiveNameLength { get; set; } = 64;

	[ParameterAlias("ANameDict", "ADict", "AD")]
	[ParameterDescription("Dictionary of generated archive file name; Predefined dictionary names are available at README")]
	public string ArchiveNameDictionary { get; set; } = "AlphaNumeric";

	[ParameterAlias("AName", "AN")]
	[ParameterDescription("Use specific archive name; instead of generated name")]
	public string? ArchiveName { get; set; }

	[ParameterDescription("Additional associated URLs separated in ';' char by default; the separator char could be reassigned with '-UrlSeparator' parameter")]
	public string? Urls { get; set; }

	[ParameterAlias("UrlSep")]
	[ParameterDescription("URL separator char to separate URLs in '-Urls' parameter")]
	public string UrlSeparator { get; set; } = ";";

	[ParameterDescription("Additional associated notes separated in ';' char by default; the separator char could be reassigned with '-NoteSeparator' parameter")]
	public string? Notes { get; set; }

	[ParameterAlias("NoteSep")]
	[ParameterDescription("Note separator char to separate notes in '-Notes' parameter")]
	public string NoteSeparator { get; set; } = ";";

	[ParameterAlias("NoA")]
	[ParameterDescription("Don't call the archiver for specified file/folder")]
	public bool NoArchive { get; set; }

	[ParameterAlias("AppendTo", "Append")]
	[ParameterDescription(@"INSTEAD OF RUNNING ARCHIVER(Implicit '-NoArchive' switch), Append the generated(or specified) informations to specified file to support external archiving tools, in following format: <originalFileName>:<archiveFileName>:<password>
WARNING: You *MUST* run external archiving tools to archive your files")]
	public string? AppendLogTo { get; set; }
}

internal class AddCommand : AbstractCommand
{
	public override string Description => "Add a new entry to database and run archiver to archive specified file or folder.";

	public override string HelpMessage => ParameterDeserializer.GetHelpMessage<AddCommandParameter>();

	public AddCommand(Program instance) : base(instance, "add")
	{
	}

	protected override bool Execute(string[] args)
	{
		if (!ParameterDeserializer.TryParse(out AddCommandParameter param, args))
			return false;

		var password = param.Password;
		if (password?.Contains('|') == true)
		{
			Console.WriteLine("Character '|' is disallowed in password.");
			return true;
		}

		if (string.IsNullOrWhiteSpace(password))
		{
			password = RandomStringGenerator.RandomString(param.PasswordLength, param.PasswordDictionary);
			Console.WriteLine("Generated password: " + password);
		}

		var src = param.Path;
		var srcFileName = Path.GetFileName(src);
		var srcPath = Path.GetFullPath(src);
		var isFile = new FileInfo(srcPath).Exists;
		if (!isFile && !new DirectoryInfo(srcPath).Exists)
		{
			Console.WriteLine($"File or directory '{srcPath}' not exists.");
			return false;
		}

		string? dest;
		if (string.IsNullOrWhiteSpace(param.ArchiveRepository))
		{
			dest = isFile ? Path.GetDirectoryName(srcPath) : (new DirectoryInfo(srcPath).Parent?.FullName);
			Console.WriteLine("Using original file directory as Archive repository directory: " + dest);
		}
		else
		{
			dest = Path.GetFullPath(param.ArchiveRepository);
		}

		if (string.IsNullOrWhiteSpace(dest))
		{
			Console.WriteLine("Destination directory is null or inaccessible");
			return false;
		}

		var name = param.Name;
		DbEntry? duplicate = DeleteDuplicateNameEntries(name);

		var newName = GenerateNewFileName(param.ArchiveNameLength, dest, param.ArchiveNameDictionary);
		DbRoot.Entries.Add(new DbEntry()
		{
			Name = name,
			OriginalFileName = srcFileName, // Path is relative to database path
			ArchiveFileName = newName,
			Password = password,
			Urls = param.Urls?.Split(param.UrlSeparator).ToList() ?? duplicate?.Urls,
			Notes = param.Notes?.Split(param.NoteSeparator).ToList() ?? duplicate?.Notes
		});
		Instance.Database.MarkDirty();

		Console.WriteLine("Entry added. Don't forget to save the database!");

		var appendTo = param.AppendLogTo;
		if (!string.IsNullOrWhiteSpace(appendTo))
		{
			try
			{
				File.AppendAllText(appendTo, $"{srcFileName}|{newName}|{password}{Environment.NewLine}");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to append generated data to specified file.");
				Console.WriteLine(ex);
			}
			return true; // Skip calling archiver
		}

		if (!param.NoArchive)
			CallArchiver(srcPath, Path.Combine(dest, newName), password, isFile);

		return true;
	}

	private string GenerateNewFileName(int nameLength, string destFolder, string dict)
	{
		string newName;
		do
			newName = RandomStringGenerator.RandomString(nameLength, dict);
		while (DbRoot.GeneratedFileNames.Contains(newName) || new FileInfo(Path.Combine(destFolder, newName)).Exists);

		Console.WriteLine("New filename generated: " + newName);
		DbRoot.GeneratedFileNames.Add(newName);
		return newName;
	}

	private DbEntry? DeleteDuplicateNameEntries(string name)
	{
		bool predicate(DbEntry entry) => string.Equals(name, entry.Name, StringComparison.OrdinalIgnoreCase);

		DbEntry? first = DbRoot.Entries.Find(predicate);
		var deleted = DbRoot.Entries.RemoveAll(predicate);
		if (deleted > 0)
			Console.WriteLine($"Overwriting {deleted} entry with same name '{name}'.");
		return first;
	}

	private void CallArchiver(string srcPath, string destPath, string password, bool isFile)
	{
		var process = new Process();
		process.StartInfo.FileName = Instance.Config.ArchiverExecutable;
		process.StartInfo.Arguments = Instance.Config.ArchiverParameter.FormatToken(new
		{
			Target = isFile ? srcPath : "*",
			Archive = destPath,
			Password = password
		});
		process.StartInfo.WorkingDirectory = isFile ? (Path.GetDirectoryName(srcPath) ?? "") : srcPath;
		process.StartInfo.UseShellExecute = true;
		process.Start();
		process.WaitForExit();
		Console.WriteLine("Archiver exit with code: " + process.ExitCode);
	}
}
