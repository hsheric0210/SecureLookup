using Org.BouncyCastle.Crypto.Generators;

namespace SecureLookup.PasswordHash;
internal class BCryptPasswordHash : AbstractPasswordHash
{
	protected const string CostProp = "cost";

	public override int SaltSize => 16;

	public BCryptPasswordHash() : base("bcrypt")
	{
	}

	public override byte[] Hash(byte[] password, int desiredLength, byte[] salt, IReadOnlyDictionary<string, string> props)
	{
		return BCrypt.Generate(password, salt, int.Parse(props[CostProp]));
	}

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => props.ContainsKey(CostProp) && int.TryParse(props[CostProp], out _);
}
