using Igloo.Serialization;

namespace Igloo.NBT.Types;

/// <summary>
///     Represents a NBT type.
/// </summary>
/// <typeparam name="TSelf">The NBT type.</typeparam>
/// <typeparam name="TData">The NBT data type.</typeparam>
public interface ITagType<TSelf, TData> : ISerializer<TData> where TSelf : ITagType<TSelf, TData>
{
    /// <summary>
    ///     Gets the NBT type id.
    /// </summary>
    static abstract TagId Id { get; }

    /// <summary>
    ///     Indicates whether the specified value is a valid NBT data.
    /// </summary>
    /// <param name="data">The value to test.</param>
    /// <returns>true if <paramref name="data"/> is a valid NBT data; otherwise, false.</returns>
    static abstract bool Validate(in TData data);

    /// <summary>
    ///     Creates a new <see cref="ITag{T}"/> with the specified value.
    /// </summary>
    /// <param name="data">The NBT data.</param>
    /// <returns>The created NBT instance.</returns>
    static abstract ITag<TData> Create(in TData data);
}