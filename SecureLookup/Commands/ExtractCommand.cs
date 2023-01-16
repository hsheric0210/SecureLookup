using SecureLookup.Db;
using SecureLookup.Parameter;
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

	[ParameterAlias("parallel", "prl")]
	[ParameterDescription("Override the parallel execution limitation of unarchivers")]
	public int Parallelism { get; set; } = 8;

	[ParameterAlias("Block", "bl")]
	[ParameterDescription("Block command execution until all unarchiving is finished")]
	public bool BlockExecution { get; set; }
}

internal class ExtractCommand : AbstractFilterCommand
{
	public override string Description => "Extracts specified archive with specified unarchiver.";

	protected override string AdditionalHelpMessage => ParameterDeserializer.GetHelpMessage<OpenCommandParameter>("Extraction parameters");

	public ExtractCommand(Program instance) : base(instance, "extract")
	{
	}

	protected override bool ExecuteForEntries(string[] args, IList<DbEntry> entries)
	{
		if (!ParameterDeserializer.TryParse(out OpenCommandParameter param, args))
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
		Console.WriteLine($"*** Total {entries.Count} entries selected.");
		if (assumeYes && entries.Count > 1)
		{
			if (!ConsoleUtils.CheckContinue("Multiple entries are selected."))
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
				Console.WriteLine("Following file will be overwritten: " + target);
				if (!ConsoleUtils.CheckContinue())
					return true;
			}

			Console.WriteLine("Archive name: " + entry.Name);
			Console.WriteLine("Archive file-name: " + entry.ArchiveFileName);
			Console.WriteLine("Archive password: " + entry.Password);

			var sync = new SemaphoreSlim(param.Parallelism);
			taskQueue.Add(Task.Run(async () =>
			{
				try
				{
					await sync.WaitAsync();
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
				}
				finally
				{
					sync.Release();
				}
			}));
		}
		if (param.BlockExecution)
			Task.WhenAll(taskQueue).Wait();

		return true;
	}
}
