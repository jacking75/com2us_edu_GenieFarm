using MySqlX.XDevAPI.Common;

/// <summary>
/// 데이터 로드와 관련된 비즈니스 로직을 처리하고 <br/>
/// DB Operation Call을 수행하는 서비스 클래스
/// </summary>
public class LoadDataService : ILoadDataService
{
    readonly ILogger<LoadDataService> _logger;
    readonly IGameDb _gameDb;

    public LoadDataService(ILogger<LoadDataService> logger, IGameDb gameDb)
    {
        _logger = logger;
        _gameDb = gameDb;
    }

    /// <summary>
    /// 유저ID를 이용해 기본 게임 데이터를 로드합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultGameData(long userId)
    {
        var result = new DefaultDataDTO();

        // 기본 유저 정보 로드
        result.UserData = await _gameDb.GetDefaultUserDataByUserId(userId);
        if (result.UserData == null)
        {
            return new (ErrorCode.LoadDataService_GetDefaultGameDataByUserId_UserData, null);
        }

        // 출석 정보 로드
        result.AttendData = await _gameDb.GetDefaultAttendDataByUserId(userId);
        if (result.AttendData == null)
        {
            return new (ErrorCode.LoadDataService_GetDefaultGameDataByUserId_AttendData, null);
        }

        // 농장 기본 정보 로드
        result.FarmInfoData = await _gameDb.GetDefaultFarmDataByUserId(userId);
        if (result.FarmInfoData == null)
        {
            return new (ErrorCode.LoadDataService_GetDefaultGameDataByUserId_FarmData, null);
        }

        return new (ErrorCode.None, result);
    }

    /// <summary>
    /// 유저ID를 이용해 출석 데이터를 로드합니다.
    /// </summary>
    public async Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceDataByUserId(Int64 userId)
    {
        // 출석 정보 로드
        var attendanceData = await _gameDb.GetDefaultAttendDataByUserId(userId);
        if (attendanceData == null)
        {
            return new (ErrorCode.LoadDataService_GetAttendanceDataByUserId, null);
        }

        return new (ErrorCode.None, attendanceData);
    }
}
