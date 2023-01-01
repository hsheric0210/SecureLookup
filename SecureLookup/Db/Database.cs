using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SecureLookup.Db;
public class Database
{
	/// <summary>
	/// Database inner root DTO
	/// </summary>
	public DbInnerRoot InnerRoot { get; internal set; }

	/// <summary>
	/// Database outer root DTO
	/// </summary>
	public DbOuterRoot OuterRoot { get; internal set; }

	/// <summary>
	/// Password hashed with primary password hashing algorithm.
	/// </summary>
	internal byte[] PasswordHash { get; set; }

	/// <summary>
	/// The database source file path
	/// </summary>
	public string Source { get; }

	/// <summary>
	/// Is this database dirty? (modified)
	/// </summary>
	public bool Dirty { get; private set; }

	/// <summary>
	/// Creates an instance of encrypted database wrapper
	/// </summary>
	/// <param name="fileName">The encrypted database file path</param>
	/// <param name="key">Database encryption key. Usually a hashed password.</param>
	/// <exception cref="FileNotFoundException">If the database file <paramref name="fileName"/> not found</exception>
	internal Database()
	{
	}

	/// <summary>
	/// Mark the database as dirty (modified)
	/// </summary>
	public void MarkDirty() => Dirty = true;


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
		serializer.Serialize(xw, Inner);
	}
}
