using Igloo.Buffers;
using Igloo.NBT.Types;

namespace Igloo.NBT;

internal record Tag<TType, TData> : ITag<TData> where TType : ITagType<TType, TData>
{
    public TagId Id => TType.Id;

    public TData Data { get; }

    public Tag(in TData data)
    {
        if (!TType.Validate(in data))
            throw new ArgumentException("Invalid NBT data", nameof(data));

        Data = data;
    }

    void ITag.WritePayload(ref BufferWriter writer)
    {
        TType.Serialize(ref writer, this);
    }

    public static implicit operator TData(Tag<TType, TData> tag)
    {
        return tag.Data;
    }
}