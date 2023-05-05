using System.Buffers;

namespace Igloo.Buffers;

/// <summary>
///     Provides a big-endian writer for binary data
/// </summary>
public readonly ref struct BufferWriter
{
    public delegate void Writer<in T>(ref BufferWriter writer, T value);

    private readonly IBufferWriter<byte> _writer;

    public BufferWriter(IBufferWriter<byte> buffer)
    {
        _writer = buffer;
    }

    /// <summary>
    ///     Write a given amount of bytes 
    /// </summary>
    /// <param name="size">The size of the buffer</param>
    /// <returns>The buffer to write to</returns>
    /// <remarks>The returned buffer is only valid until the next call to any of the WriteXXX functions</remarks>
    public Span<byte> WriteBytes(int size)
    {
        var buffer = _writer.GetSpan(size);
        _writer.Advance(size);
        return buffer;
    }

    public void WriteBytes(ReadOnlySpan<byte> value)
    {
        _writer.Write(value);
    }

    /// <summary>
    ///     Write a <see cref="byte"/> into the buffer
    /// </summary>
    /// <param name="value">The value</param>
    public void WriteByte(byte value)
    {
        const int size = sizeof(byte);
        var buffer = WriteBytes(size);
        buffer[0] = value;
    }

    /// <summary>
    ///     Write a <see cref="sbyte"/> into the buffer
    /// </summary>
    /// <param name="value">The value</param>
    public void WriteSByte(sbyte value)
    {
        const int size = sizeof(sbyte);
        var buffer = WriteBytes(size);
        buffer[0] = (byte)value;
    }
}