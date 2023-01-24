using System.Buffers;

namespace SecureLookup.Compression;
internal static class StreamExtension
{
	private const int DefaultBufferSize = 8192;

	/// <summary>
	/// Copy all stream contents to another stream, but with length limit.
	/// Useful when the stream content is not null-terminated.
	/// </summary>
	/// <param name="from">Source stream</param>
	/// <param name="to">Destination stream</param>
	/// <param name="count">Count of bytes to copy</param>
	/// <param name="bufferSize">Copy buffer size, 8KiB by default</param>
	/// <returns>Count of copied bytes</returns>
	public static long CopyTo(this Stream from, Stream to, long count, int bufferSize = DefaultBufferSize)
	{
		var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
		long totalRead = 0;
		while (totalRead < count)
		{
			var read = from.Read(buffer, 0, Math.Min(buffer.Length, (int)(count - totalRead)));
			if (read <= 0)
				break;
			to.Write(buffer, 0, read);
			totalRead += read;
		}
		ArrayPool<byte>.Shared.Return(buffer);
		return totalRead;
	}

	public static MemoryStream CopyToMemory(this Stream from, long count, int bufferSize = DefaultBufferSize)
	{
		var ms = new MemoryStream((int)count);
		CopyTo(from, ms, count, bufferSize);
		return ms;
	}

	/// <summary>
	/// Retrieves specified count of bytes from the stream
	/// </summary>
	/// <param name="from">The stream to read bytes from</param>
	/// <param name="count">The count of bytes to read from stream</param>
	/// <returns>Retrieved bytes</returns>
	public static byte[] ReadBytes(this Stream from, long count)
	{
		var bytes = new byte[count];
		var readed = from.Read(bytes, 0, (int)count);
		if (readed < count)
			throw new AggregateException($"Failed to read {count} bytes from the stream.");
		return bytes;
	}

	/// <summary>
	/// Retrieves a long value from the stream
	/// </summary>
	/// <param name="from">The stream to read value from</param>
	/// <returns>Retrieved long value</returns>
	public static long ReadLong(this Stream from)
	{
		var bytes = new byte[sizeof(long)];
		int read = from.Read(bytes);
		if (read < bytes.Length)
			throw new AggregateException($"Failed to read {bytes.Length} bytes from the stream.");
		return BitConverter.ToInt64(bytes);
	}
}
