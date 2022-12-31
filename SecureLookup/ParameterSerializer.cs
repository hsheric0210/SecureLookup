using System.Reflection;
using System.Text;

namespace SecureLookup;
internal static class ParameterSerializer
{
	private static T? Instantize<T>() => (T?)typeof(T).GetConstructor(Array.Empty<Type>())?.Invoke(Array.Empty<object>());

	public static bool TryParse<T>(out T param, params string[] args)
	{
		Type targetType = typeof(T);
		T? instance = Instantize<T>();
		if (instance is null)
			throw new TypeLoadException($"Type {typeof(T)} doesn't have an argument-less constructor.");
		param = instance;

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

	public static string GetHelpMessage<T>(string? customHeader = null)
	{
		var builder = new StringBuilder();
		builder.AppendLine(customHeader ?? "Available parameters:");

		// Process properties
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
