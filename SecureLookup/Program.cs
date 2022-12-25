using SecureLookup.Commands;
using System.Xml;

namespace SecureLookup;
public class Program
{
	private readonly string password;
	private bool loop;
	private readonly CommandFactory cmdFactory;

	public string DbFile { get; set; }
	public XmlDocument Db { get; }

	public static void Main(params string[] args)
	{
		var dbFile = args.GetSwitches("database", "db", "d");
		var pass = args.GetSwitches("password", "pass", "pw", "p");
		var command = args.GetSwitches("command", "cmd", "c");
		if (dbFile is null || pass is null)
		{
			Console.WriteLine("Usage: <executable> -db<Database> -pw<Password> [-cmd<Command>]");
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
			var pieces = command.Split('+');
			instance.Execute(pieces[0], pieces.Skip(1).ToArray());
		}
	}

	public Program(string dbFile, string password, bool loop)
	{
		DbFile = dbFile;
		this.password = password;
		try
		{
			if (new FileInfo(dbFile).Exists)
			{
				Db = XmlEncryptor.Load(dbFile, password);
			}
			else
			{
				Db = CreateDb();
				SaveDb();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception occurred while loading the database file.");
			Console.WriteLine(ex);
			Environment.Exit(ex.HResult);
		}

		this.loop = loop;
		cmdFactory = new CommandFactory(this);
	}

	public XmlDocument CreateDb()
	{
		var doc = new XmlDocument();
		doc.InsertBefore(doc.CreateXmlDeclaration("1.0", "utf-8", null), doc.DocumentElement);
		doc.AppendChild(doc.CreateElement("root"));
		return doc;
	}

	private void Start()
	{
		MainLoop();
	}

	public void SaveDb() => XmlEncryptor.Save(Db, DbFile, password);

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
			var linePieces = Console.ReadLine()?.Split(' ');
			if (linePieces is not null && linePieces.Length > 0)
			{
				Execute(linePieces[0], linePieces.Skip(1).ToArray());
			}
		}
	}
}