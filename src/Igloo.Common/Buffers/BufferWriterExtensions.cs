using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Igloo.Buffers;

/// <summary>
///     Provides <see cref="BufferWriter"/> extensions for primitives
/// </summary>
public static class BufferWriterExtensions
{
    public static void WriteBool(ref this BufferWriter writer, bool value)
    {
        writer.WriteByte(value ? byte.MaxValue : byte.MinValue);
    }

    public static void WriteShort(ref this BufferWriter writer, short value)
    {
        const int size = sizeof(short);
        var buffer = writer.WriteBytes(size);
        BinaryPrimitives.WriteInt16BigEndian(buffer, value);
    }

    public static void WriteUShort(ref this BufferWriter writer, ushort value)
    {
        const int size = sizeof(ushort);
        var buffer = writer.WriteBytes(size);
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
    }

    public static void WriteInt(ref this BufferWriter writer, int value)
    {
        const int size = sizeof(int);
        var buffer = writer.WriteBytes(size);
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
    }

    public static void WriteUInt(ref this BufferWriter writer, uint value)
    {
        const int size = sizeof(uint);
        var buffer = writer.WriteBytes(size);
        BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
    }

    public static void WriteLong(ref this BufferWriter writer, long value)
    {
        const int size = sizeof(long);
        var buffer = writer.WriteBytes(size);
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
    }

    public static void WriteULong(ref this BufferWriter writer, ulong value)
    {
        const int size = sizeof(ulong);
        var buffer = writer.WriteBytes(size);
        BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
    }

    public static void WriteFloat(ref this BufferWriter writer, float value)
    {
        var bits = BitConverter.SingleToInt32Bits(value);
        writer.WriteInt(bits);
    }

    public static void WriteDouble(ref this BufferWriter writer, double value)
    {
        var bits = BitConverter.DoubleToInt64Bits(value);
        writer.WriteLong(bits);
    }

    public static void WriteString(ref this BufferWriter writer, ReadOnlySpan<char> str, short maxLength = short.MaxValue)
    {
        var maxByteCount = maxLength * 3;

        if (str.Length > maxLength)
            throw new IOException($"String is too long ({str.Length} > {maxLength})");

        var byteCount = Encoding.UTF8.GetByteCount(str);
        if (byteCount > maxByteCount)
            throw new IOException($"String is too large ({byteCount} > {maxByteCount})");

        writer.WriteVarInt(byteCount);
        var byteBuffer = writer.WriteBytes(byteCount);
        Encoding.UTF8.GetBytes(str, byteBuffer);
    }

    public static void WriteIdentifier(ref this BufferWriter writer, Identifier value)
    {
        var identifierString = value.ToString();
        writer.WriteString(identifierString);
    }

    public static void WriteUuid(ref this BufferWriter writer, Guid uuid)
    {
        const int uuidSize = 16;
        var uuidBuffer = writer.WriteBytes(uuidSize);
        if (!uuid.TryWriteBytes(uuidBuffer))
            throw new UnreachableException($"UUID must be {uuidSize} bytes long");

        (uuidBuffer[0], uuidBuffer[3]) = (uuidBuffer[3], uuidBuffer[0]);
        (uuidBuffer[1], uuidBuffer[2]) = (uuidBuffer[2], uuidBuffer[1]);
        (uuidBuffer[4], uuidBuffer[5]) = (uuidBuffer[5], uuidBuffer[4]);
        (uuidBuffer[6], uuidBuffer[7]) = (uuidBuffer[7], uuidBuffer[6]);
    }

    public static void WriteVector2(ref this BufferWriter writer, Vector2 value)
    {
        writer.WriteFloat(value.X);
        writer.WriteFloat(value.Y);
    }

    public static void WriteVector3(ref this BufferWriter writer, Vector3 value)
    {
        writer.WriteFloat(value.X);
        writer.WriteFloat(value.Y);
        writer.WriteFloat(value.Z);
    }

    public static void WriteVarIntArray<T>(ref this BufferWriter writer, ReadOnlySpan<T> array, BufferWriter.Writer<T> writerDelegate)
    {
        writer.WriteVarInt(array.Length);
        foreach (var value in array)
            writerDelegate(ref writer, value);
    }
}