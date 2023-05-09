namespace Igloo.NBT;

public static class TagWriterExtensions
{
    public static void WriteBool(this in TagWriter writer, ReadOnlySpan<char> name, bool value)
    {
        writer.WriteByte(name, (sbyte)(value ? 1 : 0));
    }
}