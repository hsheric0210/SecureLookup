using SecureLookup.Commands;
using System.Text;
using System.Xml;

namespace SecureLookup;
public class Program
{
	private readonly string password;
	private bool loop;
	private readonly CommandFactory cmdFactory;

	public string DbFile { get; set; }
	public XmlOuterDb Outer { get; }
	public XmlInnerRootEntry Db { get; }

	public static void Main(params string[] args)
	{
		var dbFile = args.GetSwitches("database", "db", "d");
		var pass = args.GetSwitches("password", "pass", "pw", "p");
		var command = args.GetSwitches("command", "cmd", "c");
		if (dbFile is null || pass is null)
		{
			Console.WriteLine(@"Available parameters:
  Mandatory parameters:
	-db<Database>		- Database file
	-pw<Password>		- Database unlock password
  Optional parameters:
	[-cmd<Command>]		- Command to run immediately after database open
	[-nl]			- Disable the main loop; exit immediately after executing the command specified with '-cmd' switch");
			return;
		}

		var instance = new Program(
			dbFile,
			pass,
			!args.HasSwitches("nl", "noloop", "l-"));
		instance.Start();

		if (command is not null)
		{
			// command by parameter is separated by '+' character
			var pieces = command.SplitOutsideQuotes(' ');
			instance.Execute(pieces[0], pieces.Skip(1).ToArray());
		}
	}

	public Program(string dbFile, string password, bool loop)
	{
		DbFile = dbFile;
		this.password = password;
		try
		{
			Outer = new XmlOuterDb(dbFile, Encoding.UTF8.GetBytes(password));
			Db = new XmlInnerRootEntry();
			if (new FileInfo(dbFile).Exists)
				Db = Outer.Load();
			else
				SaveDb();
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to load the database file.");
			Console.WriteLine(ex);
			Environment.Exit(ex.HResult);
		}

		this.loop = loop;
		cmdFactory = new CommandFactory(this);
	}

	private void Start()
	{
		MainLoop();
	}

	public void SaveDb()
	{
		try
		{
			Outer.Save(Db);
		}
		catch(Exception ex)
		{
			Console.WriteLine("Failed to save the database file.");
			Console.WriteLine(ex);
		}
	}

	public void Exit() => loop = false;

	private void Execute(string cmdString, string[] args)
	{
		AbstractCommand? cmd = cmdFactory.FindCommand(cmdString);
		if (cmd is not null)
			cmd.TryExecute(args);
		else
			Console.WriteLine($"Command '{cmdString}' not found.");
	}

	private void MainLoop()
	{
		while (loop)
		{
			Console.Write(DbFile + ">");
			var linePieces = Console.ReadLine()?.SplitOutsideQuotes(' ');
			if (linePieces is not null && linePieces.Length > 0)
			{
				Execute(linePieces[0], linePieces.Skip(1).ToArray());
			}
		}
	}
}