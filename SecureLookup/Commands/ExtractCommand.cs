using StringTokenFormatter;
using System.Diagnostics;
using System.Text;

namespace SecureLookup.Commands;
internal class OpenCommandParameter
{
	[ParameterAlias("Dest")]
	[ParameterDescription("The folder where the archive will be extracted; Warning: All contents will be extracted to THE SUBFOLDER in specified destination directory")]
	[MandatoryParameter]
	public string Destination { get; set; } = "";

	[ParameterAlias("ArchiveRepo", "Repo", "Source", "Src")]
	[ParameterDescription("The folder where the archive files are stored; same as original file or folder by default")]
	public string? ArchiveRepository { get; set; }

	[ParameterAlias("ay", "y")]
	[ParameterDescription("Assume yes to all user input prompts")]
	public bool AssumeAllYes { get; set; }

	[ParameterAlias("Block", "bl")]
	[ParameterDescription("Block command execution until all unarchiving is finished")]
	public bool BlockExecution { get; set; }
}

internal class ExtractCommand : AbstractFilterCommand
{
	public override string Description => "Open specified archive file with specified unarchiver.";

	protected override string AdditionalHelpMessage => ParameterSerializer.GetHelpMessage<OpenCommandParameter>("Extraction parameters");

	public ExtractCommand(Program instance) : base(instance, "extract")
	{
	}

	protected override bool ExecuteForEntries(string[] args, IList<DbEntry> entries)
	{
		if (!ParameterSerializer.TryParse(out OpenCommandParameter param, args))
			return false;

		var repo = param.ArchiveRepository;
		if (string.IsNullOrWhiteSpace(repo))
		{
			repo = Environment.CurrentDirectory; // cwd as repo
		}
		else if (!new DirectoryInfo(repo).Exists)
		{
			Console.WriteLine("Archive repository directory not exist: " + repo);
			return true;
		}
		else
		{
			repo = Path.GetFullPath(repo);
		}

		var dest = Path.GetFullPath(param.Destination);
		var destDir = new DirectoryInfo(dest);
		if (!destDir.Exists)
			destDir.Create();

		var assumeYes = param.AssumeAllYes;

		var builder = new StringBuilder();
		builder.Append("*** Total ").Append(entries.Count).AppendLine(" entries found.");
		if (assumeYes && entries.Count > 1)
		{
			Console.Write("Multiple entries are selected. ");
			if (!CheckContinue())
				return true;
		}

		var taskQueue = new List<Task>();

		foreach (DbEntry entry in entries)
		{
			var archive = Path.Combine(repo, entry.ArchiveFileName);
			if (!new FileInfo(archive).Exists)
			{
				Console.WriteLine("Archive not exists: " + archive);
				continue;
			}

			var target = Path.Combine(dest, entry.OriginalFileName);
			if (!assumeYes && new FileInfo(target).Exists)
			{
				Console.WriteLine("Target file will be overwritten: " + target);
				if (!CheckContinue())
					return true;
			}

			Console.WriteLine("Archive name: " + entry.Name);
			Console.WriteLine("Archive file-name: " + entry.ArchiveFileName);
			Console.WriteLine("Archive password: " + entry.Password);

			taskQueue.Add(Task.Run(async () =>
			{
				var process = new Process();
				process.StartInfo.FileName = Instance.Config.UnarchiverExecutable;
				process.StartInfo.Arguments = Instance.Config.UnarchiverParameter.FormatToken(new
				{
					Target = target,
					Archive = archive,
					entry.Password
				});
				process.StartInfo.WorkingDirectory = repo;
				process.StartInfo.UseShellExecute = true;
				process.Start();
				await process.WaitForExitAsync();
			}));
		}
		if (param.BlockExecution)
			Task.WhenAll(taskQueue).Wait();

		return true;
	}

	private static bool CheckContinue()
	{
		Console.Write("Do you want to continue? [Y/N]: ");
		return Console.ReadKey().Key == ConsoleKey.Y;
	}
}
