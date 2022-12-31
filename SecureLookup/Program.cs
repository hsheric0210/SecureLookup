using SecureLookup.Commands;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SecureLookup;

public class ProgramParameter
{
	[ParameterAlias("db", "d")]
	[ParameterDescription("The database file to use")]
	[MandatoryParameter]
	public string Database { get; set; } = "";

	[ParameterAlias("pass", "psw", "pw", "p")]
	[ParameterDescription("The password to open the database")]
	[MandatoryParameter]
	public string Password { get; set; } = "";

	[ParameterAlias("cmd", "c")]
	[ParameterDescription("The command that will run immediately after database loaded")]
	public string? Command { get; set; }

	[ParameterAlias("noloop", "nl")]
	[ParameterDescription("Disable the main loop. The program will immediately exit after executing the command specified by '-command' parameter.")]
	public bool? DisableLoop { get; set; }

	// TODO
	[ParameterAlias("batch", "bf")]
	[ParameterDescription("Execute each lines of specified file as command AND EXIT. Remember to append 'save' at the last line to save all changes.")]
	public string? BatchFile { get; set; }

	// TODO
	[ParameterAlias("export", "ex")]
	[ParameterDescription("Export all entries to specified file AND EXIT.")]
	public string? ExportFile { get; set; }
}

public class Program
{
	private const string ConfigFileName = "config.xml";

	private bool loop;
	private readonly string dbFileName;
	internal DbEncrypted EncryptedDb { get; private set; }

	public string DbFile { get; set; }
	public CommandFactory CommandFactory { get; }
	public DbInnerRoot Db { get; }
	public ConfigRoot Config { get; }

	public static void Main(params string[] args)
	{
		if (!ParameterSerializer.TryParse(out ProgramParameter param, args))
		{
			Console.WriteLine(ParameterSerializer.GetHelpMessage<ProgramParameter>());
			return;
		}

		var instance = new Program(
			param.Database,
			param.Password,
			param.DisableLoop != true);
		instance.Start();

		if (!string.IsNullOrWhiteSpace(param.Command))
		{
			// command by parameter is separated by '+' character
			var pieces = param.Command.SplitOutsideQuotes(' ');
			instance.Execute(pieces[0], pieces.Skip(1).ToArray());
		}
	}

	public Program(string dbFile, string password, bool loop)
	{
		try
		{
			Config = new ConfigRoot();
			if (new FileInfo(ConfigFileName).Exists)
				Config = LoadConfig();
			else
				WriteDefaultConfig(Config);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to load the configuration file.");
			Console.WriteLine(ex);
			Environment.Exit(ex.HResult);
		}

		try
		{
			DbFile = Path.GetFullPath(dbFile);
			dbFileName = Path.GetFileName(DbFile);
			EncryptedDb = new DbEncrypted(dbFile, Encoding.UTF8.GetBytes(password));
			Db = new DbInnerRoot();
			if (new FileInfo(dbFile).Exists)
				Db = EncryptedDb.Load();
			else
				SaveDb();
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to load the database file. Maybe mismatched key?");
			Console.WriteLine(ex);
			Environment.Exit(ex.HResult);
		}

		this.loop = loop;
		CommandFactory = new CommandFactory(this);
	}

	private static ConfigRoot LoadConfig()
	{
		using FileStream stream = File.Open(ConfigFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		var serializer = new XmlSerializer(typeof(ConfigRoot));
		return (ConfigRoot)serializer.Deserialize(stream)!;
	}

	private static void WriteDefaultConfig(ConfigRoot config)
	{
		using FileStream stream = File.Open(ConfigFileName, FileMode.Create, FileAccess.Write, FileShare.None);
		using var xw = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true });
		var serializer = new XmlSerializer(typeof(ConfigRoot));
		serializer.Serialize(xw, config);
	}

	public void ChangePassword(string newPassword)
	{
		var newOuter = new DbEncrypted(DbFile, Encoding.UTF8.GetBytes(newPassword));
		newOuter.Save(Db);
		EncryptedDb = newOuter;
	}

	private void Start() => MainLoop();

	public void SaveDb()
	{
		try
		{
			EncryptedDb.Save(Db);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to save the database file.");
			Console.WriteLine(ex);
		}
	}

	public void Exit(bool discard)
	{
		loop = false;
		if (!discard && EncryptedDb.Dirty)
			SaveDb();
	}

	private void Execute(string cmdString, string[] args)
	{
		AbstractCommand? cmd = CommandFactory.FindCommand(cmdString);
		if (cmd is not null)
			cmd.TryExecute(args);
		else
			Console.WriteLine($"Command '{cmdString}' not found.");
	}

	private void MainLoop()
	{
		while (loop)
		{
			Console.Write(dbFileName + ">");
			var linePieces = Console.ReadLine()?.SplitOutsideQuotes(' ');
			if (linePieces is not null && linePieces.Length > 0)
				Execute(linePieces[0], linePieces.Skip(1).ToArray());
		}
	}
}