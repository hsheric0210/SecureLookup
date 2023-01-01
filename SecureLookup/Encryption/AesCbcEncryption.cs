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

	protected override EncryptedData Encrypt(byte[] plaintext, byte[] key)
	{
		var seed = RandomNumberGenerator.GetBytes(SeedSize);
		using var cipher = Aes.Create();
		cipher.KeySize = KeySize * 8;
		cipher.Key = key;
		cipher.IV = seed;
		cipher.Mode = CipherMode.CBC;
		cipher.Padding = PaddingMode.PKCS7;
		return new EncryptedData(cipher.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length), Array.Empty<byte>(), seed);
	}

	protected override byte[] Decrypt(EncryptedData encrypted, byte[] key)
	{
		using var cipher = Aes.Create();
		cipher.KeySize = KeySize * 8; // AES-256
		cipher.Key = key;
		cipher.IV = encrypted.Seed;
		cipher.Mode = CipherMode.CBC;
		cipher.Padding = PaddingMode.PKCS7;
		var ciphertext = encrypted.Data;
		return cipher.CreateDecryptor().TransformFinalBlock(ciphertext, 0, ciphertext.Length);
	}
}
