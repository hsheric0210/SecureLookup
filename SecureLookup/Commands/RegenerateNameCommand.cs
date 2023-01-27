using SecureLookup.Db;
using SecureLookup.Parameter;
using System.Text;

namespace SecureLookup.Commands;

internal class RegenerateNameCommandParameter
{

	[ParameterAlias("ANameLen", "ALen", "AL")]
	[ParameterDescription("Length of generated archive file name")]
	public int ArchiveNameLength { get; set; } = 64;

	[ParameterAlias("ANameDictFile", "ADictF")]
	[ParameterDescription("The dictionary file to generate new archive name; You can use predefined dictionaries and Unicode dictionaries with file")]
	public string? ArchiveNameDictionaryFile { get; set; }

	[ParameterAlias("ANameDict", "ADict", "AD")]
	[ParameterDescription("Dictionary of generated archive file name; Predefined dictionary names are available at README")]
	public string? ArchiveNameDictionary { get; set; }

	[ParameterAlias("AName", "AN")]
	[ParameterDescription("Use specific archive name, instead of generated name; NOT ALLOWED IF MULTIPLE ARCHIVES MATCHED FILTER")]
	public string? ArchiveName { get; set; }

	[ParameterAlias("ArchiveRepo", "Repo", "Destination", "Dest")]
	[ParameterDescription("The folder where the archive files will be stored; if you want to treat cwd as repository, use '.'")]
	[MandatoryParameter]
	public string ArchiveRepository { get; set; } = "";
}

internal class RegenerateNameCommand : AbstractFilterCommand
{
	public override string Description => "Regenerate names and rename obfuscated files.";

	protected override string AdditionalHelpMessage => ParameterDeserializer.GetHelpMessage<RegenerateNameCommandParameter>("Name regeneration parameters");

	public RegenerateNameCommand(Program instance) : base(instance, "nameregen")
	{
	}

	protected override bool ExecuteForEntries(string[] args, IList<DbEntry> entries)
	{
		if (!ParameterDeserializer.TryParse(out RegenerateNameCommandParameter param, args))
			return false;

		if (entries.Count > 1 && !string.IsNullOrEmpty(param.ArchiveName))
		{
			Console.WriteLine("Specifying single unique name for multiple archives is not allowed.");
			return false;
		}

		if (new List<string?>()
		{
			param.ArchiveNameDictionary,
			param.ArchiveNameDictionaryFile,
			param.ArchiveName
		}.All(str => string.IsNullOrWhiteSpace(str)))
		{
			Console.WriteLine("You must specify one of these parameters: '-ArchiveNameDictionary' '-ArchiveName'");
			return false;
		}

		Instance.Database.MarkDirty();

		Console.WriteLine($"*** Total {entries.Count} obfuscated-names are renamed.");
		var builder = new StringBuilder();
		foreach (DbEntry entry in entries)
		{
			var fi = new FileInfo(Path.Combine(param.ArchiveRepository, entry.ArchiveFileName));
			if (!fi.Exists)
				continue;
			var generated = param.ArchiveName;
			generated ??= GenerateString(param.ArchiveNameLength, param.ArchiveRepository, param.ArchiveNameDictionary, param.ArchiveNameDictionaryFile);
			builder.AppendLine().AppendLine("***");
			builder.Append("Name: ").AppendLine(entry.Name);
			builder.Append("Encrypted archive name: ").Append(entry.ArchiveFileName).Append(" => ").AppendLine(generated);
			try
			{
				fi.MoveTo(Path.Combine(param.ArchiveRepository, generated));
				entry.ArchiveFileName = generated;
				builder.AppendLine("Status: Succeed");
			}
			catch (Exception ex)
			{
				builder.AppendLine("Status: Failed");
				builder.AppendLine(ex.ToString());
			}
			builder.AppendLine("***");
			Console.WriteLine(builder.ToString());
		}
		return true;
	}
}
