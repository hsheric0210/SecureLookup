using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace SecureLookup;
internal static class Hasher
{
	public static byte[] Sha3(byte[] bytes)
	{
		var sha3 = new Sha3Digest(512); // SHA3-512
		sha3.BlockUpdate(bytes, 0, bytes.Length);
		var hash = new byte[64];
		sha3.DoFinal(hash, 0);
		return hash;
	}

	public static string Sha3(string content) => Convert.ToHexString(Sha3(Encoding.UTF8.GetBytes(content)));
}
