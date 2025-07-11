namespace VariantSerializationTest;

public class Cell
{
	public double X { get; set; }
	public double Y { get; set; }
	public double Mass { get; set; }
	public byte Type { get; set; }
	public byte R { get; set; }
	public byte G { get; set; }
	public byte B { get; set; }
	public String? Name { get; set; } //Type == 1 才会有名字
}
//正常来说（就当前细胞数据结构），一个细胞会至少占用8+8+8+1+1+1+1=28字节
