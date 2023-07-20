using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using ZLogger;
using System.Collections;
using Org.BouncyCastle.Asn1.Ocsp;

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

    public async Task<ErrorCode> CreateDefaultData(string playerId, string nickname)
    {
        var rollbackQuerys = new List<SqlKata.Query>();

        // 기본 유저 데이터 생성
        (var defaultUserDataResult, var userId) = await CreateDefaultUserData(playerId, nickname, rollbackQuerys);
        if (defaultUserDataResult != ErrorCode.None)
        {
            return defaultUserDataResult;
        }

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

    public async Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultDataByPlayerId(string playerId)
    {
        var result = new DefaultDataDTO();

        // 기본 유저 정보 로드
        result.UserData = await GetUserData(playerId);
        if (result.UserData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.Account_Fail_UserInfoNotExists, null);
        }

        // 출석부 정보 로드
        result.AttendData = await GetAttendData(result.UserData.UserId);
        if (result.AttendData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.Account_Fail_AttendDataNotExists, null);
        }

        // 농장 기본 정보 로드
        result.FarmInfoData = await GetFarmInfoData(result.UserData.UserId);
        if (result.FarmInfoData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.Account_Fail_FarmInfoNotExists, null);
        }

        return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.None, result);
    }

    public async Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultDataByUserId(Int64 userId)
    {
        var result = new DefaultDataDTO();

        // 기본 유저 정보 로드
        result.UserData = await GetUserDataByUserId(userId);
        if (result.UserData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.Account_Fail_UserInfoNotExists, null);
        }

        // 출석부 정보 로드
        result.AttendData = await GetAttendData(userId);
        if (result.AttendData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.Account_Fail_AttendDataNotExists, null);
        }

        // 농장 기본 정보 로드
        result.FarmInfoData = await GetFarmInfoData(userId);
        if (result.FarmInfoData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.Account_Fail_FarmInfoNotExists, null);
        }

        return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.None, result);
    }

    public async Task<Int64> GetUserIdByPlayerId(string playerId)
    {
        var result = (await _queryFactory.Query("user_basicinfo")
                                         .Select("UserId").Where("PlayerId", playerId)
                                         .GetAsync<Int64>()).FirstOrDefault();

        return result;
    }

    public async Task<bool> UpdateLastLoginAt(Int64 userId)
    {
        var result = await _queryFactory.Query("user_basicinfo").Where("UserId", userId)
                                        .UpdateAsync(new { LastLoginAt = DateTime.Now });

        if (result < 1)
        {
            var errorCode = ErrorCode.Account_Fail_UpdateLastLogin;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         new { UserID = userId }, "Failed");

            return false;
        }

        return true;
    }

    async Task<Tuple<ErrorCode, Int64>> CreateDefaultUserData(string playerId, string nickname, List<SqlKata.Query> rollbackQuerys)
    {
        try
        {
            var userId = await _queryFactory.Query("user_basicinfo")
                                            .InsertGetIdAsync<Int64>(new { PlayerId = playerId, Nickname = nickname });

            rollbackQuerys.Add(_queryFactory.Query("user_basicinfo").Where("UserId", userId));

            return new Tuple<ErrorCode, Int64>(ErrorCode.None, userId);
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.Account_Fail_DuplicateNickname;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { PlayerID = playerId, Nickname = "nickname" }, "Failed");

            return new Tuple<ErrorCode, Int64>(errorCode, 0);
        }
    }

    async Task<ErrorCode> CreateDefaultAttendanceData(Int64 userId, List<SqlKata.Query> rollbackQuerys)
    {
        try
        {
            var attendanceInsert = await _queryFactory.Query("user_attendance")
                                                      .InsertAsync(new { UserId = userId, AttendanceCount = 0 });

            rollbackQuerys.Add(_queryFactory.Query("user_attendance").Where("UserId", userId));

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.Account_Fail_CreateDefaultAttendanceData;

            await Rollback(errorCode, rollbackQuerys);

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserId = userId }, "Failed");

            return errorCode;
        }
    }

    async Task<ErrorCode> CreateDefaultFarmData(Int64 userId, List<SqlKata.Query> rollbackQuerys)
    {
        try
        {
            // 마스터 DB의 Default 데이터를 가져와 생성
            var defaultData = _masterDb._defaultFarmData!;
            await _queryFactory.Query("farm_info")
                               .InsertAsync(new { UserId = userId,
                                                  FarmLevel = defaultData.DefaultLevel,
                                                  MaxStorage = defaultData.DefaultStorage,
                                                  Love = defaultData.DefaultLove,
                                                  Money = defaultData.DefaultMoney });

            rollbackQuerys.Add(_queryFactory.Query("farm_info").Where("UserId", userId));

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.Account_Fail_CreateDefaultFarmData;

            await Rollback(errorCode, rollbackQuerys);

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserId = userId }, "Failed");

            return errorCode;
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
                affectedRow += await _queryFactory.Query("farm_item")
                                                  .InsertAsync(new {
                                                      OwnerId = userId,
                                                      ItemCode = item.Code,
                                                      ItemCount = item.Count });
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
            var errorCode = ErrorCode.Account_Fail_InsertDefaultItem;

            await Rollback(errorCode, rollbackQuerys);

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { UserId = userId }, "Failed");

            return errorCode;
        }
    }

    async Task Rollback(ErrorCode errorCode, List<SqlKata.Query> rollbackQuerys)
    {
        foreach (var query in rollbackQuerys)
        {
            var affectedRowCount = await query.DeleteAsync();

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                         new { Query = _queryFactory.Compiler.Compile(query).RawSql,
                                               AffectedRowCount = affectedRowCount }, "Rollback");
        }
    }

    async Task<AccountModel?> GetUserData(string playerId)
    {
        var userData = (await _queryFactory.Query("user_basicinfo").Where("PlayerId", playerId)
                                           .GetAsync<AccountModel>()).FirstOrDefault();

        return userData;
    }
    
    async Task<AccountModel?> GetUserDataByUserId(Int64 userId)
    {
        var userData = (await _queryFactory.Query("user_basicinfo").Where("UserId", userId)
                                           .GetAsync<AccountModel>()).FirstOrDefault();

        return userData;
    }

    async Task<AttendanceModel?> GetAttendData(Int64 userId)
    {
        var attendData = (await _queryFactory.Query("user_attendance").Where("userId", userId)
                                             .GetAsync<AttendanceModel>()).FirstOrDefault();

        return attendData;
    }

    async Task<FarmInfoModel?> GetFarmInfoData(Int64 userId)
    {
        var farmInfoData = (await _queryFactory.Query("farm_info").Where("userId", userId)
                                               .GetAsync<FarmInfoModel>()).FirstOrDefault();

        return farmInfoData;
    }
}