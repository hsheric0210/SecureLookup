using SecureLookup.Commands;
using SecureLookup.Db;
using SecureLookup.Parameter;
using StringTokenFormatter;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SecureLookup;

public class Program
{
	private const string ConfigFileName = "config.xml";

	private bool loop;
	private readonly string dbFileName;
	public Database Database { get; }

	public string DbPath { get; set; }
	public CommandFactory CommandFactory { get; }
	public ConfigRoot Config { get; }

	public ISet<string> BatchCompresingFiles { get; set; } = new HashSet<string>();

	public static void Main(params string[] args)
	{
		if (!ParameterDeserializer.TryParse(out ProgramParameter param, args))
		{
			Console.WriteLine(ParameterDeserializer.GetHelpMessage<ProgramParameter>("Common parameters:"));
			Console.WriteLine(ParameterDeserializer.GetHelpMessage<DatabaseCreationParameter>("Database creation parameters (Only applies when opening an inexistent database file):"));
			return;
		}

		Program? instance;
		var dbPath = Path.GetFullPath(param.Database);
		var dbCreation = !new FileInfo(dbPath).Exists;
		if (dbCreation)
		{
			if (ParameterDeserializer.TryParse(out DatabaseCreationParameter cparam, args))
			{
				// Create
				instance = new Program(dbPath, !param.DisableLoop, param.Password, cparam);
			}
			else
			{
				Console.WriteLine(ParameterDeserializer.GetHelpMessage<DatabaseCreationParameter>("Database creation parameters:"));
				return;
			}
		}
		else
		{
			// Load
			instance = new Program(dbPath, !param.DisableLoop, param.Password);
		}

		if (instance is null)
			return;

		if (!dbCreation && !string.IsNullOrEmpty(param.ExportFile))
		{
			try
			{
				instance.Database.Export(param.ExportFile);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to export as: " + param.ExportFile);
				Console.WriteLine(ex);
			}
			return;
		}

		if (!string.IsNullOrWhiteSpace(param.BatchFile) && new FileInfo(param.BatchFile).Exists)
		{
			instance.Backup();
			instance.BatchExecute(param.BatchFile);
			instance.Exit(false);
			return;
		}

		if (!string.IsNullOrWhiteSpace(param.Command))
		{
			instance.Execute(param.Command);
		}
		instance.Start();
	}

	/// <summary>
	/// Loads the specified already-existing database file
	/// </summary>
	/// <param name="dbFile">Full path to source database file</param>
	/// <param name="loop">Enable command loop</param>
	/// <param name="password">The database encryption password</param>
	public Program(string dbFile, bool loop, string password) : this(dbFile, loop)
	{
		try
		{
			Database = DatabaseLoader.Load(DbPath, Encoding.UTF8.GetBytes(password));
		}
		catch (Exception ex)
		{
			Console.WriteLine("Database loading failure.");
			Console.WriteLine(ex);
			Environment.Exit(ex.HResult);
		}
	}

	/// <summary>
	/// Creates a new database from scratch
	/// </summary>
	/// <param name="dbPath">Full path to destination database file</param>
	/// <param name="loop">Enable command loop</param>
	/// <param name="password">The initial database encryption password</param>
	/// <param name="parameter">Additional database creation parameter</param>
	public Program(string dbPath, bool loop, string password, DatabaseCreationParameter parameter) : this(dbPath, loop)
	{
		try
		{
			Database = DatabaseCreator.Create(dbPath, Encoding.UTF8.GetBytes(password), parameter);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Database creation failure.");
			Console.WriteLine(ex);
			Environment.Exit(ex.HResult);
		}
	}

	private Program(string dbPath, bool loop)
	{
		DbPath = dbPath;
		dbFileName = Path.GetFileName(DbPath);

		try
		{
			Config = new ConfigRoot();
			if (new FileInfo(ConfigFileName).Exists)
				Config = LoadConfig();
			else
				WriteConfig(Config);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to load the configuration file.");
			Console.WriteLine(ex);
			Environment.Exit(ex.HResult);
		}
		this.loop = loop;
		CommandFactory = new CommandFactory(this);
	}

	/// <summary>
	/// Loads configuration file and deserializes it as <see cref="ConfigRoot"/> DTO
	/// </summary>
	/// <returns>The deserialized <see cref="ConfigRoot"/> DTO object</returns>
	private static ConfigRoot LoadConfig()
	{
		using FileStream stream = File.Open(ConfigFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		var serializer = new XmlSerializer(typeof(ConfigRoot));
		return (ConfigRoot)serializer.Deserialize(stream)!;
	}

	/// <summary>
	/// Writes the configuration file
	/// </summary>
	/// <param name="config">Deserialized <see cref="ConfigRoot"/> DTO to write</param>
	private static void WriteConfig(ConfigRoot config)
	{
		using FileStream stream = File.Open(ConfigFileName, FileMode.Create, FileAccess.Write, FileShare.None);
		using var xw = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) });
		var serializer = new XmlSerializer(typeof(ConfigRoot));
		serializer.Serialize(xw, config);
	}

	public void Backup()
	{
		try
		{
			var inf = new FileInfo(DbPath);
			if (inf.Exists)
				inf.CopyTo($"{inf.FullName}.{DateTime.Now:yyyy-MM-dd-HH-mm-ss.ffff}.bak");
		}
		catch
		{
			// ignored
		}
	}

	private void BatchExecute(string batchFile)
	{
		IList<string> lines;
		try
		{
			lines = File.ReadAllLines(batchFile);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to read the batch file: " + batchFile);
			Console.WriteLine(ex);
			Environment.Exit(1);
			return;
		}

		foreach (var line in lines)
			Execute(line);
	}

	private void Start() => MainLoop();

	/// <summary>
	/// Saves the database, with internal exception handling.
	/// </summary>
	public void SaveDb()
	{
		try
		{
			Database.Save();
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to save the database file.");
			Console.WriteLine(ex);
		}
	}

	/// <summary>
	/// Exits the command loop
	/// </summary>
	/// <param name="discard">Don't save the database before exiting</param>
	public void Exit(bool discard)
	{
		loop = false;
		if (!discard && Database.Dirty)
			SaveDb();
		if (BatchCompresingFiles.Count > 0)
		{
			if (!string.IsNullOrWhiteSpace(Config.BatchArchiverExecutable) && new FileInfo(Config.BatchArchiverExecutable).Exists)
			{
				Task.Run(() =>
				{
					foreach (var batch in BatchCompresingFiles)
					{
						try
						{
							var proc = new Process();
							proc.StartInfo.FileName = Config.BatchArchiverExecutable;
							proc.StartInfo.Arguments = Config.BatchArchiverParameter.FormatToken(new { BatchFile = batch });
							proc.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
							proc.StartInfo.UseShellExecute = true;
							proc.Start();
							proc.WaitForExit();
							Shell32.MoveToRecycleBin(batch);
						}
						catch (Exception ex)
						{
							Console.WriteLine("Failed to finish batch archiving");
							Console.WriteLine(ex);
						}
					}
				}).Wait();
			}
			else
			{
				Console.WriteLine("Batch archiver not found: " + Config.BatchArchiverExecutable);
			}
		}
	}

	/// <summary>
	/// Executes the specified command with parameters
	/// </summary>
	/// <param name="line">The full command line</param>
	private void Execute(string line)
	{
		var pieces = line.SplitOutsideQuotes(' ');
		Execute(pieces[0], pieces.Skip(1).ToArray());
	}

	/// <summary>
	/// Executes the specified command with parameters
	/// </summary>
	/// <param name="cmdString">Command to execute in string</param>
	/// <param name="args">Command parameters</param>
	private void Execute(string cmdString, string[] args)
	{
		AbstractCommand? cmd = CommandFactory.FindCommand(cmdString);
		if (cmd is not null)
		{
			try
			{
				cmd.TryExecute(args);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception thrown during command execution!");
				Console.WriteLine(ex);
			}

		}
		else
			Console.WriteLine($"Command '{cmdString}' not found.");
	}

	/// <summary>
	/// Main command loop to interactively handle user command inputs
	/// </summary>
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