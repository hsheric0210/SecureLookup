using System.Text;

namespace SecureLookup.Commands;

internal class EditCommandParameter
{
	[ParameterDescription("Name of the entry; Entry with same name will be overwritten")]
	public string Name { get; set; } = "";

	[ParameterAlias("OriginalName", "OName")]
	[ParameterDescription("The name of original file/folder")]
	public string OriginalFileName { get; set; } = "";

	[ParameterAlias("ArchiveName", "AName")]
	[ParameterDescription("The name of archive file")]
	public string? ArchiveFileName { get; set; }

	[ParameterAlias("Pass", "PSW", "PW")]
	[ParameterDescription("User-specified file encryption password; character '|' is disallowed due its usage as separator")]
	public string? Password { get; set; }

	[ParameterDescription("Additional associated URLs separated in ';' char by default; the separator char could be reassigned with '-UrlSeparator' parameter")]
	public string? Urls { get; set; }

	[ParameterAlias("UrlSep")]
	[ParameterDescription("URL separator char to separate URLs in '-Urls' parameter")]
	public string UrlSeparator { get; set; } = ";";

	[ParameterDescription("Additional associated notes separated in ';' char by default; the separator char could be reassigned with '-NoteSeparator' parameter")]
	public string? Notes { get; set; }

	[ParameterAlias("NoteSep")]
	[ParameterDescription("Note separator char to separate notes in '-Notes' parameter")]
	public string NoteSeparator { get; set; } = ";";
}

internal class EditCommand : AbstractFilterCommand
{
	public override string Description => "Removes the entries matching the filter from the database.";

	protected override string AdditionalHelpMessage => ParameterSerializer.GetHelpMessage<EditCommandParameter>("Edit parameters");

	public EditCommand(Program instance) : base(instance, "edit")
	{
	}

	protected override bool ExecuteForEntries(string[] args, IList<DbEntry> entries)
	{
		if (!ParameterSerializer.TryParse(out EditCommandParameter param, args))
			return false;

		if (new List<string?>()
		{
			param.Name,
			param.OriginalFileName,
			param.ArchiveFileName,
			param.Password,
			param.Urls,
			param.Notes
		}.All(str => string.IsNullOrWhiteSpace(str)))
		{
			Console.WriteLine("You must specify one of these parameters: '-Name' '-OriginalFileName' '-ArchiveFileName' '-Password' '-Urls' '-Notes'");
			return false;
		}

		Instance.EncryptedDb.MarkDirty();

		Console.WriteLine($"*** Total {entries.Count} entries selected.");
		if (entries.Count > 1)
		{
			if (!string.IsNullOrWhiteSpace(param.Name))
			{
				Console.WriteLine("Can't rename mutiple entries to the same name because duplicated names are not allowed");
				return true;
			}
			else
			{
				if (!ConsoleUtils.CheckContinue("Multiple entries are selected."))
					return true;
			}
		}

		var builder = new StringBuilder();
		foreach (DbEntry entry in entries)
		{
			builder.AppendLine().AppendLine("***");
			ChangeString(builder, "Name", () => entry.Name, value => entry.Name = value, param.Name);
			ChangeString(builder, "Original file/folder name", () => entry.OriginalFileName, value => entry.OriginalFileName = value, param.OriginalFileName);
			ChangeString(builder, "Encrypted archive name", () => entry.OriginalFileName, value => entry.OriginalFileName = value, param.OriginalFileName);
			ChangeString(builder, "Password", () => entry.Password, value => entry.Password = value, param.Password);

			Change<List<string>?>(builder, "Urls", () => entry.Urls, value => entry.Urls = value, param.Urls?.Split(param.UrlSeparator).ToList(), toString: value => value is null ? null : string.Join(param.UrlSeparator, value));
			Change<List<string>?>(builder, "Notes", () => entry.Notes, value => entry.Notes = value, param.Notes?.Split(param.NoteSeparator).ToList(), toString: value => value is null ? null : string.Join(param.NoteSeparator, value));
			builder.AppendLine("***");
			Console.WriteLine(builder.ToString());
		}
		return true;
	}

	public static void Change<T>(StringBuilder builder, string name, Func<T> getter, Action<T> setter, T? newValue, Func<T?, bool>? valueValidation = null, Func<T, string?>? toString = null)
	{
		builder.Append(name).Append(": ").Append(toString?.Invoke(getter()) ?? getter()?.ToString());
		if (valueValidation?.Invoke(newValue) ?? (newValue is not null))
		{
			builder.Append(" -> ").Append(toString?.Invoke(getter()) ?? newValue?.ToString());
			setter(newValue!);
		}
		builder.AppendLine();
	}

	public static void ChangeString(StringBuilder builder, string name, Func<string> getter, Action<string> setter, string? newValue) => Change(builder, name, getter, setter, newValue, value => !string.IsNullOrWhiteSpace(value));
}
