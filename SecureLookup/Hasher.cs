using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace SecureLookup;
internal static class Hasher
{
	/// <summary>
	/// Digests given byte array and returns SHA3-512 hash
	/// </summary>
	/// <param name="bytes">Input byte array</param>
	/// <returns>The SHA3-512 hash of given byte array</returns>
	public static byte[] Sha3(byte[] bytes)
	{
		var sha3 = new Sha3Digest(512); // SHA3-512
		sha3.BlockUpdate(bytes, 0, bytes.Length);
		var hash = new byte[64];
		sha3.DoFinal(hash, 0);
		return hash;
	}

	/// <summary>
	/// Digests UTF-8 decoded byte array of given string and returns SHA3-512 hash in hex string format
	/// </summary>
	/// <param name="content">Input string</param>
	/// <returns>The SHA3-512 hash in hex string format</returns>
	public static string Sha3(string content) => Convert.ToHexString(Sha3(Encoding.UTF8.GetBytes(content)));
}
