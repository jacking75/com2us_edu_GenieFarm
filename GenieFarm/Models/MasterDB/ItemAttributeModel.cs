public class ItemAttributeModel
{
    public Int64 Code { get; set; }
    public Int16 TypeCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public Int64 SellPrice { get; set; }
    public Int64 BuyPrice { get; set; }
    public string Desc { get; set; } = string.Empty;
}
