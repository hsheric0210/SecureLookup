using SimpleBase;
using System.Xml.Serialization;

namespace SecureLookup.Db;

/// <summary>
/// The Data-Transfer-Object of whole SecureLookup database XML
/// </summary>
[XmlRoot("db")]
public class DbOuterRoot
{
	/// <summary>
	/// Primary password hashing. Only used to hash user password directly. Password is hashed by this algorithm and stored in <see cref="Database"/>. Its salt will change when user changes password.
	/// </summary>
	[XmlElement("primaryPasswordHashing")]
	public DbPasswordHashingEntry PrimaryPasswordHashing { get; set; } = new DbPasswordHashingEntry();

	/// <summary>
	/// Primary password hash size. It will change in range 32-128 when user changes password.
	/// </summary>
	[XmlElement("primaryPasswordHashSize")]
	public int PrimaryPasswordHashSize { get; set; }

	/// <summary>
	/// Secondary password hashing. Used in every encryption or decryption. Its salt will change every save.
	/// </summary>
	[XmlElement("secondaryPasswordHashing")]
	public DbPasswordHashingEntry SecondaryPasswordHashing { get; set; } = new DbPasswordHashingEntry();

	/// <summary>
	/// Database compression
	/// </summary>
	[XmlElement("compression")]
	public DbCompressionEntry Compression { get; set; } = new DbCompressionEntry();

	/// <summary>
	/// Database integrity
	/// </summary>
	[XmlElement("hash")]
	public DbHashEntry Hash { get; set; } = new DbHashEntry();

	/// <summary>
	/// Database encryption
	/// </summary>
	[XmlElement("encryption")]
	public DbEncryptionEntry Encryption { get; set; } = new DbEncryptionEntry();
}

public class DbPasswordHashingEntry
{
	[XmlElement("algorithm")]
	public string AlgorithmName { get; set; } = "Argon2id";

	[XmlElement("salt")]
	public string Salt { get; set; } = "";

	[XmlIgnore]
	public byte[] SaltBytes
	{
		get => Base85.Z85.Decode(Salt);
		set => Salt = Base85.Z85.Encode(value);
	}

	[XmlElement("properties")]
	public string Properties { get; set; } = "";
}

/// <summary>
/// Nested XML which holds informations about inner database data compression
/// </summary>
public class DbCompressionEntry
{
	/// <summary>
	/// The name of compression algorithm applied to <see cref="Data">Data</see>
	/// </summary>
	[XmlElement("algorithm")]
	public string AlgorithmName { get; set; } = "";

	/// <summary>
	/// Nested XML document that stores additional options needed for compression/decompression
	/// </summary>
	[XmlElement("properties")]
	public string Properties { get; set; } = "";
}

public class DbHashEntry
{
	/// <summary>
	/// The name of hashing algorithm to check inner database integrity
	/// </summary>
	[XmlElement("algorithm")]
	public string AlgorithmName { get; set; } = "";

	/// <summary>
	/// The hash of encrypted database in hex string
	/// </summary>
	[XmlElement("hash")]
	public string Hash { get; set; } = "";

	/// <summary>
	/// The hash of encrypted database in byte array
	/// </summary>
	[XmlIgnore]
	public byte[] HashBytes
	{
		get => Convert.FromHexString(Hash);
		set => Hash = Convert.ToHexString(value);
	}
}

/// <summary>
/// Nested XML element which holds informations related to encryption cipher
/// </summary>
public class DbEncryptionEntry
{
	/// <summary>
	/// The name of cipher algorithm used to encrypt or decrypt <see cref="Data"/>
	/// </summary>
	[XmlElement("algorithm")]
	public string AlgorithmName { get; set; } = "";

	/// <summary>
	/// Cipher IV or Nonce in <see href="https://rfc.zeromq.org/spec/32/">Z85</see> encoded string
	/// </summary>
	[XmlElement("seed")]
	public string Seed { get; set; } = "";

	/// <summary>
	/// Cipher IV or Nonce in byte array
	/// </summary>
	[XmlIgnore]
	public byte[] SeedBytes
	{
		get => Base85.Z85.Decode(Seed);
		set => Seed = Base85.Z85.Encode(value);
	}

	/// <summary>
	/// The AEAD tag of <see cref="Data"/> in <see href="https://rfc.zeromq.org/spec/32/">Z85</see> encoded string. Could be empty.
	/// </summary>
	[XmlElement("tag")]
	public string Tag { get; set; } = "";

	/// <summary>
	/// The AEAD tag of <see cref="Data"/> in byte array. Could be empty.
	/// </summary>
	[XmlIgnore]
	public byte[] TagBytes
	{
		get => Base85.Z85.Decode(Tag);
		set => Tag = Base85.Z85.Encode(value);
	}

	/// <summary>
	/// Encrypted and compressed <see cref="DbInnerRoot">inner database XML</see> in <see href="https://rfc.zeromq.org/spec/32/">Z85</see> encoded string
	/// </summary>
	[XmlElement("data")]
	public string Data { get; set; } = "";

	/// <summary>
	/// Encrypted and compressed <see cref="DbInnerRoot">inner database XML</see> in byte array
	/// </summary>
	[XmlIgnore]
	public byte[] DataBytes
	{
		get => Base85.Z85.Decode(Data);
		set => Data = Base85.Z85.Encode(value);
	}
}
