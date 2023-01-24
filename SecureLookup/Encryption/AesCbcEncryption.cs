using System.Security.Cryptography;

namespace SecureLookup.Encryption;
internal class AesCbcEncryption : AbstractEncryption
{
	public override int KeySize => 32; // AES-256
	public override int SeedSize => 16;

	public override int TagSize => 0;

	public AesCbcEncryption() : base("AES-CBC")
	{
	}

	protected override EncryptedData Encrypt(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> key)
	{
		var seed = RandomNumberGenerator.GetBytes(SeedSize);
		using var cipher = Aes.Create();
		cipher.KeySize = KeySize * 8; // AES-256
		cipher.Key = key.ToArray();
		cipher.Mode = CipherMode.CBC;
		return new EncryptedData(cipher.EncryptCbc(plaintext, seed, PaddingMode.PKCS7), Array.Empty<byte>(), seed);
	}

	protected override ReadOnlySpan<byte> Decrypt(EncryptedData encrypted, ReadOnlySpan<byte> key)
	{
		using var cipher = Aes.Create();
		cipher.KeySize = KeySize * 8; // AES-256
		cipher.Key = key.ToArray();
		cipher.Mode = CipherMode.CBC;
		return cipher.DecryptCbc(encrypted.Data, encrypted.Seed, PaddingMode.PKCS7);
	}
}
