using Org.BouncyCastle.Crypto.Generators;

namespace SecureLookup.PasswordHash;
internal class BCryptPasswordHash : AbstractPasswordHash
{
	protected const string CostProp = "cost";

	public override int SaltSize => 16;

	public BCryptPasswordHash() : base("bcrypt")
	{
	}

	public override ReadOnlySpan<byte> Hash(ReadOnlySpan<byte> password, int desiredLength, ReadOnlySpan<byte> salt, IReadOnlyDictionary<string, string> props) => BCrypt.Generate(password.ToArray(), salt.ToArray(), int.Parse(props[CostProp]));

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => props.ContainsKey(CostProp) && int.TryParse(props[CostProp], out _);
}
