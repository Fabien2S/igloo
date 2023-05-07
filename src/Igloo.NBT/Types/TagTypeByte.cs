using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeByte : ITagType<TagTypeByte, sbyte>
{
    public static TagId Id => TagId.Byte;

    public static bool Validate(in sbyte value)
    {
        return true;
    }

    public static ITag<sbyte> Create(in sbyte value)
    {
        return new Tag<TagTypeByte, sbyte>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in sbyte value)
    {
        writer.WriteSByte(value);
    }

    public static void Deserialize(ref BufferReader reader, out sbyte value)
    {
        value = reader.ReadSByte();
    }
}