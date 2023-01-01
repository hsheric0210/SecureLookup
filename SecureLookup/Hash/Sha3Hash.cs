using Org.BouncyCastle.Crypto.Digests;

namespace SecureLookup.Hash;
internal class Sha3Hash : AbstractHash
{
	public Sha3Hash() : base("SHA3-512")
	{
	}

	public override byte[] Hash(byte[] data)
	{
		var sha3 = new Sha3Digest(512);
		sha3.BlockUpdate(data, 0, data.Length);
		var hash = new byte[64];
		sha3.DoFinal(hash, 0);
		return hash;
	}
}
