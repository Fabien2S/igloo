using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeLong : ITagType<TagTypeLong, long>
{
    public static TagId Id => TagId.Long;

    public static bool Validate(in long value)
    {
        return true;
    }

    public static ITag<long> Create(in long value)
    {
        return new Tag<TagTypeLong, long>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in long value)
    {
        writer.WriteLong(value);
    }

    public static void Deserialize(ref BufferReader reader, out long value)
    {
        value = reader.ReadLong();
    }
}