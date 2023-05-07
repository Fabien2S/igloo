using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeFloat : ITagType<TagTypeFloat, float>
{
    public static TagId Id => TagId.Float;

    public static bool Validate(in float value)
    {
        return true;
    }

    public static ITag<float> Create(in float value)
    {
        return new Tag<TagTypeFloat, float>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in float value)
    {
        writer.WriteFloat(value);
    }

    public static void Deserialize(ref BufferReader reader, out float value)
    {
        value = reader.ReadFloat();
    }
}