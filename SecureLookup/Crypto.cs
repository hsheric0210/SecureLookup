﻿using Konscious.Security.Cryptography;

namespace SecureLookup;
internal static class Crypto
{
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
