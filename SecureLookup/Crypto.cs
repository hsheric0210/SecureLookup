using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace SecureLookup;
internal static class Crypto
{
	public static byte[] GenerateBytes(int size)
	{
		var bytes = new byte[size];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(bytes);
		return bytes;
	}

	public static byte[] DeriveKey(byte[] password, byte[] salt, int desiredLength)
	{
		var hasher = new Argon2id(password)
		{
			Salt = salt,
			DegreeOfParallelism = 12,
			MemorySize = 65536,
			Iterations = 64
		};
		return hasher.GetBytes(desiredLength);
	}
}
