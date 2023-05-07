using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeByteArray : ITagType<TagTypeByteArray, sbyte[]>
{
    public static TagId Id => TagId.ByteArray;

    public static bool Validate(in sbyte[] value)
    {
        return true;
    }

    public static ITag<sbyte[]> Create(in sbyte[] value)
    {
        return new Tag<TagTypeByteArray, sbyte[]>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in sbyte[] value)
    {
        writer.WriteInt(value.Length);
        foreach (var entry in value)
        {
            writer.WriteSByte(entry);
        }
    }

    public static void Deserialize(ref BufferReader reader, out sbyte[] value)
    {
        value = new sbyte[reader.ReadInt()];
        for (var i = 0; i < value.Length; i++)
        {
            value[i] = reader.ReadSByte();
        }
    }
}