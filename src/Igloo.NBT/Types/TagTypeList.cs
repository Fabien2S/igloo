using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeList : ITagType<TagTypeList, ITag[]>
{
    public static TagId Id => TagId.List;

    public static bool Validate(in ITag[] value)
    {
        switch (value.Length)
        {
            case 0:
                return true;
            case 1:
                return value[0].Id != TagId.End;
            case 2:
                return value[0].Id != TagId.End && value[0].Id == value[1].Id;
            case 3:
                return value[0].Id != TagId.End && value[0].Id == value[1].Id && value[1].Id == value[2].Id;
            default:
            {
                var expected = value[0].Id;
                if (expected == TagId.End)
                {
                    return false;
                }

                for (var i = 1; i < value.Length; i++)
                {
                    if (value[i].Id != expected)
                        return false;
                }

                return true;
            }
        }
    }

    public static ITag<ITag[]> Create(in ITag[] value)
    {
        return new Tag<TagTypeList, ITag[]>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in ITag[] value)
    {
        writer.WriteByte(value.Length == 0 ? (byte)TagId.End : (byte)value[0].Id);
        writer.WriteInt(value.Length);
        foreach (var tag in value)
        {
            tag.WritePayload(ref writer);
        }
    }

    public static void Deserialize(ref BufferReader reader, out ITag[] value)
    {
        var id = (TagId)reader.ReadByte();
        value = new ITag[reader.ReadInt()];
        for (var i = 0; i < value.Length; i++)
        {
            value[i] = ITag.ReadPayload(ref reader, id);
        }
    }
}