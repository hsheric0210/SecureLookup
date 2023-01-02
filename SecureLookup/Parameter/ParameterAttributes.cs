namespace SecureLookup.Parameter;
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
