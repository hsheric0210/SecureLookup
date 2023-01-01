using System.Reflection;
using System.Text;

namespace SecureLookup;
internal static class ParameterDeserializer
{
	private static T? Instantize<T>() => (T?)typeof(T).GetConstructor(Array.Empty<Type>())?.Invoke(Array.Empty<object>());

	public static bool TryParse<T>(out T param, params string[] args)
	{
		Type targetType = typeof(T);
		T? instance = Instantize<T>();
		if (instance is null)
			throw new TypeLoadException($"Type {typeof(T)} doesn't have an argument-less constructor.");
		param = instance;

		foreach (PropertyInfo prop in targetType.GetProperties())
		{
			ICollection<string> names = GetParameterNames(prop);
			Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
			var value = propType == typeof(bool) ? args.HasSwitches(names.ToArray()) : (object?)args.GetSwitches(names.ToArray());

			object? convertedValue = null;
			if (value is not null)
			{
				try
				{
					convertedValue = Convert.ChangeType(value, propType);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to convert value of parameter ({prop.Name}=)'{value}' to {propType}");
					Console.WriteLine(ex);
					return false;
				}
			}

			if (value is not null && !(value is string str && string.IsNullOrEmpty(str)))
				prop.SetValue(param, convertedValue);
			else if (prop.IsParameterMandatory())
				return false; // One or more parameters marked as mandatory(not nullable) are not present
		}

		return true;
	}

	public static string GetHelpMessage<T>(string? customHeader = null)
	{
		var builder = new StringBuilder();
		builder.AppendLine(customHeader ?? "Available parameters:");

		foreach (PropertyInfo prop in typeof(T).GetProperties())
		{
			ICollection<string> names = prop.GetParameterNames();
			var optional = !prop.IsParameterMandatory();
			Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
			var propTypeName = propType.Name;

			builder.Append('\t');
			if (optional)
				builder.Append("  [");
			if (propType == typeof(bool))
				builder.AppendJoin(' ', names.Select(name => "-" + name));
			else
				builder.AppendJoin(' ', names.Select(name => $"-{name}<{propTypeName}>"));
			if (optional)
				builder.Append(']');
			builder.AppendLine();
			var desc = GetParameterDescription(prop);
			if (!string.IsNullOrWhiteSpace(desc))
				builder.Append("\t\t- ").Append(desc);
			if (optional)
			{
				T? instance = Instantize<T>();
				if (instance is not null)
				{
					var defValue = prop.GetValue(instance);
					if (defValue is not null)
						builder.AppendLine().Append("\t\t(Default: ").Append(defValue).Append(')');
				}
			}
			builder.AppendLine();
		}

		return builder.ToString();
	}

	private static ICollection<string> GetParameterNames(this MemberInfo info)
	{
		var names = new List<string>
		{
			info.Name
		};

		var attributes = (ParameterAliasAttribute[])info.GetCustomAttributes<ParameterAliasAttribute>(false);
		names.AddRange(attributes.SelectMany(attribute => attribute.Aliases.Select(alias => alias.ToLowerInvariant())));
		return names.Distinct().ToList();
	}

	private static string? GetParameterDescription(this MemberInfo info) => info.GetCustomAttribute<ParameterDescriptionAttribute>()?.Description;

	private static bool IsParameterMandatory(this MemberInfo info) => info.GetCustomAttribute<MandatoryParameterAttribute>() is not null;
}

static class ParameterSerializerExtension
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

#region Attributes
/// <summary>
/// Configure alias (sub-name) for specified parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ParameterAliasAttribute : Attribute
{
	public string[] Aliases { get; }

	public ParameterAliasAttribute(params string[] aliases) => Aliases = aliases;
}

/// <summary>
/// Configure description (usage, explanation, example, etc.) for specified parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ParameterDescriptionAttribute : Attribute
{
	public string Description { get; }

	public ParameterDescriptionAttribute(string description) => Description = description;
}

/// <summary>
/// Marker attribute that indicates the specified property is considered as mandatory parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class MandatoryParameterAttribute : Attribute { }
#endregion

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