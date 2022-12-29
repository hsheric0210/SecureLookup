using SecureLookup.Commands;
using System.Text;

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
}

public class Program
{
	private bool loop;
	internal DbEncrypted EncryptedDb { get; private set; }

	public string DbFile { get; set; }
	public CommandFactory CommandFactory { get; }
	public DbInnerRoot Db { get; }
	public bool DbDirty { get; private set; }

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
		DbFile = dbFile;
		try
		{
			EncryptedDb = new DbEncrypted(dbFile, Encoding.UTF8.GetBytes(password));
			Db = new DbInnerRoot();
			if (new FileInfo(dbFile).Exists)
				Db = EncryptedDb.Load();
			else
				SaveDb();
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to load the database file. Mayby mismatched key?");
			Console.WriteLine(ex);
			Environment.Exit(ex.HResult);
		}

		this.loop = loop;
		CommandFactory = new CommandFactory(this);
	}

	public void MarkDbDirty() => DbDirty = true;

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

	public void Exit()
	{
		if (EncryptedDb.Dirty)
			SaveDb();
		loop = false;
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
			Console.Write(DbFile + ">");
			var linePieces = Console.ReadLine()?.SplitOutsideQuotes(' ');
			if (linePieces is not null && linePieces.Length > 0)
				Execute(linePieces[0], linePieces.Skip(1).ToArray());
		}
	}
}