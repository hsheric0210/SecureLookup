using System.Reflection;
using System.Text;

namespace SecureLookup;
internal static class ParameterSerializer
{
	public static bool TryParse<T>(out T param, params string[] args)
	{
		Type targetType = typeof(T);
		ConstructorInfo? targetCtor = targetType.GetConstructor(Array.Empty<Type>());
		if (targetCtor is null)
			throw new TypeLoadException("Type " + targetType + " doesn't have a argument-less constructor.");
		param = (T)targetCtor.Invoke(Array.Empty<object>());

		// Process properties
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

	public static string GetHelpMessage<T>()
	{
		var builder = new StringBuilder();
		builder.AppendLine("Available parameters:");

		// Process properties
		foreach (PropertyInfo prop in typeof(T).GetProperties())
		{
			ICollection<string> names = prop.GetParameterNames();
			var optional = !prop.IsParameterMandatory();
			Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
			var propTypeName = propType.Name;

			builder.Append("  ");
			if (optional)
				builder.Append("  [");
			if (propType == typeof(bool))
				builder.AppendJoin(' ', names.Select(name => "-" + name));
			else
				builder.AppendJoin(' ', names.Select(name => $"-{name}<{propTypeName}>"));
			if (optional)
				builder.Append(']');
			var desc = GetParameterDescription(prop);
			if (!string.IsNullOrWhiteSpace(desc))
				builder.Append("    - ").Append(desc);
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

		names.Sort((string a, string b) => b.Length.CompareTo(a.Length)); // Descending order
		return names.Distinct().ToList();
	}

	private static string? GetParameterDescription(this MemberInfo info) => info.GetCustomAttribute<ParameterDescriptionAttribute>()?.Description;

	private static bool IsParameterMandatory(this MemberInfo info) => info.GetCustomAttribute<MandatoryParameterAttribute>() is not null;
}

[AttributeUsage(AttributeTargets.Property)]
public class ParameterAliasAttribute : Attribute
{
	public string[] Aliases { get; }

	public ParameterAliasAttribute(params string[] aliases) => Aliases = aliases;
}

[AttributeUsage(AttributeTargets.Property)]
public class ParameterDescriptionAttribute : Attribute
{
	public string Description { get; }

	public ParameterDescriptionAttribute(string description) => Description = description;
}

[AttributeUsage(AttributeTargets.Property)]
public class MandatoryParameterAttribute : Attribute { }
