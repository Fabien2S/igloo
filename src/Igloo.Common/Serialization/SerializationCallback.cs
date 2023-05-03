using Igloo.Common.Buffers;

namespace Igloo.Common.Serialization;

public delegate void SerializationCallback(ref BufferWriter writer);