using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeDouble : ITagType<TagTypeDouble, double>
{
    public static TagId Id => TagId.Double;

    public static bool Validate(in double value)
    {
        return true;
    }

    public static ITag<double> Create(in double value)
    {
        return new Tag<TagTypeDouble, double>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in double value)
    {
        writer.WriteDouble(value);
    }

    public static void Deserialize(ref BufferReader reader, out double value)
    {
        value = reader.ReadDouble();
    }
}