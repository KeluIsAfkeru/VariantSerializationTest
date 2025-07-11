using VariantSerializationTest;

Random rand = new();
List<Cell> cells = new();

string GenRandName(int len)
{
	const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
	char[] buffer = new char[len];
	for (int i = 0; i < len; i++)
		buffer[i] = chars[rand.Next(chars.Length)];
	return new string(buffer);
}

Cell GenCell()
{
	var type = (byte)(rand.Next(1, 9));
	var nameLen = rand.Next(3, 20);
	var name = String.Empty;
	if(type == 1)
		name = GenRandName(nameLen);
	var cell = new Cell()
	{
		X = rand.NextDouble() * 1000,
		Y = rand.NextDouble() * 1000,
		Mass = rand.NextDouble() * 100,
		Type = type,
		R = (byte)rand.Next(0, 256),
		G = (byte)rand.Next(0, 256),
		B = (byte)rand.Next(0, 256),
		Name = name
	};
	return cell;
}

List<Cell> GenCells(int len)
{
	var temp = new List<Cell>();
	for(int i = 0; i < len; i++)
	{
		var cell = GenCell();
		temp.Add(cell);
	}
	return temp;
}

//-----------------------------
using var ms_Efficient = new MemoryStream();
using var ms_Raw = new MemoryStream();
const int CellQuantity = 1000000; //生成的细胞圆形数量
cells = GenCells(CellQuantity);

//序列化（变长编码+浮点值量化）
CellSerializer.Serialize(cells, ms_Efficient);
long efficientSize = ms_Efficient.Length;

//原始序列化（如果用最原始的办法将数据类型完整写入）
CellSerializer.serializeRaw(cells, ms_Raw);
long rawSize = ms_Raw.Length;

//反序列化（模拟客户端将二进制转回原来的细胞数据）
ms_Efficient.Position = 0;
var cellsDe = CellSerializer.Deserialize(ms_Efficient);
/*
for (int i = 0; i < cellsDe.Count; i++)
{
	var c = cellsDe[i];
	Console.WriteLine($"Cell {i}: X={c.X:F2}, Y={c.Y:F2}, Mass={c.Mass:F2}, Type={c.Type}, RGB=({c.R},{c.G},{c.B}), Name={c.Name}");
}
*/
//打印结果
Console.WriteLine($"克鲁极端压缩后的序列化字节数: {efficientSize}");
Console.WriteLine($"原始序列化字节数: {rawSize}");
Console.WriteLine($"节省字节数: {rawSize - efficientSize}");
Console.WriteLine($"节省百分比: {((rawSize - efficientSize) * 100.0 / rawSize):F2}%");