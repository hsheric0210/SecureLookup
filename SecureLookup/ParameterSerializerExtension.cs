namespace SecureLookup;
public static class ParameterSerializerExtension
{
	private static readonly char[] switchPrefix = new char[] { '-', '/' };

	private static bool HasSwitchPrefix(this string arg, string theSwitch) => switchPrefix.Any(prefix => arg.StartsWith(prefix)) && arg[1..].StartsWith(theSwitch, StringComparison.OrdinalIgnoreCase);

	public static bool HasSwitch(this string[] args, string theSwitch) => args.Any(arg => arg.HasSwitchPrefix(theSwitch));

	public static bool HasSwitch(this string args, string theSwitch) => args.SplitOutsideQuotes(' ').HasSwitch(theSwitch);

	public static bool HasSwitches(this string[] args, params string[] switches) => switches.Any(theSwitch => args.HasSwitch(theSwitch));

	public static bool HasSwitches(this string args, string theSwitch) => args.SplitOutsideQuotes(' ').HasSwitches(theSwitch);

	public static string? GetSwitch(this string[] args, string theSwitch) => args
		.Where(arg => arg.HasSwitchPrefix(theSwitch))
		.Select(arg =>
		{
			var part = arg[(theSwitch.Length + 1)..];
			if (part.Length > 0 && (part[0] == ':' || part[0] == '=')) // support paramName and paramValue separator (ex: '-d:db.xml' or '-d=db.xml')
				part = part[1..];
			return part.Trim('\"');
		})
		.FirstOrDefault();

	public static string? GetSwitch(this string args, string theSwitch) => args.SplitOutsideQuotes(' ').GetSwitch(theSwitch);

	public static string? GetSwitches(this string[] args, params string[] switches) => switches.Select(theSwitch => args.GetSwitch(theSwitch)).FirstOrDefault(switchValue => switchValue is not null);

	public static string? GetSwitches(this string args, params string[] switches) => args.SplitOutsideQuotes(' ').GetSwitches(switches);
}
