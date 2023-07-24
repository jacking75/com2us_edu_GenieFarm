using MySqlX.XDevAPI.Common;

public class LoadDataService : ILoadDataService
{
    readonly ILogger<LoadDataService> _logger;
    readonly IGameDb _gameDb;

    public LoadDataService(ILogger<LoadDataService> logger, IGameDb gameDb)
    {
        _logger = logger;
        _gameDb = gameDb;
    }

    public async Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultGameData(long userId)
    {
        var result = new DefaultDataDTO();

        // 기본 유저 정보 로드
        result.UserData = await _gameDb.GetDefaultUserDataByUserId(userId);
        if (result.UserData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.LoadDataService_GetDefaultGameDataByUserId_UserData, null);
        }

        // 출석 정보 로드
        result.AttendData = await _gameDb.GetDefaultAttendDataByUserId(userId);
        if (result.AttendData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.LoadDataService_GetDefaultGameDataByUserId_AttendData, null);
        }

        // 농장 기본 정보 로드
        result.FarmInfoData = await _gameDb.GetDefaultFarmDataByUserId(userId);
        if (result.FarmInfoData == null)
        {
            return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.LoadDataService_GetDefaultGameDataByUserId_FarmData, null);
        }

        return new Tuple<ErrorCode, DefaultDataDTO?>(ErrorCode.None, result);
    }

    public async Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceDataByUserId(Int64 userId)
    {
        // 출석 정보 로드
        var attendanceData = await _gameDb.GetDefaultAttendDataByUserId(userId);
        if (attendanceData == null)
        {
            return new Tuple<ErrorCode, AttendanceModel?>(ErrorCode.LoadDataService_GetAttendanceDataByUserId, null);
        }

        return new Tuple<ErrorCode, AttendanceModel?>(ErrorCode.None, attendanceData);
    }
}
