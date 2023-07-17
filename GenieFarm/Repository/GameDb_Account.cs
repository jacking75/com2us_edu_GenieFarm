using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using ZLogger;
using System.Collections;

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

    public async Task<AccountModel?> GetDefaultDataByPlayerId(string playerId)
    {
        // PlayerId가 일치하는 행의 User Basic Information을 가져온다.
        var query = _queryFactory.Query("user_basicinfo").Where("playerId", playerId);
        var result = (await query.GetAsync<AccountModel>()).FirstOrDefault();

        return result;
    }

    public async Task<ErrorCode> CreateDefaultData(string playerId, string nickname)
    {
        var rollbackQuerys = new List<SqlKata.Query>();
        var userId = (Int64)0;

        // 기본 유저 데이터 생성
        try
        {
            userId = await _queryFactory.Query("user_basicinfo").InsertGetIdAsync<Int64>(new { PlayerId = playerId, Nickname = nickname });
            rollbackQuerys.Add(_queryFactory.Query("user_basicinfo").Where("UserId", userId));
        }
        catch
        {
            return ErrorCode.DuplicateNickname;
        }

        // 유저 출석 정보 생성
        try
        {
            var attendanceInsert = await _queryFactory.Query("user_attendance").InsertAsync(new { UserId = userId, AttendanceCount = 0 });

            rollbackQuerys.Add(_queryFactory.Query("user_attendance").Where("UserId", userId));
        }
        catch
        {
            foreach (var query in rollbackQuerys)
            {
                await query.DeleteAsync();
            }

            return ErrorCode.CreateDefaultAttendanceDataException;
        }

        // 농장 기본 정보 생성
        try
        {
            // 마스터 DB의 Default 데이터를 가져와 생성
            var defaultData = _masterDb._defaultFarmData!;
            await _queryFactory.Query("farm_info").InsertAsync(new { UserId = userId, FarmLevel = defaultData.DefaultLevel, MaxStorage = defaultData.DefaultStorage, Love = defaultData.DefaultLove, Money = defaultData.DefaultMoney });

            rollbackQuerys.Add(_queryFactory.Query("farm_info").Where("UserId", userId));
        }
        catch
        {
            foreach(var query in rollbackQuerys)
            {
                await query.DeleteAsync();
            }

            return ErrorCode.CreateDefaultFarmDataException;
        }

        // 기본 아이템 생성
        try
        {
            rollbackQuerys.Add(_queryFactory.Query("farm_item").Where("OwnerId", userId));

            // 마스터 DB의 Default 아이템 리스트를 가져와 쿼리 생성
            var columns = new[] { "OwnerId", "ItemCode", "ItemCount" };
            var data = new List<object[]>();
            _masterDb._defaultFarmItemList!.ForEach(item =>
            {
                data.Add(new object[]{ userId, item.Code, item.Count });
            });

            // 아이템 Insert
            var affectedRow = await _queryFactory.Query("farm_item").InsertAsync(columns, data);
            if (affectedRow != _masterDb._defaultFarmItemList!.Count)
            {
                throw new Exception();
            }
        }
        catch
        {
            foreach (var query in rollbackQuerys)
            {
                await query.DeleteAsync();
            }

            return ErrorCode.InsertDefaultItemFail;
        }

        return ErrorCode.None;
    }

    //public async Task<bool> TryChangeNickname(string playerId, string nickname)
    //{
    //    try
    //    {
    //        var result = await _queryFactory.Query("user_basicinfo").Where("PlayerId", playerId).UpdateAsync(new { Nickname = nickname });
    //        return result > 0;
    //    }
    //    catch
    //    {
    //        return false;
    //    }
    //}

    //public async Task<bool> CheckNicknameExists(string nickname)
    //{
    //    var result = await _queryFactory.Query("user_basicinfo").Select("UserId").Where("Nickname", nickname).GetAsync<Int64>();

    //    return result.Count() > 0;
    //}

    public async Task<bool> CheckPlayerIdExists(string playerId)
    {
        var result = await _queryFactory.Query("user_basicinfo").Select("UserId").Where("PlayerId", playerId).GetAsync<Int64>();

        return result.Count() > 0;
    }

    //public async Task<Int32> UpdateLastLoginAt(Int64 userId)
    //{
    //    var result = await _queryFactory.Query("user_basicinfo").Where("UserId", userId).UpdateAsync(new { LastLoginAt = DateTime.Now });

    //    return result;
    //}
}