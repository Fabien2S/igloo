using System.Diagnostics.CodeAnalysis;
using Igloo.Buffers;
using Igloo.NBT.Types;

namespace Igloo.NBT;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class TagIO
{
    public static TagWriter WriteTag(this in BufferWriter writer)
    {
        var tagWriter = new TagWriter(writer);
        tagWriter.WriteHeader(ReadOnlySpan<char>.Empty, TagId.Compound);
        return tagWriter;
    }

    public static void WriteTag(this ref BufferWriter writer, ITag<TagCompound>? tag)
    {
        if (tag == null)
        {
            writer.WriteByte((byte)TagId.End);
            return;
        }

        writer.WriteByte((byte)TagId.Compound);
        TagTypeString.Serialize(ref writer, string.Empty);
        TagTypeCompound.Serialize(ref writer, tag.Data);
    }

    public static ITag<TagCompound>? ReadTag(this ref BufferReader reader)
    {
        var id = (TagId)reader.ReadByte();
        if (id == TagId.End)
        {
            return null;
        }

        if (id != TagId.Compound)
            throw new IOException($"Root tag must be a {nameof(TagId.Compound)} tag");

        TagTypeString.Deserialize(ref reader, out _);
        return (ITag<TagCompound>)ITag.ReadPayload(ref reader, id);
    }
}