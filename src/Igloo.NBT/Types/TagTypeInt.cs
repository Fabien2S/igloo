using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeInt : ITagType<TagTypeInt, int>
{
    public static TagId Id => TagId.Int;

    public static bool Validate(in int value)
    {
        return true;
    }

    public static ITag<int> Create(in int value)
    {
        return new Tag<TagTypeInt, int>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in int value)
    {
        writer.WriteInt(value);
    }

    public static void Deserialize(ref BufferReader reader, out int value)
    {
        value = reader.ReadInt();
    }
}