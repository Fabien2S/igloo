using Igloo.Buffers;

namespace Igloo.NBT.Types;

public class TagTypeEnd : ITagType<TagTypeEnd, ValueTuple>
{
    private static readonly ITag<ValueTuple> Instance = new Tag<TagTypeEnd, ValueTuple>(default);

    public static TagId Id => TagId.End;

    public static bool Validate(in ValueTuple value)
    {
        return true;
    }

    public static ITag<ValueTuple> Create(in ValueTuple value)
    {
        return Instance;
    }

    public static void Serialize(ref BufferWriter writer, in ValueTuple value)
    {
    }

    public static void Deserialize(ref BufferReader reader, out ValueTuple value)
    {
        value = Instance.Data;
    }
}