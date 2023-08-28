namespace QuickJson.Tests.TestingClasses.WithoutAttributes;

internal class Numerics
{
    public ushort UShort { get; set; } = 41;
    public short Short { get; set; } = -6;
    public uint UInt { get; set; } = 42;
    public int Int { get; set; } = 75;
    public ulong ULong { get; set; } = 589;
    public long Long { get; set; } = -9754516;
    public double Double { get; set; } = 31.842;
    public float Float { get; set; } = 93.45721f;
    public decimal Decimal { get; set; } = 2899.9871m;
    public string StringWithNumericValue { get; set; } = "2899.9871";
}