using System.ComponentModel.DataAnnotations;
using System.Linq;
using SecureLookup.Parameter;

namespace SecureLookup.Commands;

internal class CleanCommandParameter
{
	[ParameterAlias("ArchiveRepo", "Repo", "Destination", "Dest")]
	[ParameterDescription("The folder where the archive files will be stored; if you want to treat cwd as repository, use '.'")]
	[MandatoryParameter]
	public string ArchiveRepository { get; set; } = "";
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

		int genNameCount = DbRoot.GeneratedFileNames.Count;
		DbRoot.GeneratedFileNames.Clear();
		Console.WriteLine($"Deleted {genNameCount} generated names.");

		var dangling = DbRoot.Entries.Where(entry => !new FileInfo(Path.Combine(repo, entry.ArchiveFileName)).Exists).ToList();
		Console.WriteLine($"Removing total {dangling.Count} dangling entries.");
		foreach (Db.DbEntry entry in dangling)
			DbRoot.Entries.Remove(entry);

		Instance.Database.MarkDirty();
		return true;
	}
}
