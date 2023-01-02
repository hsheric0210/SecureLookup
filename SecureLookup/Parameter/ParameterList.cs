namespace SecureLookup.Parameter;

internal class ParameterList
{
	private readonly IReadOnlyList<ParameterEntry> parameters;

	public ParameterList(IEnumerable<string> args) => parameters = args.Select(arg => new ParameterEntry(arg)).ToList();

	public ParameterList(string args) : this(args.SplitOutsideQuotes(' ')) { }

	private IReadOnlyList<ParameterEntry> GetUnhandledParameters() => parameters.Where(parameter => !parameter.IsHandled).ToList();

	public bool HasSwitch(string key) => GetUnhandledParameters().Any(parameter => parameter.IsSwitch(key));

	public string? GetSwitchValue(string key) => GetUnhandledParameters().Select(parameter => parameter.GetValue(key)).FirstOrDefault(parameter => parameter is not null);

	public bool HasSwitchRange(IEnumerable<string> switches) => switches.Any(theSwitch => HasSwitch(theSwitch));

	public string? GetValueRange(IEnumerable<string> switches) => switches.Select(theSwitch => GetSwitchValue(theSwitch)).FirstOrDefault(value => value is not null);

	private sealed record ParameterEntry(string Content)
	{
		private static readonly char[] switchPrefix = new char[] { '-', '/' };
		private static readonly char[] kvDelimiter = new char[] { ':', '=' };

		public bool IsHandled { get; private set; }

		private bool HasSwitchPrefix(string theSwitch) => switchPrefix.Any(prefix => Content.StartsWith(prefix)) && Content[1..].StartsWith(theSwitch, StringComparison.OrdinalIgnoreCase);

		internal bool IsSwitch(string theSwitch)
		{
			if (!HasSwitchPrefix(theSwitch))
				return false;
			IsHandled = true;
			return true;
		}

		internal string? GetValue(string theSwitch)
		{
			if (!HasSwitchPrefix(theSwitch))
				return null;
			IsHandled = true;
			var off = theSwitch.Length + 1;
			if (Content.Length >= off && kvDelimiter.Contains(Content[off])) // drop key-value delimiter
				off++;
			var value = Content[off..].Trim('\"');
			return string.IsNullOrWhiteSpace(value) ? null : value;
		}
	}
}
