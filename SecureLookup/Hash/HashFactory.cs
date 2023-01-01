using SecureLookup.Db;

namespace SecureLookup.Hash;
public static class HashFactory
{
	private static readonly ICollection<AbstractHash> registeredHashes = new List<AbstractHash>()
	{
		new Sha2Hash(),
		new Sha3Hash()
	};

	/// <summary>
	/// Returns a list of all available algorithm names
	/// </summary>
	public static ICollection<string> GetAvailableAlgorithms() => registeredHashes.Select(h => h.AlgorithmName).ToList();

	/// <summary>
	/// Hash specified data with specified hash algorithm
	/// </summary>
	/// <param name="algorithmName">Name of hashing algorithm</param>
	/// <param name="data">Data to hash</param>
	/// <returns>The hash</returns>
	public static byte[] Hash(string algorithmName, byte[] data)
	{
		AbstractHash hash = Lookup(algorithmName);
		return hash.Hash(data);
	}

	/// <summary>
	/// Compare the stored hash and calculated hash
	/// </summary>
	/// <param name="entry">The stored hash, in XML DTO form</param>
	/// <param name="data">Data to compare hash</param>
	/// <returns><c>true</c> if both hash equals, <c>false</c> if hash mismatched</returns>
	public static bool Verify(DbHashEntry entry, byte[] data)
	{
		AbstractHash hash = Lookup(entry.AlgorithmName);
		var expected = Convert.FromHexString(entry.Hash);
		return hash.Hash(data).SequenceEqual(expected);
	}

	public static AbstractHash Lookup(string algorithmName) => registeredHashes.FirstOrDefault(h => h.AlgorithmName.Equals(algorithmName, StringComparison.OrdinalIgnoreCase)) ?? throw new NotSupportedException("Unknown hash algorithm: " + algorithmName);
}
