namespace SecureLookup;
public static class DictionaryFileCache
{
	private static readonly IDictionary<string, string> cache = new Dictionary<string, string>();

	public static string ReadDictionary(string dictionaryFile)
	{
		var key = Path.GetFullPath(dictionaryFile);
		if (!cache.ContainsKey(key))
			cache[key] = string.Concat(File.ReadAllLines(dictionaryFile));
		return cache[key];
	}
}
