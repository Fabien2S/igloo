using System.Diagnostics;
using System.Text;
using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeString : ITagType<TagTypeString, string>
{
    private static readonly UTF8Encoding Encoding = new(false, true);

    public static TagId Id => TagId.String;

    public static bool Validate(in string value)
    {
        return true;
    }

    public static ITag<string> Create(in string value)
    {
        return new Tag<TagTypeString, string>(in value);
    }

    public static void Serialize(ref BufferWriter writer, in string value)
    {
        var length = Encoding.GetByteCount(value);
        writer.WriteUShort((ushort)length);

        var dest = writer.WriteBytes(length);
        var written = Encoding.GetBytes(value, dest);
        Debug.Assert(written == length, "written == length");
    }

    public static void Deserialize(ref BufferReader reader, out string value)
    {
        var length = reader.ReadUShort();
        var data = reader.ReadBytes(length);
        value = Encoding.GetString(data);
    }
}