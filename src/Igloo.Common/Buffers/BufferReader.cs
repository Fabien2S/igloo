namespace Igloo.Buffers;

/// <summary>
///     Provides a big-endian reader for binary data
/// </summary>
public ref struct BufferReader
{
    public ReadOnlySpan<byte> Readable => _buffer[_cursor..];
    public ReadOnlySpan<byte> ReadSpan => _buffer[.._cursor];

    private readonly ReadOnlySpan<byte> _buffer;

    private int _cursor;

    public BufferReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;
        _cursor = 0;
    }

    /// <summary>
    ///     Read a given amount of bytes 
    /// </summary>
    /// <param name="size">The size of the buffer</param>
    /// <returns>The buffer to read from</returns>
    /// <remarks>The returned buffer is only valid until the next call to any of the ReadXXX functions</remarks>
    public ReadOnlySpan<byte> ReadBytes(int size)
    {
        var cursor = _cursor + size;
        if (cursor < 0 || _buffer.Length < cursor)
            throw new EndOfStreamException();

        _cursor = cursor;
        return _buffer.Slice(_cursor - size, size);
    }

    /// <summary>
    ///     Read a <see cref="byte"/> from the buffer
    /// </summary>
    /// <returns>The unsigned byte value</returns>
    public byte ReadByte()
    {
        const int size = sizeof(byte);
        var buffer = ReadBytes(size);
        return buffer[0];
    }

    /// <summary>
    ///     Read a <see cref="sbyte"/> from the buffer
    /// </summary>
    /// <returns>The signed byte value</returns>
    public sbyte ReadSByte()
    {
        const int size = sizeof(sbyte);
        var buffer = ReadBytes(size);
        return (sbyte)buffer[0];
    }
}