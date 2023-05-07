using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeLongArray : ITagType<TagTypeLongArray, long[]>
{
    public static TagId Id => TagId.LongArray;

    public static bool Validate(in long[] value)
    {
        return true;
    }

    public static ITag<long[]> Create(in long[] value)
    {
        return new Tag<TagTypeLongArray, long[]>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in long[] value)
    {
        writer.WriteInt(value.Length);
        foreach (var entry in value)
        {
            writer.WriteLong(entry);
        }
    }

    public static void Deserialize(ref BufferReader reader, out long[] value)
    {
        value = new long[reader.ReadInt()];
        for (var i = 0; i < value.Length; i++)
        {
            value[i] = reader.ReadLong();
        }
    }
}