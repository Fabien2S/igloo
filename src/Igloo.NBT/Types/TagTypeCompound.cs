using System.Collections.Frozen;
using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeCompound : ITagType<TagTypeCompound, TagCompound>
{
    public static TagId Id => TagId.Compound;

    public static bool Validate(in TagCompound value)
    {
        foreach (var (_, tag) in value)
        {
            if (tag.Id == TagId.End)
            {
                return false;
            }
        }

        return true;
    }

    public static ITag<TagCompound> Create(in TagCompound value)
    {
        return new Tag<TagTypeCompound, TagCompound>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in TagCompound value)
    {
        foreach (var (key, tag) in value)
        {
            writer.WriteByte((byte)tag.Id);
            TagTypeString.Serialize(ref writer, key);
            tag.WritePayload(ref writer);
        }

        writer.WriteByte((byte)TagId.End);
    }

    public static void Deserialize(ref BufferReader reader, out TagCompound value)
    {
        var entries = new Dictionary<string, ITag>(StringComparer.Ordinal);

        TagId id;
        while ((id = (TagId)reader.ReadByte()) != TagId.End)
        {
            TagTypeString.Deserialize(ref reader, out var key);
            entries[key] = ITag.ReadPayload(ref reader, id);
        }

        value = entries.ToFrozenDictionary(StringComparer.Ordinal, true);
    }
}