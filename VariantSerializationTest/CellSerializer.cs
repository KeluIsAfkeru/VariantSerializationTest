using System.Text;

namespace VariantSerializationTest;

public class CellSerializer
{
	const uint Q_STEPS = 100_000; //量化精度,动态适应范围，将浮点数莲花成整数再用variant编码会大大提高压缩率，也是很极端的一个方法了，谢谢归零的指点
	static uint Quantize(double value, double min, double max, uint steps)
	{
		if (max == min) return 0;
		if (value < min) value = min;
		if (value > max) value = max;
		return (uint)((value - min) / (max - min) * steps);
	}

	static double Dequantize(uint q, double min, double max, uint steps)
	{
		if (max == min) return min;
		return min + (q / (double)steps) * (max - min);
	}

	static void WriteDouble(Stream stream, double value)
	{
		Span<byte> buffer = stackalloc byte[8];
		BitConverter.TryWriteBytes(buffer, value);
		stream.Write(buffer);
	}
	static double ReadDouble(Stream stream)
	{
		Span<byte> buffer = stackalloc byte[8];
		stream.ReadExactly(buffer);
		return BitConverter.ToDouble(buffer);
	}

	public static void Serialize(List<Cell> cells, Stream stream)
	{
		//遍历所有的细胞找到区间
		double xMin = double.MaxValue, xMax = double.MinValue;
		double yMin = double.MaxValue, yMax = double.MinValue;
		double mMin = double.MaxValue, mMax = double.MinValue;

		foreach (var c in cells)
		{
			if (c.X < xMin) xMin = c.X;
			if (c.X > xMax) xMax = c.X;
			if (c.Y < yMin) yMin = c.Y;
			if (c.Y > yMax) yMax = c.Y;
			if (c.Mass < mMin) mMin = c.Mass;
			if (c.Mass > mMax) mMax = c.Mass;
		}

		//写区间
		WriteDouble(stream, xMin);
		WriteDouble(stream, xMax);
		WriteDouble(stream, yMin);
		WriteDouble(stream, yMax);
		WriteDouble(stream, mMin);
		WriteDouble(stream, mMax);

		//写数量
		VarintHelper.WriteVarint(stream, (ulong)cells.Count);

		//写每个cell
		foreach (var cell in cells)
		{
			uint qx = Quantize(cell.X, xMin, xMax, Q_STEPS);
			uint qy = Quantize(cell.Y, yMin, yMax, Q_STEPS);
			uint qmass = Quantize(cell.Mass, mMin, mMax, Q_STEPS);

			VarintHelper.WriteVarint(stream, qx);
			VarintHelper.WriteVarint(stream, qy);
			VarintHelper.WriteVarint(stream, qmass);

			/*
			 * 1字节存储会浪费
			stream.WriteByte(cell.Type);
			stream.WriteByte(cell.R);
			stream.WriteByte(cell.G);
			stream.WriteByte(cell.B);
			 *用位域编码打包不浪费任何位
			*/

			//用位域编码打包不浪费任何位
			uint packed = PackTypeRGB(cell.Type, cell.R, cell.G, cell.B);
			stream.WriteByte((byte)((packed >> 24) & 0xFF));
			stream.WriteByte((byte)((packed >> 16) & 0xFF));
			stream.WriteByte((byte)((packed >> 8) & 0xFF));
			stream.WriteByte((byte)(packed & 0xFF));

			if (cell.Type == 1)
			{
				var nameBytes = Encoding.UTF8.GetBytes(cell.Name ?? "");
				VarintHelper.WriteVarint(stream, (ulong)nameBytes.Length);
				stream.Write(nameBytes, 0, nameBytes.Length);
			}
		}
	}

	public static List<Cell> Deserialize(Stream stream)
	{
		//读取区间
		double xMin = ReadDouble(stream);
		double xMax = ReadDouble(stream);
		double yMin = ReadDouble(stream);
		double yMax = ReadDouble(stream);
		double mMin = ReadDouble(stream);
		double mMax = ReadDouble(stream);

		//读取数量
		ulong count = VarintHelper.ReadVarint(stream);

		//读取每个cell
		var cells = new List<Cell>();
		for (ulong i = 0; i < count; i++)
		{
			uint qx = (uint)VarintHelper.ReadVarint(stream);
			uint qy = (uint)VarintHelper.ReadVarint(stream);
			uint qmass = (uint)VarintHelper.ReadVarint(stream);

			double x = Dequantize(qx, xMin, xMax, Q_STEPS);
			double y = Dequantize(qy, yMin, yMax, Q_STEPS);
			double mass = Dequantize(qmass, mMin, mMax, Q_STEPS);


			/*
			 * 太浪费位了
			int type = stream.ReadByte();
			int r = stream.ReadByte();
			int g = stream.ReadByte();
			int b = stream.ReadByte();
			*/
			uint packed = 0;
			packed |= (uint)(stream.ReadByte() << 24);
			packed |= (uint)(stream.ReadByte() << 16);
			packed |= (uint)(stream.ReadByte() << 8);
			packed |= (uint)(stream.ReadByte());
			UnpackTypeRGB(packed, out byte type, out byte r, out byte g, out byte b);

			string name = null;
			if (type == 1)
			{
				ulong nameLen = VarintHelper.ReadVarint(stream);
				byte[] nameBytes = new byte[nameLen];
				stream.ReadExactly(nameBytes);
				name = Encoding.UTF8.GetString(nameBytes);
			}

			cells.Add(new Cell
			{
				X = x,
				Y = y,
				Mass = mass,
				Type = (byte)type,
				R = (byte)r,
				G = (byte)g,
				B = (byte)b,
				Name = name
			});
		}
		return cells;
	}

	public static List<Cell> serializeRaw(List<Cell> cells, Stream stream)
	{
		foreach (var c in cells)
		{
			stream.Write(BitConverter.GetBytes(c.X));
			stream.Write(BitConverter.GetBytes(c.Y));
			stream.Write(BitConverter.GetBytes(c.Mass));

			stream.WriteByte(c.Type);
			stream.WriteByte(c.R);
			stream.WriteByte(c.G);
			stream.WriteByte(c.B);

			if (c.Type == 1)
			{
				var nameBytes = Encoding.UTF8.GetBytes(c.Name ?? "");
				stream.Write(BitConverter.GetBytes(nameBytes.Length));
				stream.Write(nameBytes, 0, nameBytes.Length);
			}
		}

		return cells;
	}

	public static uint PackTypeRGB(byte type, byte r, byte g, byte b)
		=> (uint)((type << 24) | (r << 16) | (g << 8) | b);

	public static void UnpackTypeRGB(uint packed, out byte type, out byte r, out byte g, out byte b)
	{
		type = (byte)((packed >> 24) & 0x0F); //只取4位
		r = (byte)((packed >> 16) & 0xFF);
		g = (byte)((packed >> 8) & 0xFF);
		b = (byte)(packed & 0xFF);
	}


}
