using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SecureLookup.Db;
using SecureLookup.Parameter;

namespace SecureLookup.Commands;

internal class CleanCommandParameter
{
	[ParameterAlias("ArchiveRepo", "Repo", "Destination", "Dest")]
	[ParameterDescription("The folder where the archive files will be stored; if you want to treat cwd as repository, use '.'")]
	[MandatoryParameter]
	public string ArchiveRepository { get; set; } = "";

	[ParameterDescription("Delete backup entries together")]
	public bool IncludeBackups { get; set; } = false;
}

internal class CleanCommand : AbstractCommand
{
	public override string HelpMessage => ParameterDeserializer.GetHelpMessage<CleanCommandParameter>("Cleaning parameters");

	public override string Description => "Cleans up generated name cachees and entries with reference to inexistent archives.";

	public CleanCommand(Program instance) : base(instance, "clean")
	{
	}

	protected override bool Execute(string[] args)
	{
		if (!ParameterDeserializer.TryParse(out CleanCommandParameter param, args))
			return false;

		var repo = Path.GetFullPath(param.ArchiveRepository);
		if (!new DirectoryInfo(repo).Exists)
		{
			Console.WriteLine("Archive repository directory not found: " + repo);
			return false;
		}

		var removedNames = DbRoot.GeneratedFileNames.RemoveWhere(name => !new FileInfo(Path.Combine(repo, name)).Exists);
		Console.WriteLine($"Deleted {removedNames} generated names.");

		var dangling = DbRoot.Entries.Where(entry => !new FileInfo(Path.Combine(repo, entry.ArchiveFileName)).Exists).ToList();
		var builder = new StringBuilder();
		builder.Append("Removing total ").Append(dangling.Count).AppendLine(" dangling entries.");
		foreach (DbEntry entry in dangling)
		{
			entry.AppendEntry(builder);
			DbRoot.Entries.Remove(entry);
		}
		builder.AppendLine();
		Console.WriteLine(builder.ToString());

		if (param.IncludeBackups)
		{
			var backups = DbRoot.Entries.Where(entry => ((DbEntryFlags)entry.Flags).HasFlag(DbEntryFlags.Backup)).ToList();
			builder.Clear();
			builder.Append("Removing total ").Append(backups.Count).AppendLine(" backup entries.");
			foreach (DbEntry entry in backups)
			{
				entry.AppendEntry(builder);
				DbRoot.Entries.Remove(entry);
			}
			builder.AppendLine();
			Console.WriteLine(builder.ToString());
		}

		Instance.Database.MarkDirty();
		return true;
	}
}
