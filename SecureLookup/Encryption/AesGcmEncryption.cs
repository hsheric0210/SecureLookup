using System.Security.Cryptography;

namespace SecureLookup.Encryption;
internal class AesGcmEncryption : AbstractEncryption
{
	public override int KeySize => 32; // AES-256
	public override int SeedSize => AesGcm.NonceByteSizes.MaxSize;

	public override int TagSize => AesGcm.TagByteSizes.MaxSize;

	public AesGcmEncryption() : base("AES-GCM")
	{
	}

	protected override EncryptedData Encrypt(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> key)
	{
		var cipherText = new byte[plaintext.Length];
		var seed = RandomNumberGenerator.GetBytes(SeedSize);
		var tag = new byte[TagSize];
		using var cipher = new AesGcm(key);
		cipher.Encrypt(seed, plaintext, cipherText, tag);
		return new EncryptedData(cipherText, seed, tag);
	}

	protected override ReadOnlySpan<byte> Decrypt(EncryptedData encrypted, ReadOnlySpan<byte> key)
	{
		var plainText = new byte[encrypted.Data.Length];
		using var cipher = new AesGcm(key);
		cipher.Decrypt(encrypted.Seed, encrypted.Data, encrypted.Tag, plainText);
		return plainText;
	}
}
