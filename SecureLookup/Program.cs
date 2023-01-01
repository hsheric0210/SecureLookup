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

	[ParameterAlias("batch", "bf")]
	[ParameterDescription("Execute each lines of specified file as command AND EXIT. Remember to append 'save' at the last line to save all changes.")]
	public string? BatchFile { get; set; }

	[ParameterAlias("export", "ex")]
	[ParameterDescription("Export all entries to specified file AND EXIT.")]
	public string? ExportFile { get; set; }
}

public class Program
{
	private const string ConfigFileName = "config.xml";

	private bool loop;
	private readonly string dbFileName;
	internal Database EncryptedDb { get; private set; }

	public string DbFile { get; set; }
	public CommandFactory CommandFactory { get; }
	public DbInnerRoot Db { get; }
	public ConfigRoot Config { get; }

	public static void Main(params string[] args)
	{
		if (!ParameterDeserializer.TryParse(out ProgramParameter param, args))
		{
			Console.WriteLine(ParameterDeserializer.GetHelpMessage<ProgramParameter>());
			return;
		}

		var instance = new Program(
			param.Database,
			param.Password,
			param.DisableLoop != true);

		if (!string.IsNullOrEmpty(param.ExportFile))
		{
			try
			{
				instance.ExportDecrypted(param.ExportFile);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to export to: " + param.ExportFile);
				Console.WriteLine(ex);
			}
			return;
		}

		if (!string.IsNullOrWhiteSpace(param.BatchFile) && new FileInfo(param.BatchFile).Exists)
		{
			IList<string> lines;
			try
			{
				lines = File.ReadAllLines(param.BatchFile);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to read the batch file: " + param.BatchFile);
				Console.WriteLine(ex);
				Environment.Exit(1);
				return;
			}

			foreach (var line in lines)
				instance.Execute(line);
			return;
		}

		if (!string.IsNullOrWhiteSpace(param.Command))
		{
			instance.Execute(param.Command);
		}
		instance.Start();
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
			EncryptedDb = new Database(dbFile, Encoding.UTF8.GetBytes(password));
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

	private void ExportDecrypted(string dest)
	{
		using FileStream stream = File.Open(dest, FileMode.Create, FileAccess.Write, FileShare.Read);
		using var xw = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) });
		var serializer = new XmlSerializer(typeof(DbInnerRoot));
		serializer.Serialize(xw, Db);
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
		using var xw = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) });
		var serializer = new XmlSerializer(typeof(ConfigRoot));
		serializer.Serialize(xw, config);
	}

	private void Start() => MainLoop();

	public void SaveDb()
	{
		try
		{
			EncryptedDb.Save();
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

	private void Execute(string line)
	{
		var pieces = line.SplitOutsideQuotes(' ');
		Execute(pieces[0], pieces.Skip(1).ToArray());
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
			{
				Execute(linePieces[0], linePieces.Skip(1).ToArray());
				Console.WriteLine();
			}
		}
	}
}