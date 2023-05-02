using Igloo.Common.Buffers;

namespace Igloo.Common.Serialization;

public interface ISerializer<T>
{
    /// <summary>
    ///     Serialize <see cref="value"/> into <see cref="writer"/>
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="value">The value</param>
    static abstract void Serialize(ref BufferWriter writer, in T value);

    /// <summary>
    ///     Deserialize <see cref="value"/> from <see cref="reader"/>
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="value">The value</param>
    static abstract void Deserialize(ref BufferReader reader, out T value);
}