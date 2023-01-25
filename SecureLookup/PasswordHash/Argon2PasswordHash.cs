using Konscious.Security.Cryptography;

namespace SecureLookup.PasswordHash;
internal abstract class Argon2PasswordHash : AbstractPasswordHash
{
	protected const string IterationsProp = "iterations";
	protected const string MemorySizeProp = "memorySizeKb";
	protected const string ParallelismProp = "parallelism";

	public override int SaltSize => 16;
	public override IReadOnlyDictionary<string, string> DefaultProperties => new Dictionary<string, string>()
	{
		[IterationsProp] = "64",
		[MemorySizeProp] = "131072",
		[ParallelismProp] = "10"
	};

	protected Argon2PasswordHash(string typeSuffix) : base("Argon2" + typeSuffix) { }

	public abstract Argon2 GetInstance(ReadOnlySpan<byte> password);

	public override ReadOnlySpan<byte> Hash(ReadOnlySpan<byte> password, int desiredLength, ReadOnlySpan<byte> salt, IReadOnlyDictionary<string, string> props)
	{
		Argon2 argon2 = GetInstance(password);
		argon2.Salt = salt.ToArray();
		argon2.Iterations = int.Parse(props[IterationsProp]);
		argon2.MemorySize = int.Parse(props[MemorySizeProp]);
		argon2.DegreeOfParallelism = int.Parse(props[ParallelismProp]);
		return argon2.GetBytes(desiredLength);
	}

	public override bool IsPropertiesValid(IReadOnlyDictionary<string, string> props)
	{
		return props.ContainsKey(IterationsProp)
			&& props.ContainsKey(MemorySizeProp)
			&& props.ContainsKey(ParallelismProp)
			&& int.TryParse(props[IterationsProp], out _)
			&& int.TryParse(props[MemorySizeProp], out _)
			&& int.TryParse(props[ParallelismProp], out _);
	}
}

internal class Argon2iPasswordHash : Argon2PasswordHash
{
	public Argon2iPasswordHash() : base("i")
	{
	}

	public override Argon2 GetInstance(ReadOnlySpan<byte> password) => new Argon2i(password.ToArray());
}

internal class Argon2dPasswordHash : Argon2PasswordHash
{
	public Argon2dPasswordHash() : base("d")
	{
	}

	public override Argon2 GetInstance(ReadOnlySpan<byte> password) => new Argon2d(password.ToArray());
}

internal class Argon2idPasswordHash : Argon2PasswordHash
{
	public Argon2idPasswordHash() : base("id")
	{
	}

	public override Argon2 GetInstance(ReadOnlySpan<byte> password) => new Argon2id(password.ToArray());
}