namespace SecureLookup;
public static class PropertiesUtils
{
	public static string Serialize(IReadOnlyDictionary<string, string> props) => string.Join(';', props.Select(entry => entry.Key + "=" + entry.Value));

	public static IReadOnlyDictionary<string, string> Deserialize(string props)
	{
		return new Dictionary<string, string>(props.Split(';').Select(pair =>
		{
			var kv = pair.Split('=', 2);
			return new KeyValuePair<string, string>(kv[0], kv[1]);
		}));
	}
}
