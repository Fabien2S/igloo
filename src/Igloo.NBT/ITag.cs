using Igloo.Buffers;
using Igloo.NBT.Types;

namespace Igloo.NBT;

/// <summary>
///     Represents a NBT instance.
/// </summary>
public interface ITag
{
    internal TagId Id { get; }

    protected void WritePayload(ref BufferWriter writer);

    internal static void WritePayload(ref BufferWriter writer, ITag tag)
    {
        tag.WritePayload(ref writer);
    }

    internal static ITag ReadPayload(ref BufferReader reader, TagId id)
    {
        static ITag ReadTagPayload<TType, TData>(ref BufferReader reader) where TType : ITagType<TType, TData>
        {
            TType.Deserialize(ref reader, out var data);
            return TType.Create(in data);
        }

        return id switch
        {
            TagId.End => ReadTagPayload<TagTypeEnd, ValueTuple>(ref reader),
            TagId.Byte => ReadTagPayload<TagTypeByte, sbyte>(ref reader),
            TagId.Short => ReadTagPayload<TagTypeShort, short>(ref reader),
            TagId.Int => ReadTagPayload<TagTypeInt, int>(ref reader),
            TagId.Long => ReadTagPayload<TagTypeLong, long>(ref reader),
            TagId.Float => ReadTagPayload<TagTypeFloat, float>(ref reader),
            TagId.Double => ReadTagPayload<TagTypeDouble, double>(ref reader),
            TagId.ByteArray => ReadTagPayload<TagTypeByteArray, sbyte[]>(ref reader),
            TagId.String => ReadTagPayload<TagTypeString, string>(ref reader),
            TagId.List => ReadTagPayload<TagTypeList, ITag[]>(ref reader),
            TagId.Compound => ReadTagPayload<TagTypeCompound, TagCompound>(ref reader),
            TagId.IntArray => ReadTagPayload<TagTypeIntArray, int[]>(ref reader),
            TagId.LongArray => ReadTagPayload<TagTypeLongArray, long[]>(ref reader),
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "Invalid NBT id")
        };
    }
}

/// <summary>
///     Represents a typed NBT instance.
/// </summary>
/// <typeparam name="T">The NBT data type</typeparam>
public interface ITag<out T> : ITag
{
    /// <summary>
    ///     Gets the NBT instance data
    /// </summary>
    T Data { get; }
}