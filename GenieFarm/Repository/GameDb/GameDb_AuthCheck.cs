using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using ZLogger;
using System.Collections;
using Org.BouncyCastle.Asn1.Ocsp;
using SqlKata;

public partial class GameDb : IGameDb
{
    readonly ILogger<GameDb> _logger;
    readonly IMasterDb _masterDb;
    readonly IConfiguration _configuration;
    readonly MySqlConnection _dbConn;
    QueryFactory _queryFactory;

    public GameDb(ILogger<GameDb> logger, IMasterDb masterDb, IConfiguration configuration)
    {
        _logger = logger;
        _masterDb = masterDb;
        _configuration = configuration;

        var DbConnectString = _configuration.GetSection("DBConnection")["GameDb"];
        _dbConn = new MySqlConnection(DbConnectString);
        _dbConn.Open();

        var compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, compiler);
    }

    public Query GetQuery(string tableName)
    {
        return _queryFactory.Query(tableName);
    }

    public async Task Rollback(ErrorCode errorCode, List<SqlKata.Query> rollbackQueries)
    {
        foreach (var query in rollbackQueries)
        {
            var affectedRowCount = await _queryFactory.ExecuteAsync(query);

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         new
                                         {
                                             Query = _queryFactory.Compiler.Compile(query).RawSql,
                                             AffectedRowCount = affectedRowCount
                                         }, "Rollback");
        }
    }

    public async Task<Int64> GetUserIdByPlayerId(string playerId)
    {
        var result = (await _queryFactory.Query("user_basicinfo")
                                         .Select("UserId").Where("PlayerId", playerId)
                                         .GetAsync<Int64>()).FirstOrDefault();

        return result;
    }

    public async Task<AccountModel?> GetDefaultUserDataByPlayerId(string playerId)
    {
        var userData = (await _queryFactory.Query("user_basicinfo").Where("PlayerId", playerId)
                                           .GetAsync<AccountModel>()).FirstOrDefault();

        return userData;
    }

    public async Task<AccountModel?> GetDefaultUserDataByUserId(Int64 userId)
    {
        var userData = (await _queryFactory.Query("user_basicinfo").Where("UserId", userId)
                                           .GetAsync<AccountModel>()).FirstOrDefault();

        return userData;
    }

    public async Task<FarmInfoModel?> GetDefaultFarmDataByUserId(Int64 userId)
    {
        var farmInfoData = (await _queryFactory.Query("farm_info").Where("userId", userId)
                                               .GetAsync<FarmInfoModel>()).FirstOrDefault();

        return farmInfoData;
    }

    public async Task<Int64> InsertGetIdDefaultUserData(string playerId, string nickname)
    {
        return await _queryFactory.Query("user_basicinfo")
                                        .InsertGetIdAsync<Int64>(new { PlayerId = playerId,
                                                                       Nickname = nickname });
    }

    public async Task<Int32> InsertDefaultAttendanceData(Int64 userId)
    {
        return await _queryFactory.Query("user_attendance")
                                  .InsertAsync(new { UserId = userId, AttendanceCount = 0 });
    }

    public async Task<Int32> InsertDefaultFarmData(Int64 userId)
    {
        var defaultData = _masterDb._defaultFarmData!;
        return await _queryFactory.Query("farm_info")
                                  .InsertAsync(new { UserId = userId,
                                                     FarmLevel = defaultData.DefaultLevel,
                                                     MaxStorage = defaultData.DefaultStorage,
                                                     Love = defaultData.DefaultLove,
                                                     Money = defaultData.DefaultMoney });
    }

    public async Task<Int32> InsertDefaultItems(Int64 userId)
    {
        // 마스터 DB의 Default 아이템 데이터를 가져와 Insert
        var insertedRow = 0;
        foreach (var item in _masterDb._defaultFarmItemList!)
        {
            insertedRow += await _queryFactory.Query("farm_item")
                                              .InsertAsync(new { OwnerId = userId,
                                                                 ItemCode = item.Code,
                                                                 ItemCount = item.Count });
        }

        return insertedRow;
    }

    public async Task<Int32> UpdateLastLoginAt(Int64 userId)
    {
        return await _queryFactory.Query("user_basicinfo").Where("UserId", userId)
                                  .UpdateAsync(new { LastLoginAt = DateTime.Now });
    }
}