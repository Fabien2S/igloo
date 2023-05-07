using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeShort : ITagType<TagTypeShort, short>
{
    public static TagId Id => TagId.Short;

    public static bool Validate(in short value)
    {
        return true;
    }

    public static ITag<short> Create(in short value)
    {
        return new Tag<TagTypeShort, short>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in short value)
    {
        writer.WriteShort(value);
    }

    public static void Deserialize(ref BufferReader reader, out short value)
    {
        value = reader.ReadShort();
    }
}