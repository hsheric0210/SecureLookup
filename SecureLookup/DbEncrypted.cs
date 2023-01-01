using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SecureLookup;
public class Database
{
	private DbInnerRoot? inner;

	private byte[] key;

	public string FileName { get; }
	public DbInnerRoot Root => inner ?? throw new InvalidOperationException("Database inner is not loaded yet.");

	public bool Dirty { get; private set; }

	/// <summary>
	/// Creates an instance of encrypted database wrapper
	/// </summary>
	/// <param name="fileName">The encrypted database file path</param>
	/// <param name="key">Database encryption key. Usually a hashed password.</param>
	public Database(string fileName, byte[] key)
	{
		FileName = fileName;
		this.key = key;
	}

	/// <summary>
	/// Mark the database as dirty (modified)
	/// </summary>
	public void MarkDirty() => Dirty = true;

	/// <summary>
	/// Saves the associated database
	/// </summary>
	public void Save()
	{
		InternalSave(Root, FileName, key);
		Dirty = false;
	}

	/// <summary>
	/// Decrypt and load the specified database
	/// </summary>
	/// <returns>Enum class <c>DbLoadResult</c> that indicates the result of decryption</returns>
	/// <exception cref="AggregateException">If any exception occurs during loading or decryption</exception>
	public void Load()
	{
		Dirty = false;
		inner = InternalLoad(FileName, key);
	}

	/// <summary>
	/// Changes the database kjey
	/// </summary>
	/// <param name="newKey">New database key to use</param>
	/// <exception cref="InvalidOperationException">If the database is not loaded yet</exception>
	public void ChangeKey(byte[] newKey)
	{
		key = newKey;
		InternalSave(Root, FileName, newKey);
	}

	/// <summary>
	/// Export the decrypted <c>DbInnerRoot</c> entry to specified file in indented form, in UTF-8 encoding.
	/// </summary>
	/// <param name="destinationFile">The destination file. If the file already exists, it will be overwritten.</param>
	/// <exception cref="InvalidOperationException">If the database is not loaded yet</exception>
	public void Export(string destinationFile)
	{
		using FileStream stream = File.Open(destinationFile, FileMode.Create, FileAccess.Write, FileShare.Read);
		using var xw = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) });
		var serializer = new XmlSerializer(typeof(DbInnerRoot));
		serializer.Serialize(xw, Root);
	}

	/// <summary>
	/// Saves the xml document in encrypted form
	/// </summary>
	/// <param name="root">Decrypted <c>DbInnerRoot</c> entry to save</param>
	/// <param name="fileName">The destination encrypted database file path</param>
	/// <param name="key">Database encryption key</param>
	private static void InternalSave(DbInnerRoot root, string fileName, byte[] key)
	{
		var enc = new Encryption(key);

		var encrypted = enc.Encrypt(root);
		var outer = new DbOuterRoot()
		{
			PasswordHashing = new DbPasswordHashingEntry
			{
				Salt = Convert.ToBase64String(enc.Salt)
			},
			Hash = Hasher.Sha3(encrypted),
			EncryptedData = encrypted
		};

		var serializer = new XmlSerializer(typeof(DbOuterRoot));
		using FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
		using var xw = XmlWriter.Create(fs, new XmlWriterSettings
		{
			Indent = true,
			Encoding = new UTF8Encoding(false)
		});
		serializer.Serialize(xw, outer);
	}

	/// <summary>
	/// Loads the encrypted xml document
	/// </summary>
	/// <param name="source">The source encrypted database file path</param>
	/// <param name="key">Database decryption key</param>
	/// <returns>Decrypted <c>DbInnerRoot</c> entry</returns>
	/// <exception cref="AggregateException">If any exception occurs</exception>
	private static DbInnerRoot InternalLoad(string source, byte[] key)
	{
		try
		{
			using FileStream fs = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.Read);
			var serializer = new XmlSerializer(typeof(DbOuterRoot));
			var outer = (DbOuterRoot)serializer.Deserialize(fs)!;

			// Read salt
			Span<byte> salt = stackalloc byte[16];
			try
			{
				if (!Convert.TryFromBase64String(outer.PasswordHashing.Salt, salt, out var saltBytes) && saltBytes == 16)
					throw new AggregateException($"Salt length mismatch: expected={salt.Length}, actual={saltBytes}");
			}
			catch (Exception ex)
			{
				throw new AggregateException("Salt decoding failure", ex);
			}

			// Compare hash
			var data = outer.EncryptedData;
			try
			{
				var expected = outer.Hash;
				var calculated = Hasher.Sha3(data);
				if (!calculated.Equals(expected, StringComparison.OrdinalIgnoreCase))
					throw new AggregateException($"Database hash mismatch: expected={expected}, calculated={calculated}");
			}
			catch (Exception ex)
			{
				throw new AggregateException("Hash calculation failure", ex);
			}

			try
			{
				var enc = new Encryption(key, salt.ToArray());
				return enc.Decrypt(data);
			}
			catch
			{
				throw new AggregateException("Decryption failure");
			}
		}
		catch (Exception ex)
		{
			throw new AggregateException("File access failure", ex);
		}
	}
}
