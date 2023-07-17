public class ItemTypeModel
{
    public Int16 TypeCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Multiple { get; set; }
    public bool Consumable { get; set; }
    public bool Equipable { get; set; }
}