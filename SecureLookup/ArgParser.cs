namespace SecureLookup;
public static class ArgParser
{
	private static readonly char[] switchPrefix = new char[] { '-', '/' };

	private static bool HasSwitchPrefix(this string arg, string theSwitch) => switchPrefix.Any(prefix => arg.StartsWith(prefix)) && arg[1..].StartsWith(theSwitch, StringComparison.OrdinalIgnoreCase);

	public static bool HasSwitch(this string[] args, string theSwitch) => args.Any(arg => arg.HasSwitchPrefix(theSwitch));

	public static bool HasSwitches(this string[] args, params string[] switches) => switches.Any(theSwitch => args.HasSwitch(theSwitch));

	public static string? GetSwitch(this string[] args, string theSwitch) => args.Where(arg => arg.HasSwitchPrefix(theSwitch)).Select(arg => arg[(theSwitch.Length + 1)..]).FirstOrDefault();

	public static string? GetSwitches(this string[] args, params string[] switches) => switches.Select(theSwitch => args.GetSwitch(theSwitch)).FirstOrDefault(switchValue => switchValue is not null);
}
