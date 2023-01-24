using Org.BouncyCastle.Crypto.Digests;

namespace SecureLookup.Hash;
internal class Sha3Hash : AbstractHash
{
	public Sha3Hash() : base("SHA3-512")
	{
	}

	public override ReadOnlySpan<byte> Hash(ReadOnlySpan<byte> data)
	{
		var sha3 = new Sha3Digest(512);
		sha3.BlockUpdate(data.ToArray(), 0, data.Length);
		var hash = new byte[sha3.GetDigestSize()];
		sha3.DoFinal(hash, 0);
		return hash;
	}
}
