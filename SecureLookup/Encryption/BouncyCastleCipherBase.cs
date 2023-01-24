using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Misc;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

namespace SecureLookup.Encryption;
internal abstract class BouncyCastleCipherBase : AbstractEncryption
{
	private string algorithmString;

	public BouncyCastleCipherBase(string algorithmName, string algorithmString) : base(algorithmName) => this.algorithmString = algorithmString;

	protected override EncryptedData Encrypt(ReadOnlySpan<byte> plainText, ReadOnlySpan<byte> key)
	{
		var seed = RandomNumberGenerator.GetBytes(SeedSize);
		IBufferedCipher cipher = CipherUtilities.GetCipher(algorithmString);
		cipher.Init(true, new ParametersWithIV(new KeyParameter(key.ToArray()), seed));
		var cipherText = cipher.DoFinal(plainText.ToArray());
		return new EncryptedData(cipherText, seed, Array.Empty<byte>());
	}

	protected override ReadOnlySpan<byte> Decrypt(EncryptedData encrypted, ReadOnlySpan<byte> key)
	{
		IBufferedCipher cipher = CipherUtilities.GetCipher(algorithmString);
		cipher.Init(false, new ParametersWithIV(new KeyParameter(key.ToArray()), encrypted.Seed));
		return cipher.DoFinal(encrypted.Data);
	}
}
