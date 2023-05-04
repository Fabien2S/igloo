using Igloo.Buffers;

namespace Igloo.Serialization;

public delegate void SerializationCallback(ref BufferWriter writer);