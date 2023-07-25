public interface IMasterDb
{
    public List<AttendanceRewardModel>? _attendanceRewardList { get; }
    public DefaultFarmDataModel? _defaultFarmData { get; }
    public List<DefaultFarmItemModel>? _defaultFarmItemList { get; }
    public List<ItemAttributeModel>? _itemAttributeList { get; }
    public List<ItemTypeModel>? _itemTypeList { get; }
    public Dictionary<string, Int32>? _definedValueDictionary { get; }
    public VersionModel? _version { get; }

    public Task<bool> Init();
}