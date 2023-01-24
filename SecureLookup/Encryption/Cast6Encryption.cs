using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Misc;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

namespace SecureLookup.Encryption;
internal class Cast6Encryption : BouncyCastleCipherBase
{
	public override int KeySize => 32;
	public override int SeedSize => 16;

	public override int TagSize => 0;

	public Cast6Encryption() : base("CAST6-CBC", "CAST6/CBC")
	{
	}
}
