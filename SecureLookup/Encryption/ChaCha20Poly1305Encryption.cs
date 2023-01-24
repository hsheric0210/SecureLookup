using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

namespace SecureLookup.Encryption;
internal class ChaCha20Poly1305Encryption : AbstractEncryption
{
	public override int KeySize => 32;
	public override int SeedSize => 12;

	public override int TagSize => 16;

	public ChaCha20Poly1305Encryption() : base("ChaCha20-Poly1305")
	{
	}

	protected override EncryptedData Encrypt(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> key)
	{
		var seed = RandomNumberGenerator.GetBytes(SeedSize);

		try
		{
			return EncryptCng(plaintext, key, seed);
		}
		catch
		{
			// ChaCha20-Poly1305 is not supported Windows version older than 10.0.20142
			// Using BouncyCastle provider instead
			// https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/libraries/Common/src/Interop/Windows/BCrypt/BCryptAeadHandleCache.cs#L21			Console.WriteLine("ChaCha20-Poly1305 Cng implementation unavailable. Falling back to BouncyCastle implementation.");
			Console.WriteLine("ChaCha20-Poly1305 Cng implementation unavailable. Falling back to BouncyCastle implementation.");
			return EncryptBc(plaintext, key, seed);
		}
	}

	private EncryptedData EncryptCng(ReadOnlySpan<byte> plainText, ReadOnlySpan<byte> key, ReadOnlySpan<byte> seed)
	{
		var cipherText = new byte[plainText.Length];
		var tag = new byte[TagSize];
		using var cipher = new ChaCha20Poly1305(key);
		cipher.Encrypt(seed, plainText, cipherText, tag);
		return new EncryptedData(cipherText, seed.ToArray(), tag);
	}

	private EncryptedData EncryptBc(ReadOnlySpan<byte> plainText, ReadOnlySpan<byte> key, ReadOnlySpan<byte> seed)
	{
		IBufferedCipher cipher = CipherUtilities.GetCipher(AlgorithmName);
		cipher.Init(true, new AeadParameters(new KeyParameter(key.ToArray()), TagSize * 8, seed.ToArray()));
		var output = cipher.DoFinal(plainText.ToArray());
		return new EncryptedData(output[..^TagSize], seed.ToArray(), output[^TagSize..]);
	}

	protected override ReadOnlySpan<byte> Decrypt(EncryptedData encrypted, ReadOnlySpan<byte> key)
	{
		try
		{
			return DecryptCng(encrypted, key);
		}
		catch
		{
			// ChaCha20-Poly1305 is not supported Windows version older than 10.0.20142
			// Using BouncyCastle provider instead
			// https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/libraries/Common/src/Interop/Windows/BCrypt/BCryptAeadHandleCache.cs#L21
			Console.WriteLine("ChaCha20-Poly1305 Cng implementation unavailable. Falling back to BouncyCastle implementation.");
			return DecryptBc(encrypted, key);
		}
	}

	private ReadOnlySpan<byte> DecryptCng(EncryptedData encrypted, ReadOnlySpan<byte> key)
	{
		var plainText = new byte[encrypted.Data.Length];
		using var cipher = new ChaCha20Poly1305(key);
		cipher.Decrypt(encrypted.Seed, encrypted.Data, encrypted.Tag, plainText);
		return plainText;
	}

	private ReadOnlySpan<byte> DecryptBc(EncryptedData encrypted, ReadOnlySpan<byte> key)
	{
		IBufferedCipher cipher = CipherUtilities.GetCipher(AlgorithmName);
		cipher.Init(false, new AeadParameters(new KeyParameter(key.ToArray()), TagSize * 8, encrypted.Seed));
		var input = new byte[encrypted.Data.Length + encrypted.Tag.Length];
		encrypted.Data.CopyTo(input, 0);
		encrypted.Tag.CopyTo(input, encrypted.Data.Length);
		return cipher.DoFinal(input);
	}
}
