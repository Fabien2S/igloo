using System.Diagnostics;
using System.Text;
using Igloo.Buffers;

namespace Igloo.NBT;

public readonly ref struct TagWriter
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    private readonly BufferWriter _writer;

    internal TagWriter(in BufferWriter writer)
    {
        _writer = writer;
    }

    public void Dispose()
    {
        _writer.WriteByte((byte)TagId.End);
    }

    internal void WriteHeader(ReadOnlySpan<char> name, TagId id)
    {
        _writer.WriteByte((byte)id);
        WriteUtf(name);
    }

    private void WriteUtf(ReadOnlySpan<char> value)
    {
        var length = Encoding.GetByteCount(value);
        _writer.WriteUShort((ushort)length);

        var dest = _writer.WriteBytes(length);
        var written = Encoding.GetBytes(value, dest);
        Debug.Assert(written == length, "written == length");
    }

    public void WriteByte(ReadOnlySpan<char> tag, sbyte value)
    {
        WriteHeader(tag, TagId.Byte);
        _writer.WriteSByte(value);
    }

    public void WriteShort(ReadOnlySpan<char> tag, short value)
    {
        WriteHeader(tag, TagId.Short);
        _writer.WriteInt(value);
    }

    public void WriteInt(ReadOnlySpan<char> tag, int value)
    {
        WriteHeader(tag, TagId.Int);
        _writer.WriteInt(value);
    }

    public void WriteLong(ReadOnlySpan<char> tag, long value)
    {
        WriteHeader(tag, TagId.Long);
        _writer.WriteLong(value);
    }

    public void WriteFloat(ReadOnlySpan<char> tag, float value)
    {
        WriteHeader(tag, TagId.Float);
        _writer.WriteFloat(value);
    }

    public void WriteDouble(ReadOnlySpan<char> tag, double value)
    {
        WriteHeader(tag, TagId.Double);
        _writer.WriteDouble(value);
    }

    public void WriteByteArray(ReadOnlySpan<char> tag, ReadOnlySpan<sbyte> value)
    {
        WriteHeader(tag, TagId.ByteArray);
        _writer.WriteInt(value.Length);
        foreach (var element in value)
            _writer.WriteSByte(element);
    }

    public void WriteString(ReadOnlySpan<char> tag, ReadOnlySpan<char> value)
    {
        WriteHeader(tag, TagId.String);
        WriteUtf(value);
    }

    // public void BeginList(ReadOnlySpan<char> tag, TagId listId)
    // {
    //     WriteHeader(tag, TagId.List);
    //     _writer.WriteInt(value.Length);
    //     foreach (var element in value)
    //         _writer.WriteSByte(element);
    // }

    public TagWriter BeginCompound(ReadOnlySpan<char> tag)
    {
        WriteHeader(tag, TagId.Compound);
        return this;
    }

    public void WriteIntArray(ReadOnlySpan<char> tag, ReadOnlySpan<int> value)
    {
        WriteHeader(tag, TagId.IntArray);
        _writer.WriteInt(value.Length);
        foreach (var element in value)
            _writer.WriteInt(element);
    }

    public void WriteLongArray(ReadOnlySpan<char> tag, ReadOnlySpan<long> value)
    {
        WriteHeader(tag, TagId.LongArray);
        _writer.WriteInt(value.Length);
        foreach (var element in value)
            _writer.WriteLong(element);
    }
}