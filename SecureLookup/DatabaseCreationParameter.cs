using SecureLookup.Parameter;

namespace SecureLookup;
public class DatabaseCreationParameter
{
	[ParameterAlias("phashalg")]
	[ParameterDescription("Primary password hashing algorithm")]
	public string PrimaryPasswordHashingAlgorithm { get; set; } = "Argon2id";

	[ParameterAlias("phashprop")]
	[ParameterDescription("Primary password hashing properties in '<key>=<value>;<key>=<value>;<key>=<value>...' format; For futher information, see README")]
	public string? PrimaryPasswordHashingProperties { get; set; }

	[ParameterAlias("phashsize")]
	[ParameterDescription("Primary password hash length")]
	public int PrimaryPasswordHashSize { get; set; } = 32;

	[ParameterAlias("shashalg")]
	[ParameterDescription("Secondary password hashing algorithm")]
	public string SecondaryPasswordHashingAlgorithm { get; set; } = "Argon2id";

	[ParameterAlias("shashprop")]
	[ParameterDescription("Secondary password hashing properties in '<key>=<value>;<key>=<value>;<key>=<value>...' format; For futher information, see README")]
	public string? SecondaryPasswordHashingProperties { get; set; }

	[ParameterAlias("compressalg", "calg")]
	[ParameterDescription("Database compression algorithm")]
	public string DatabaseCompressionAlgorithm { get; set; } = "PPMd";

	[ParameterAlias("compressprop", "cprop")]
	[ParameterDescription("Database compression properties in '<key>=<value>;<key>=<value>;<key>=<value>...' format; For futher information, see README")]
	public string? DatabaseCompressionProperties { get; set; }

	[ParameterAlias("dhashalg")]
	[ParameterDescription("Database hashing algorithm (for integrity check)")]
	public string DatabaseHashingAlgorithm { get; set; } = "SHA3-512";

	[ParameterAlias("dencalg")]
	[ParameterDescription("Database encryption algorithm")]
	public string DatabaseEncryptionAlgorithm { get; set; } = "AES-GCM";
}
