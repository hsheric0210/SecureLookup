using System.Security.Cryptography;

namespace SecureLookup.Hash;
internal class Sha2Hash : AbstractHash
{
	public Sha2Hash() : base("SHA2-512")
	{
	}

	public override ReadOnlySpan<byte> Hash(ReadOnlySpan<byte> data) => SHA512.HashData(data);
}
