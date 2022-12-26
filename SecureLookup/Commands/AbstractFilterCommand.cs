using System.Text.RegularExpressions;

namespace SecureLookup.Commands;
internal abstract class AbstractFilterCommand : AbstractCommand
{
	protected AbstractFilterCommand(Program instance, string name) : base(instance, name)
	{
	}

	protected virtual string MandatoryParameters { get; } = "";
	protected virtual string OptionalParameters { get; } = "";

	protected override string ParameterExplain => $@"
  Mandatory parameters:
	-mode<(e)quals/(c)ontains/(s)tartsWith/e(n)dsWith/(r)egex>			- Search mode (alias: '-m')
	-target<(a)ll/(n)ame/(i)d/o(r)iginalFileName/(e)ncryptedFileName/(u)rls/n(o)tes>		- Search targets (alias: '-t')
	-keyword<keyword/regex>						- Keyword to search / Regex (alias: '-kw', '-k'){MandatoryParameters}
  Optional parameter:
	[-cs]		- Case sensitive search (Search is case insensitive by default){OptionalParameters}";

	protected override bool Execute(string[] args)
	{
		var mode = args.GetSwitches("mode", "m");
		var target = args.GetSwitches("target", "t");
		var keyword = args.GetSwitches("keyword", "kw", "k");
		if (string.IsNullOrWhiteSpace(mode) || string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(keyword))
			return false;
		Predicate<string>? pred = CreatePredicate(mode[0], keyword, args.HasSwitch("cs"));
		if (pred is null)
		{
			Console.WriteLine("Unsupported mode: " + mode);
			return false;
		}

		var targetChar = char.ToLowerInvariant(target[0]);
		var all = targetChar == 'a';
		try
		{
			ExecuteForEntries(Instance.Db.Entries.Where(entry =>
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
			Console.WriteLine("Invalid regex: " + keyword);
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

	protected abstract bool ExecuteForEntries(IList<XmlInnerEntry> entries);

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
