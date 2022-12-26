using System.Xml;

namespace SecureLookup.Commands;
internal class AddCommand : AbstractCommand
{
	public AddCommand(Program instance) : base(instance, "add")
	{
	}

	protected override string Usage => @"
  Mandatory parameters:
	-name<name>		- Entry name
	-file<filePath>		- Original file path
	-pass<filePassword>	- File password
  Optional parameters:
		[-ren]				- Rename existing file to newly generated name
		[-rsglen<newName_length>]	- Length of generated new name
		[-dict<name_dictionary>]	- Name of dictionary to generate new file name
		[-id<id>]			- Specify the id tag
		[-urls<tags>]			- Additional tags, separated with ';' char by default
		[-urlsep<char>]			- Specify url separator char used in '-urls' switch
		[-notes<description>]		- Additional notes, separated with ';' char by default
		[-note<char>]			- Specify note separator char used in '-notes' switch";

	protected override int MandatoryParameterCount => 3;

	protected override bool Execute(string[] args)
	{
		var name = args.GetSwitch("name");
		var file = args.GetSwitch("file");
		var pass = args.GetSwitch("pass");
		var fileDir = Path.GetDirectoryName(file);
		if (name is null || file is null || pass is null || fileDir is null)
			return false;

		var dict = args.GetSwitch("dict") ?? "AlphaNumeric";

		var id = args.GetSwitch("id");

		if (id is not null && Instance.Db.Entries.Any(entry => string.Equals(id, entry.Id, StringComparison.OrdinalIgnoreCase)))
		{
		}

		var newNameLen = 64;
		var rsglen = args.GetSwitch("rsglen");
		if (rsglen is not null && int.TryParse(rsglen, out newNameLen))
			return false;

		var urlsep = args.GetSwitch("urlsep") ?? ";";
		var notesep = args.GetSwitch("notesep") ?? ";";

		if (!new FileInfo(file).Exists)
		{
			Console.WriteLine($"File \'{Path.GetFullPath(file)}\' not exists.");
			return false;
		}

		string newName;
		do
			newName = RandomStringGenerator.RandomString(newNameLen, dict);
		while (new FileInfo(Path.Combine(fileDir, newName)).Exists); // Check if any file with same name already exists.

		Instance.Db.Entries.Add(new XmlInnerEntry()
		{
			Name = name,
			OriginalFileName = file,
			EncryptedFileName = newName,
			Password = pass,
			Id = id,
			Urls = args.GetSwitch("urls")?.Split(urlsep).ToList(),
			Notes = args.GetSwitch("notes")?.Split(notesep).ToList()
		});
		Instance.SaveDb();

		if (args.HasSwitch("ren"))
		{
			try
			{
				File.Move(file, Path.Combine(fileDir, newName));
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
