using SqlKata.Execution;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using ZLogger;

public partial class GameDb : IGameDb
{
    readonly ILogger<GameDb> _logger;
    readonly IConfiguration _configuration;
    readonly MySqlConnection _dbConn;
    QueryFactory _queryFactory;

    public GameDb(ILogger<GameDb> Logger, IConfiguration configuration)
    {
        _logger = Logger;
        _configuration = configuration;

        var DbConnectString = _configuration.GetSection("DBConnection")["GameDb"];
        _dbConn = new MySqlConnection(DbConnectString);
        _dbConn.Open();

        var compiler = new SqlKata.Compilers.MySqlCompiler();
        _queryFactory = new SqlKata.Execution.QueryFactory(_dbConn, compiler);
    }

    public async Task<AccountModel> GetDefaultDataByAuthId(String authId)
    {
        // AuthId가 일치하는 행의 User Basic Information을 가져온다.
        var query = _queryFactory.Query("user_basicinfo").Where("AuthId", authId);
        var result = await query.GetAsync<AccountModel>();

        return result.FirstOrDefault();
    }

    public async Task<ErrorCode> CreateDefaultData(String authId, String nickname)
    {
        var userId = (Int64)0;

        // 기본 유저 데이터 생성
        try
        {
            userId = await _queryFactory.Query("user_basicinfo").InsertGetIdAsync<Int64>(new { AuthId = authId, Nickname = nickname });
        }
        catch
        {
            return ErrorCode.DuplicateNickname;
        }

        // 유저 출석 정보 생성
        try
        {
            var attendanceInsert = _queryFactory.Query("user_attendance").InsertAsync(new { UserId = userId, AttendanceCount = 0 });
        }
        catch
        {
            // 기본 유저 데이터 롤백
            await _queryFactory.Query("user_basicinfo").Where("UserId", userId).DeleteAsync();

            return ErrorCode.RegisterAttendanceException;
        }

        return ErrorCode.None;
    }

    public async Task<Boolean> TryChangeNickname(String authId, String nickname)
    {
        try
        {
            var result = _queryFactory.Query("user_basicinfo").Where("AuthId", authId).Update(new { Nickname = nickname });
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Boolean> CheckNicknameExists(String nickname)
    {
        var result = _queryFactory.Query("user_basicinfo").Select("UserId").Where("Nickname", nickname).Get<Int64>();

        return result.Count() > 0;
    }

    public async Task<Boolean> CheckAuthIdExists(String authId)
    {
        var result = _queryFactory.Query("user_basicinfo").Select("UserId").Where("AuthId", authId).Get<Int64>();

        return result.Count() > 0;
    }

    public async Task<Int32> UpdateLastLoginAt(Int64 userId)
    {
        var result = _queryFactory.Query("user_basicinfo").Where("UserId", userId).Update(new { LastLoginAt = DateTime.Now });

        return result;
    }
}