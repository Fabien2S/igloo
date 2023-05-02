using System.Buffers;

namespace Igloo.Common.Buffers;

public static class BufferVarInt
{
    private const byte MaxSize = 5;

    private const byte SegmentBits = 0b01111111;
    private const byte SegmentBitCount = 7;

    private const byte ContinueBit = 0b10000000;

    public static bool TryReadVarInt32(ref this SequenceReader<byte> reader, out int value, out int size)
    {
        value = 0;
        size = 0;

        byte read;
        do
        {
            if (!reader.TryRead(out read))
                return false;

            value |= (read & SegmentBits) << (size * SegmentBitCount);
            size++;

            if (size >= reader.Length)
                return false;
            if (size >= MaxSize)
                return false;
        } while ((read & ContinueBit) == ContinueBit);

        return true;
    }

    public static int ReadVarInt32(ref this BufferReader reader, int maxSize = MaxSize)
    {
        var size = 0;
        var value = 0;

        byte read;
        do
        {
            read = reader.ReadByte();
            value |= (read & SegmentBits) << (size * SegmentBitCount);
            size++;

            if (size >= maxSize)
                throw new IOException("VarInt is too long");
        } while ((read & ContinueBit) == ContinueBit);

        return value;
    }

    public static void WriteVarInt32(ref this BufferWriter writer, int value)
    {
        while (true)
        {
            if ((value & ~SegmentBits) == 0)
            {
                writer.WriteByte((byte)value);
                return;
            }

            writer.WriteByte((byte)((value & SegmentBits) | ContinueBit));
            value >>>= SegmentBitCount;
        }
    }

    public static void WriteVarInt32(ref this BufferWriter writer, int value, out int byteCount)
    {
        byteCount = 1;

        while (true)
        {
            if ((value & ~SegmentBits) == 0)
            {
                writer.WriteByte((byte)value);
                return;
            }

            writer.WriteByte((byte)((value & SegmentBits) | ContinueBit));
            value >>>= SegmentBitCount;
            byteCount++;
        }
    }
}