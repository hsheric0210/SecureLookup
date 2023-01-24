using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Misc;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

namespace SecureLookup.Encryption;
internal class Cast5Encryption : BouncyCastleCipherBase
{
	public override int KeySize => 16;
	public override int SeedSize => 8;

	public override int TagSize => 0;

	public Cast5Encryption() : base("CAST5-CBC", "CAST5/CBC")
	{
	}
}
