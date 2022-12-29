namespace SecureLookup.Commands;

internal class AddCommandParameter
{
	[ParameterAlias("n")]
	[ParameterDescription("Name of the entry; Each entry must have distinct name")]
	[MandatoryParameter]
	public string Name { get; set; } = "";

	[ParameterAlias("Path", "f")]
	[ParameterDescription("Original file path")]
	[MandatoryParameter]
	public string File { get; set; } = "";

	[ParameterAlias("pass", "psw", "pw", "p")]
	[ParameterDescription("The file encryption password")]
	[MandatoryParameter]
	public string Password { get; set; } = "";

	[ParameterAlias("ren")]
	[ParameterDescription("Rename existing file to generated name")]
	public bool? Rename { get; set; }

	[ParameterAlias("NewNameLen", "rsglen", "nnl")]
	[ParameterDescription("Length of generated file name")]
	public int? NewNameLength { get; set; }

	[ParameterAlias("NewNameDict", "namedict", "dict")]
	[ParameterDescription("Dictionary to generate new file name; Predefined dictionary names are available at README")]
	public string? NewNameDictionary { get; set; }

	[ParameterDescription("Entry id tag; Each entry must have distinct id")]
	public string? Id { get; set; }

	[ParameterDescription("Additional associated URLs separated in ';' char by default; the separator char could be reassigned by '-UrlSeparator' parameter")]
	public string? Urls { get; set; }

	[ParameterAlias("urlsep")]
	[ParameterDescription("URL separator char to separate URLs in '-Urls' parameter")]
	public string? UrlSeparator { get; set; }

	[ParameterDescription("Additional associated notes separated in ';' char by default; the separator char could be reassigned by '-NoteSeparator' parameter")]
	public string? Notes { get; set; }

	[ParameterAlias("notesep")]
	[ParameterDescription("Note separator char to separate notes in '-Notes' parameter")]
	public string? NoteSeparator { get; set; }
}

internal class AddCommand : AbstractCommand
{
	public AddCommand(Program instance) : base(instance, "add")
	{
	}

	protected override string HelpMessage => ParameterSerializer.GetHelpMessage<AddCommandParameter>();


	protected override bool Execute(string[] args)
	{
		if (!ParameterSerializer.TryParse(out AddCommandParameter param, args))
			return false;

		var name = param.Name;
		var file = param.File;
		var fileDir = Path.GetDirectoryName(Path.GetFullPath(file));
		if (string.IsNullOrWhiteSpace(fileDir))
		{
			Console.WriteLine("Directory of file is empty or null: " + file);
			return false;
		}

		var dict = param.NewNameDictionary ?? "AlphaNumeric";

		var urlsep = param.UrlSeparator ?? ";";
		var notesep = param.NoteSeparator ?? ";";

		if (!new FileInfo(file).Exists)
		{
			Console.WriteLine($"File '{Path.GetFullPath(file)}' not exists.");
			return false;
		}

		string newName;
		do
			newName = RandomStringGenerator.RandomString(param.NewNameLength ?? 64, dict);
		while (new FileInfo(Path.Combine(fileDir, newName)).Exists); // Check if any file with same name already exists.

		Console.WriteLine("New filename generated: " + newName);

		var deleted = Instance.Db.Entries.RemoveAll(entry => string.Equals(name, entry.Name, StringComparison.OrdinalIgnoreCase));
		if (deleted > 0)
			Console.WriteLine($"Overwriting {deleted} entry with same name '{name}'.");

		// duplicate id check
		var id = param.Id;
		if (id is not null)
		{
			deleted = Instance.Db.Entries.RemoveAll(entry => string.Equals(id, entry.Id, StringComparison.OrdinalIgnoreCase));
			if (deleted > 0)
				Console.WriteLine($"Overwriting {deleted} entries with same id '{id}'.");
		}

		Instance.Db.Entries.Add(new DbEntry()
		{
			Name = name,
			OriginalFileName = file,
			EncryptedFileName = newName,
			Password = param.Password,
			Id = id,
			Urls = param.Urls?.Split(urlsep).ToList(),
			Notes = param.Notes?.Split(notesep).ToList()
		});
		Console.WriteLine("Entry added. Don't forget to save the database!");

		if (param.Rename == true)
		{
			try
			{
				File.Move(file, Path.Combine(fileDir, newName));
				Console.WriteLine("Generated name applied to the file.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to rename file " + file + " to " + newName);
				Console.WriteLine(ex);
			}
		}

		return true;
	}
}
