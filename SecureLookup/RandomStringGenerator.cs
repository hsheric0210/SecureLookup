using System.Security.Cryptography;
using System.Text;

namespace SecureLookup;
internal static class RandomStringGenerator
{
	private const string LowerAlpha = "abcdefghijklmnopqrstuvwxyz";
	private const string UpperAlpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private const string Numeric = "0123456789";
	private const string LowerAlphaNumeric = LowerAlpha + Numeric;
	private const string UpperAlphaNumeric = UpperAlpha + Numeric;
	private const string AlphaNumeric = UpperAlpha + LowerAlpha + Numeric;

	public static string RandomString(int length, string dictionaryName)
	{
		var dictionary = GetDictionary(dictionaryName);
		var builder = new StringBuilder(length);
		for (var i = 0; i < length; i++)
			builder.Append(dictionary[RandomNumberGenerator.GetInt32(dictionary.Length)]);
		return builder.ToString();
	}

	private static string GetDictionary(string dictionaryName) => dictionaryName.ToLowerInvariant() switch
	{
		"loweralpha" => LowerAlpha,
		"upperalpha" => UpperAlpha,
		"numeric" => Numeric,
		"loweralphanumeric" => LowerAlphaNumeric,
		"upperalphanumeric" => UpperAlphaNumeric,
		"alphanumeric" => AlphaNumeric,
		_ => dictionaryName // use itself as dictionary
	};
}
