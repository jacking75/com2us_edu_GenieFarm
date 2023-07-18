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

    //public async Task<AccountModel?> GetDefaultDataByPlayerId(string playerId)
    //{
    //    // PlayerId가 일치하는 행의 User Basic Information을 가져온다.
    //    var query = _queryFactory.Query("user_basicinfo").Where("playerId", playerId);
    //    var result = (await query.GetAsync<AccountModel>()).FirstOrDefault();

    //    return result;
    //}

    public async Task<ErrorCode> CreateDefaultData(string playerId, string nickname)
    {
        var rollbackQuerys = new List<SqlKata.Query>();
        var userId = (Int64)0;

        // 기본 유저 데이터 생성
        var defaultUserDataResult = await CreateDefaultUserData(playerId, nickname, rollbackQuerys);
        if (defaultUserDataResult.Item1 != ErrorCode.None)
        {
            return ErrorCode.DuplicateNickname;
        }
        userId = defaultUserDataResult.Item2;

        // 유저 출석 정보 생성
        var attendanceInsertResult = await CreateDefaultAttendanceData(userId, rollbackQuerys);
        if (attendanceInsertResult != ErrorCode.None)
        {
            return attendanceInsertResult;
        }

        // 농장 기본 정보 생성
        var defaultFarmDataResult = await CreateDefaultFarmData(userId, rollbackQuerys);
        if (defaultFarmDataResult != ErrorCode.None)
        {
            return defaultFarmDataResult;
        }

        // 유저 기본 아이템 생성
        var defaultFarmItemResult = await InsertDefaultItem(userId, rollbackQuerys);
        if (defaultFarmItemResult != ErrorCode.None)
        {
            return defaultFarmItemResult;
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

    async Task<Tuple<ErrorCode, Int64>> CreateDefaultUserData(string playerId, string nickname, List<SqlKata.Query> rollbackQuerys)
    {
        try
        {
            var userId = await _queryFactory.Query("user_basicinfo").InsertGetIdAsync<Int64>(new { PlayerId = playerId, Nickname = nickname });

            rollbackQuerys.Add(_queryFactory.Query("user_basicinfo").Where("UserId", userId));
            return new Tuple<ErrorCode, Int64>(ErrorCode.None, userId);
        }
        catch
        {
            _logger.ZLogDebugWithPayload(new { Type = "CreateDefaultUserData", ErrorCode = ErrorCode.DuplicateNickname, PlayerID = playerId, Nickname = "nickname" }, "Failed");
            return new Tuple<ErrorCode, Int64>(ErrorCode.DuplicateNickname, 0);
        }
    }

    async Task<ErrorCode> CreateDefaultAttendanceData(Int64 userId, List<SqlKata.Query> rollbackQuerys)
    {
        try
        {
            var attendanceInsert = await _queryFactory.Query("user_attendance").InsertAsync(new { UserId = userId, AttendanceCount = 0 });

            rollbackQuerys.Add(_queryFactory.Query("user_attendance").Where("UserId", userId));
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            await Rollback(rollbackQuerys);

            _logger.ZLogDebugWithPayload(new { Type = "CreateDefaultAttendanceData", ErrorCode = ErrorCode.CreateDefaultAttendanceFailed, UserId = userId, Exception = ex.GetType().ToString() }, "Failed");
            return ErrorCode.CreateDefaultAttendanceFailed;
        }
    }

    async Task<ErrorCode> CreateDefaultFarmData(Int64 userId, List<SqlKata.Query> rollbackQuerys)
    {
        try
        {
            // 마스터 DB의 Default 데이터를 가져와 생성
            var defaultData = _masterDb._defaultFarmData!;
            await _queryFactory.Query("farm_info").InsertAsync(new { UserId = userId, FarmLevel = defaultData.DefaultLevel, MaxStorage = defaultData.DefaultStorage, Love = defaultData.DefaultLove, Money = defaultData.DefaultMoney });

            rollbackQuerys.Add(_queryFactory.Query("farm_info").Where("UserId", userId));
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            await Rollback(rollbackQuerys);

            _logger.ZLogDebugWithPayload(new { Type = "CreateDefaultFarmData", ErrorCode = ErrorCode.CreateDefaultFarmInfoFailed, UserId = userId, Exception = ex.GetType().ToString() }, "Failed");
            return ErrorCode.CreateDefaultFarmInfoFailed;
        }
    }

    async Task<ErrorCode> InsertDefaultItem(Int64 userId, List<SqlKata.Query> rollbackQuerys)
    {
        try
        {
            rollbackQuerys.Add(_queryFactory.Query("farm_item").Where("OwnerId", userId));

            // 마스터 DB의 Default 아이템 데이터를 가져와 Insert
            var affectedRow = 0;
            foreach (var item in _masterDb._defaultFarmItemList!)
            {
                affectedRow += await _queryFactory.Query("farm_item").InsertAsync(new { OwnerId = userId, ItemCode = item.Code, ItemCount = item.Count });
            }

            // Item 개수만큼 Insert되지 않았다면 롤백
            if (affectedRow != _masterDb._defaultFarmItemList!.Count)
            {
                throw new AffectedRowCountOutOfRangeException(affectedRow, _masterDb._defaultFarmItemList!.Count);
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            await Rollback(rollbackQuerys);

            if (ex.InnerException is AffectedRowCountOutOfRangeException)
            {
                _logger.ZLogDebugWithPayload(new { Type = "InsertDefaultItem", ErrorCode = ErrorCode.InsertDefaultItemFailed, UserId = userId, Exception = "AffectedRowCountOutOfRangeException" }, "Failed");
            } else
            {
                _logger.ZLogDebugWithPayload(new { Type = "InsertDefaultItem", ErrorCode = ErrorCode.InsertDefaultItemFailed, UserId = userId, Exception = ex.GetType().ToString() }, "Failed");
            }

            return ErrorCode.InsertDefaultItemFailed;
        }
    }

    async Task Rollback(List<SqlKata.Query> rollbackQuerys)
    {
        foreach (var query in rollbackQuerys)
        {
            var affectedRowCount = await query.DeleteAsync();
            _logger.ZLogDebugWithPayload(new { Type = "Rollback", Query = _queryFactory.Compiler.Compile(query).RawSql, AffectedRowCount = affectedRowCount }, "Rollback");
        }
    }
}