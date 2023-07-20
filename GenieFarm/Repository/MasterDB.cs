using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using ZLogger;

public class MasterDb : IMasterDb { 

    readonly ILogger<MasterDb> _logger;
    readonly IConfiguration _configuration;
    readonly MySqlConnection _dbConn;
    QueryFactory _queryFactory;

    public List<AttendanceRewardModel>? _attendanceRewardList { get; set; }
    public DefaultFarmDataModel? _defaultFarmData { get; set; }
    public List<DefaultFarmItemModel>? _defaultFarmItemList { get; set; }
    public List<ItemAttributeModel>? _itemAttributeList { get; set; }
    public List<ItemTypeModel>? _itemTypeList { get; set; }
    public VersionModel? _version { get; set; }


    public MasterDb(ILogger<MasterDb> Logger, IConfiguration configuration)
    {
        _logger = Logger;
        _configuration = configuration;

        var DbConnectString = _configuration.GetSection("DBConnection")["MasterDb"];
        _dbConn = new MySqlConnection(DbConnectString);
        _dbConn.Open();

        var compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, compiler);
    }

    public async Task<bool> Init()
    {
        try
        {
            _attendanceRewardList = (await _queryFactory.Query("attendance_reward").GetAsync<AttendanceRewardModel>()).ToList();
            _defaultFarmData = (await _queryFactory.Query("farm_default").GetAsync<DefaultFarmDataModel>()).FirstOrDefault();
            _defaultFarmItemList = (await _queryFactory.Query("item_default").GetAsync<DefaultFarmItemModel>()).ToList();
            _itemAttributeList = (await _queryFactory.Query("item_attribute").GetAsync<ItemAttributeModel>()).ToList();
            _itemTypeList = (await _queryFactory.Query("item_type").GetAsync<ItemTypeModel>()).ToList();
            _version = (await _queryFactory.Query("version").GetAsync<VersionModel>()).FirstOrDefault();
        }
        catch
        {
            _logger.ZLogInformationWithPayload(new { ErrorCode = ErrorCode.MasterDB_Fail_LoadData }, "Failed");
            return false;
        }

        if (!ValidateMasterData())
        {
            _logger.ZLogInformationWithPayload(new { ErrorCode = ErrorCode.MasterDB_Fail_InvalidData }, "Failed");
            return false;
        }

        return true;
    }

    bool ValidateMasterData()
    {
        if (_attendanceRewardList == null || _defaultFarmData == null || _defaultFarmItemList == null || _itemAttributeList == null || _itemTypeList == null || _version == null)
        {
            return false;
        }

        if (_attendanceRewardList.Count == 0 || _defaultFarmItemList.Count == 0 || _itemAttributeList.Count == 0 || _itemTypeList.Count == 0)
        {
            return false;
        }

        return true;
    }
}