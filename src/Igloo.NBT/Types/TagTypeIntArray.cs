using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeIntArray : ITagType<TagTypeIntArray, int[]>
{
    public static TagId Id => TagId.IntArray;

    public static bool Validate(in int[] value)
    {
        return true;
    }

    public static ITag<int[]> Create(in int[] value)
    {
        return new Tag<TagTypeIntArray, int[]>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in int[] value)
    {
        writer.WriteInt(value.Length);
        foreach (var entry in value)
        {
            writer.WriteInt(entry);
        }
    }

    public static void Deserialize(ref BufferReader reader, out int[] value)
    {
        value = new int[reader.ReadInt()];
        for (var i = 0; i < value.Length; i++)
        {
            value[i] = reader.ReadInt();
        }
    }
}