using System.Security.Cryptography;

namespace SecureLookup.PasswordHash;
internal abstract class Pbkdf2PasswordHash : AbstractPasswordHash
{
	protected const string IterationsProp = "iterations";

	public override int SaltSize => 32;

	protected HashAlgorithmName HashAlgorithmName { get; }

	protected Pbkdf2PasswordHash(HashAlgorithmName hashAlgorithmName) : base("PBKDF2-HMAC-" + hashAlgorithmName.ToString()) => HashAlgorithmName = hashAlgorithmName;

	public override ReadOnlySpan<byte> Hash(ReadOnlySpan<byte> password, int desiredLength, ReadOnlySpan<byte> salt, IReadOnlyDictionary<string, string> props) => Rfc2898DeriveBytes.Pbkdf2(password, salt, int.Parse(props[IterationsProp]), HashAlgorithmName, desiredLength);

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props) => props.ContainsKey(IterationsProp) && int.TryParse(props[IterationsProp], out _);
}

internal class Pbkdf2HmacSha1PasswordHash : Pbkdf2PasswordHash
{
	public Pbkdf2HmacSha1PasswordHash() : base(HashAlgorithmName.SHA1)
	{
	}
}

internal class Pbkdf2HmacSha256PasswordHash : Pbkdf2PasswordHash
{
	public Pbkdf2HmacSha256PasswordHash() : base(HashAlgorithmName.SHA256)
	{
	}
}

internal class Pbkdf2HmacSha512PasswordHash : Pbkdf2PasswordHash
{
	public Pbkdf2HmacSha512PasswordHash() : base(HashAlgorithmName.SHA512)
	{
	}
}
