using Org.BouncyCastle.Crypto.Generators;

namespace SecureLookup.PasswordHash;
internal class SCryptPasswordHash : AbstractPasswordHash
{
	protected const string CostFactorProp = "N";
	protected const string BlockSizeFactorProp = "r";
	protected const string ParallelizationFactorProp = "p";

	public override int SaltSize => 16;

	public SCryptPasswordHash() : base("scrypt")
	{
	}

	public override byte[] Hash(byte[] password, int desiredLength, byte[] salt, IReadOnlyDictionary<string, string> props)
	{
		return SCrypt.Generate(password, salt, int.Parse(props[CostFactorProp]), int.Parse(props[BlockSizeFactorProp]), int.Parse(props[ParallelizationFactorProp]), desiredLength);
	}

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props)
	{
		return props.ContainsKey(CostFactorProp)
			&& props.ContainsKey(BlockSizeFactorProp)
			&& props.ContainsKey(ParallelizationFactorProp)
			&& int.TryParse(props[CostFactorProp], out _)
			&& int.TryParse(props[BlockSizeFactorProp], out _)
			&& int.TryParse(props[ParallelizationFactorProp], out _);
	}
}
