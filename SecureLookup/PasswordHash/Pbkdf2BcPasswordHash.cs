using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace SecureLookup.PasswordHash;
internal abstract class Pbkdf2BcPasswordHash : AbstractPasswordHash
{
	protected const string IterationsProp = "iterations";

	public override int SaltSize => 32;

	protected Func<IDigest> DigestSupplier { get; }

	protected Pbkdf2BcPasswordHash(Func<IDigest> digestSupplier) : base("PBKDF2-HMAC-" + digestSupplier().AlgorithmName) => DigestSupplier = digestSupplier;

	public override ReadOnlySpan<byte> Hash(ReadOnlySpan<byte> password, int desiredLength, ReadOnlySpan<byte> salt, IReadOnlyDictionary<string, string> props)
	{
		var pbkdf2 = new Pkcs5S2ParametersGenerator(DigestSupplier());
		pbkdf2.Init(password.ToArray(), salt.ToArray(), int.Parse(props[IterationsProp]));
		var kp = (KeyParameter)pbkdf2.GenerateDerivedMacParameters(desiredLength * 8);
		return kp.GetKey();
	}
	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => props.ContainsKey(IterationsProp) && int.TryParse(props[IterationsProp], out _);
}

internal class Pbkdf2BcHmacSha3256PasswordHash : Pbkdf2BcPasswordHash
{
	public override IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>()
	{
		[IterationsProp] = "5000000"
	};

	public Pbkdf2BcHmacSha3256PasswordHash() : base(() => new Sha3Digest(256))
	{
	}
}

internal class Pbkdf2BcHmacSha3512PasswordHash : Pbkdf2BcPasswordHash
{
	public override IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>()
	{
		[IterationsProp] = "3000000"
	};

	public Pbkdf2BcHmacSha3512PasswordHash() : base(() => new Sha3Digest(512))
	{
	}
}