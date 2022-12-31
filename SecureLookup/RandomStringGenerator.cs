using System.Security.Cryptography;
using System.Text;
using System.Reflection;

namespace SecureLookup;
internal static class RandomStringGenerator
{
	public const string LowerAlpha = "abcdefghijklmnopqrstuvwxyz";
	public const string UpperAlpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	public const string Numeric = "0123456789";
	public const string Special = "!#$%&'()*+,-./:;<=>?@[]^_`{}~";
	public const string LowerAlphaNumeric = LowerAlpha + Numeric;
	public const string UpperAlphaNumeric = UpperAlpha + Numeric;
	public const string MixedAlphaNumeric = UpperAlpha + LowerAlpha + Numeric;
	public const string SpecialMixedAlphaNumeric = LowerAlpha + Numeric + Special;

	public static string RandomString(int length, string dictionaryName)
	{
		var dictionary = GetDictionary(dictionaryName);
		var builder = new StringBuilder(length);
		for (var i = 0; i < length; i++)
			builder.Append(dictionary[RandomNumberGenerator.GetInt32(dictionary.Length)]);
		return builder.ToString();
	}

	private static string GetDictionary(string dictionaryName)
	{
		return dictionaryName.ToLowerInvariant() switch
		{
			"loweralpha" => LowerAlpha,
			"upperalpha" => UpperAlpha,
			"mixedalphanumeric" or "alphanumeric" => MixedAlphaNumeric,
			"numeric" => Numeric,
			"loweralphanumeric" => LowerAlphaNumeric,
			"upperalphanumeric" => UpperAlphaNumeric,
			"specialmixedalphanumeric" => SpecialMixedAlphaNumeric,
			_ => dictionaryName // use itself as dictionary
		};
	}
}
