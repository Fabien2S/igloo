using Igloo.Buffers;

namespace Igloo.Serialization;

public interface ISerializer<T>
{
    /// <summary>
    ///     Serialize <paramref name="value"/> into <paramref name="writer"/>
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="value">The value</param>
    static abstract void Serialize(ref BufferWriter writer, in T value);

    /// <summary>
    ///     Deserialize <paramref name="value"/> from <paramref name="reader"/>
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="value">The value</param>
    static abstract void Deserialize(ref BufferReader reader, out T value);
}