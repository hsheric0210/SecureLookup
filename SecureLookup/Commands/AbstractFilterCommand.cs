using SecureLookup.Db;
using SecureLookup.Parameter;
using System.Text;
using System.Text.RegularExpressions;

namespace SecureLookup.Commands;

internal class FilterCommandParameter
{
	[ParameterAlias("m")]
	[ParameterDescription("Filter mode: (E)quals/(C)ontains/(S)tartsWith/e(N)dsWith/(R)egex")]
	public char Mode { get; set; } = 'c';

	[ParameterAlias("t")]
	[ParameterDescription("Filter target: (A)ll/(N)ame/o(R)iginalFileName/(E)ncryptedFileName/(U)rls/n(O)tes")]
	public char Target { get; set; } = 'n';

	[ParameterAlias("kw", "k", "w")]
	[ParameterDescription("Filter keywords; separated in '-KeywordSeparator' char")]
	public string Keywords { get; set; } = "";

	[ParameterAlias("kws", "ks")]
	[ParameterDescription("Separator character to separate multiple keywords")]
	public char KeywordSeparator { get; set; } = ';';

	[ParameterAlias("casesens", "cs")]
	[ParameterDescription("Case sensitive search (Search is case insensitive by default, even regex)")]
	public bool CaseSensitive { get; set; }
}

internal abstract class AbstractFilterCommand : AbstractCommand
{
	protected AbstractFilterCommand(Program instance, string name) : base(instance, name)
	{
	}

	protected virtual string AdditionalHelpMessage { get; } = "";

	public override string HelpMessage => ParameterDeserializer.GetHelpMessage<FilterCommandParameter>("Filter parameters") + Environment.NewLine + AdditionalHelpMessage;

	protected override bool Execute(string[] args)
	{
		if (!ParameterDeserializer.TryParse(out FilterCommandParameter param, args))
			return false;
		Predicate<string>? pred = CreatePredicate(param.Mode, param.Keywords.Split(param.KeywordSeparator), param.CaseSensitive);
		if (pred is null)
		{
			Console.WriteLine("Unsupported mode: " + param.Mode);
			return false;
		}

		var targetChar = char.ToLowerInvariant(param.Target);
		var all = targetChar == 'a';
		try
		{
			return ExecuteForEntries(args, DbRoot.Entries.Where(entry =>
			{
				if ((all || targetChar == 'n') && pred(entry.Name))
					return true;
				if ((all || targetChar == 'r') && pred(entry.OriginalFileName))
					return true;
				if ((all || targetChar == 'e') && pred(entry.ArchiveFileName))
					return true;
				if ((all || targetChar == 'u') && entry.Urls?.Any(url => pred(url)) == true)
					return true;
				if ((all || targetChar == 'o') && entry.Notes?.Any(url => pred(url)) == true)
					return true;
				return false;
			}).ToList());
		}
		catch (RegexParseException ex)
		{
			Console.WriteLine("Invalid regex: " + param.Keywords);
			Console.WriteLine("Detail: " + ex.Message);
			return false;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception occurred during execution.");
			Console.WriteLine(ex);
			return false;
		}
	}

	protected abstract bool ExecuteForEntries(string[] args, IList<DbEntry> entries);

	private static Predicate<string>? CreatePredicate(char mode, IEnumerable<string> keywords, bool caseSens)
	{
		StringComparison cmp = caseSens ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
		return mode switch
		{
			'e' => (string str) => keywords.Any(kw => str.Equals(kw, cmp)),
			'c' => (string str) => keywords.Any(kw => str.Contains(kw, cmp)),
			's' => (string str) => keywords.Any(kw => str.StartsWith(kw, cmp)),
			'n' => (string str) => keywords.Any(kw => str.EndsWith(kw, cmp)),
			'r' => (string str) => keywords.Select(kw => new Regex(kw, caseSens ? RegexOptions.None : RegexOptions.IgnoreCase)).Any(r => r.Match(str).Success),
			_ => null
		};
	}

	protected void AppendEntry(StringBuilder builder, DbEntry entry)
	{
		builder.AppendLine().AppendLine("***");
		builder.Append("Name: ").AppendLine(entry.Name);
		builder.Append("Original file name: ").AppendLine(entry.OriginalFileName);
		builder.Append("Encrypted file name: ").AppendLine(entry.ArchiveFileName);
		builder.Append("Password: ").AppendLine(entry.Password);
		if (entry.Urls is not null && entry.Urls.Count > 0)
		{
			builder.AppendLine("Urls:");
			foreach (var url in entry.Urls)
				builder.Append("* ").AppendLine(url);
		}
		if (entry.Notes is not null && entry.Notes.Count > 0)
		{
			builder.AppendLine("Urls:");
			foreach (var notes in entry.Notes)
				builder.Append("* ").AppendLine(notes);
		}
		builder.AppendLine("***");
	}
}
