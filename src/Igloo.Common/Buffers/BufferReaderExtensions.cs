using System.Buffers.Binary;
using System.Numerics;
using System.Text;

namespace Igloo.Common.Buffers;

/// <summary>
///     Provides <see cref="BufferReader"/> extensions for primitives
/// </summary>
public static class BufferReaderExtensions
{
    public static bool ReadBool(ref this BufferReader reader)
    {
        return reader.ReadByte() != byte.MinValue;
    }

    public static short ReadShort(ref this BufferReader reader)
    {
        const int size = sizeof(short);
        var buffer = reader.ReadBytes(size);
        return BinaryPrimitives.ReadInt16BigEndian(buffer);
    }

    public static ushort ReadUShort(ref this BufferReader reader)
    {
        const int size = sizeof(ushort);
        var buffer = reader.ReadBytes(size);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    public static int ReadInt(ref this BufferReader reader)
    {
        const int size = sizeof(int);
        var buffer = reader.ReadBytes(size);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    public static uint ReadUInt(ref this BufferReader reader)
    {
        const int size = sizeof(uint);
        var buffer = reader.ReadBytes(size);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }

    public static long ReadLong(ref this BufferReader reader)
    {
        const int size = sizeof(long);
        var buffer = reader.ReadBytes(size);
        return BinaryPrimitives.ReadInt64BigEndian(buffer);
    }

    public static ulong ReadULong(ref this BufferReader reader)
    {
        const int size = sizeof(ulong);
        var buffer = reader.ReadBytes(size);
        return BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }

    public static float ReadFloat(ref this BufferReader reader)
    {
        var bits = reader.ReadInt();
        return BitConverter.Int32BitsToSingle(bits);
    }

    public static double ReadDouble(ref this BufferReader reader)
    {
        var bits = reader.ReadLong();
        return BitConverter.Int64BitsToDouble(bits);
    }

    public static string ReadString(ref this BufferReader reader, ushort maxLength = ushort.MaxValue)
    {
        var maxByteCount = maxLength * 3;

        var byteCount = reader.ReadVarInt32();
        if (byteCount < 0 || byteCount > maxByteCount)
            throw new IOException($"String is too large ({byteCount} > {maxByteCount})");

        var byteBuffer = reader.ReadBytes(byteCount);
        var str = Encoding.UTF8.GetString(byteBuffer);

        if (str.Length > maxLength)
            throw new IOException($"String is too long ({str.Length} > {maxLength})");

        return str;
    }

    public static Vector2 ReadVector2(ref this BufferReader reader)
    {
        return new Vector2(
            reader.ReadFloat(), reader.ReadFloat()
        );
    }

    public static Vector3 ReadVector3(ref this BufferReader reader)
    {
        return new Vector3(
            reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()
        );
    }
}