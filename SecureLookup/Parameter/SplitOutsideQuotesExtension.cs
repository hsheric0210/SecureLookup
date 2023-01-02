using System.Text;

namespace SecureLookup.Parameter;
#region SplitOutsideQuotes
/// <summary>SplitOutsideQuotes</summary>
/// <remarks>
/// <para>
/// C# extension methods to split the string by a separator char (or char array),
/// but only when a separator char is outside the quotes. Supports escaping the quote (\"),
/// so it is possible to use the quotes character itself inside the source string.
/// This solution does not use regex. Includes some additional options to fine tune split results.
/// </para>
/// <para>By Pavel Hruska - <see href="https://gist.github.com/mrpeardotnet/cba4338ffe01cb6e41d2765d8886aded">mrpeardotnet/SplitOutsideQuotes.cs</see></para>
/// </remarks>
public static class SplitOutsideQuotesExtension
{
	/// <summary>
	/// Splits the string by specified separator, but only when the separator is outside the quotes.
	/// </summary>
	/// <param name="source">The source string to separate.</param>
	/// <param name="splitChar">The character used to split strings.</param>
	/// <param name="trimSplits">If set to <c>true</c>, split strings are trimmed (whitespaces are removed).</param>
	/// <param name="ignoreEmptyResults">If set to <c>true</c>, empty split results are ignored (not included in the result).</param>
	/// <param name="preserveEscapeCharInQuotes">If set to <c>true</c>, then the escape character (\) used to escape e.g. quotes is included in the results.</param>
	public static string[] SplitOutsideQuotes(this string source, char separator, bool trimSplits = true, bool ignoreEmptyResults = true, bool preserveEscapeCharInQuotes = true) => source.SplitOutsideQuotes(new[] { separator }, trimSplits, ignoreEmptyResults, preserveEscapeCharInQuotes);

	/// <summary>
	/// Splits the string by specified separator, but only when the separator is outside the quotes.
	/// </summary>
	/// <param name="source">The source string to separate.</param>
	/// <param name="splitChars">The characters used to split strings.</param>
	/// <param name="trimSplits">If set to <c>true</c>, split strings are trimmed (whitespaces are removed).</param>
	/// <param name="ignoreEmptyResults">If set to <c>true</c>, empty split results are ignored (not included in the result).</param>
	/// <param name="preserveEscapeCharInQuotes">If set to <c>true</c>, then the escape character (\) used to escape e.g. quotes is included in the results.</param>
	public static string[] SplitOutsideQuotes(this string source, char[] separators, bool trimSplits = true, bool ignoreEmptyResults = true, bool preserveEscapeCharInQuotes = true)
	{
		var result = new List<string>();
		var escapeFlag = false;
		var quotesOpen = false;
		var currentItem = new StringBuilder();

		foreach (var currentChar in source)
		{
			if (escapeFlag)
			{
				currentItem.Append(currentChar);
				escapeFlag = false;
				continue;
			}

			if (separators.Contains(currentChar) && !quotesOpen)
			{
				var currentItemString = trimSplits ? currentItem.ToString().Trim() : currentItem.ToString();
				currentItem.Clear();
				if (string.IsNullOrEmpty(currentItemString) && ignoreEmptyResults)
					continue;
				result.Add(currentItemString);
				continue;
			}

			switch (currentChar)
			{
				default:
					currentItem.Append(currentChar);
					break;
				case '\\':
					if (quotesOpen && preserveEscapeCharInQuotes)
						currentItem.Append(currentChar);
					escapeFlag = true;
					break;
				case '"':
					currentItem.Append(currentChar);
					quotesOpen = !quotesOpen;
					break;
			}
		}

		if (escapeFlag)
			currentItem.Append('\\');

		var lastCurrentItemString = trimSplits ? currentItem.ToString().Trim() : currentItem.ToString();
		if (!(string.IsNullOrEmpty(lastCurrentItemString) && ignoreEmptyResults))
			result.Add(lastCurrentItemString);

		return result.ToArray();
	}
}
#endregion