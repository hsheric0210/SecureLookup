﻿using SecureLookup.Db;
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

	[ParameterAlias("Async")]
	[ParameterDescription("Don't wait until all unarchiving task is finished; You shouldn't enable this parameter on Batch Files")]
	public bool NonBlocking { get; set; }
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
		if (!assumeYes && entries.Count > 1)
		{
			if (!ConsoleUtils.CheckContinue("Multiple entries are selected."))
				return true;
		}
		Console.WriteLine("Parallelism is " + param.Parallelism);

		var taskQueue = new List<Task>();
		var funcQueue = new List<Action>();

		using var sync = new SemaphoreSlim(param.Parallelism);
		var mainTask = Task.Run(() =>
		{
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
						break;
				}

				Console.WriteLine("Archive name: " + entry.Name);
				Console.WriteLine("Archive file-name: " + entry.ArchiveFileName);
				Console.WriteLine("Archive password: " + entry.Password);

				sync.Wait();
				taskQueue.Add(Task.Run(async () =>
				{
					try
					{
						var process = new Process();
						process.StartInfo.FileName = Instance.Config.UnarchiverExecutable;
						process.StartInfo.Arguments = Instance.Config.UnarchiverParameter.FormatToken(new
						{
							Target = target,
							Archive = archive,
							entry.Password
						});
						Console.WriteLine("Extracting with Executable: " + process.StartInfo.FileName);
						Console.WriteLine("Extracting with Parameter: " + process.StartInfo.Arguments);
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
		});

		if (!param.NonBlocking)
		{
			mainTask.Wait();
			Task.WhenAll(taskQueue).Wait();
		}

		return true;
	}
}
