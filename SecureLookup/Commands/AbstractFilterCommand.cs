using System.Text.RegularExpressions;

namespace SecureLookup.Commands;

internal class FilterCommandParameter
{
	[ParameterAlias("m")]
	[ParameterDescription("Filter mode: (e)quals/(c)ontains/(s)tartsWith/e(n)dsWith/(r)egex")]
	[MandatoryParameter]
	public char Mode { get; set; } = 'e';

	[ParameterAlias("t")]
	[ParameterDescription("Filter target: (a)ll/(n)ame/(i)d/o(r)iginalFileName/(e)ncryptedFileName/(u)rls/n(o)tes")]
	[MandatoryParameter]
	public char Target { get; set; } = 'n';

	[ParameterAlias("kw", "k", "w")]
	[ParameterDescription("Filter keyword")]
	[MandatoryParameter]
	public string Keyword { get; set; } = "";

	[ParameterAlias("casesens", "cs")]
	[ParameterDescription("Case sensitive search (Search is case insensitive by default, even regex)")]
	[MandatoryParameter]
	public bool? CaseSensitive { get; set; }
}

internal abstract class AbstractFilterCommand : AbstractCommand
{
	protected AbstractFilterCommand(Program instance, string name) : base(instance, name)
	{
	}

	protected virtual string AdditionalHelpMessage { get; } = "";

	public override string HelpMessage => ParameterSerializer.GetHelpMessage<FilterCommandParameter>() + Environment.NewLine + AdditionalHelpMessage;

	protected override bool Execute(string[] args)
	{
		if (!ParameterSerializer.TryParse(out FilterCommandParameter param, args))
			return false;
		Predicate<string>? pred = CreatePredicate(param.Mode, param.Keyword, args.HasSwitch("cs"));
		if (pred is null)
		{
			Console.WriteLine("Unsupported mode: " + param.Mode);
			return false;
		}

		var targetChar = char.ToLowerInvariant(param.Target);
		var all = targetChar == 'a';
		try
		{
			ExecuteForEntries(args, Instance.Db.Entries.Where(entry =>
			{
				if ((all || targetChar == 'n') && pred(entry.Name))
					return true;
				if ((all || targetChar == 'i') && !string.IsNullOrWhiteSpace(entry.Id) && pred(entry.Id))
					return true;
				if ((all || targetChar == 'r') && pred(entry.OriginalFileName))
					return true;
				if ((all || targetChar == 'e') && pred(entry.EncryptedFileName))
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
			Console.WriteLine("Invalid regex: " + param.Keyword);
			Console.WriteLine("Detail: " + ex.Message);
			return false;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception occurred during execution.");
			Console.WriteLine(ex);
			return false;
		}

		return true;
	}

	protected abstract bool ExecuteForEntries(string[] args, IList<DbEntry> entries);

	private static Predicate<string>? CreatePredicate(char mode, string keyword, bool caseSens)
	{
		StringComparison cmp = caseSens ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
		return mode switch
		{
			'e' => (string str) => str.Equals(keyword, cmp),
			'c' => (string str) => str.Contains(keyword, cmp),
			's' => (string str) => str.StartsWith(keyword, cmp),
			'n' => (string str) => str.EndsWith(keyword, cmp),
			'r' => (string str) => new Regex(keyword, caseSens ? RegexOptions.None : RegexOptions.IgnoreCase).Match(str).Success,
			_ => null
		};
	}
}
