namespace VariantSerializationTest;

public class VarintHelper
{
	public static void WriteVarint(Stream stream, ulong value)
	{
		while (value >= 0x80)
		{
			stream.WriteByte((byte)(value | 0x80));
			value >>= 7;
		}
		stream.WriteByte((byte)value);
	}

	public static ulong ReadVarint(Stream stream)
	{
		ulong result = 0;
		int shift = 0;
		while (true)
		{
			int b = stream.ReadByte();
			if (b == -1) throw new EndOfStreamException();
			if ((b & 0x80) == 0)
			{
				result |= (ulong)b << shift;
				break;
			}
			result |= (ulong)(b & 0x7F) << shift;
			shift += 7;
		}
		return result;
	}
}
