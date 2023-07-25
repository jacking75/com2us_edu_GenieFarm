public interface ILoadDataService
{
    /// <summary>
    /// 유저ID를 이용해 기본 게임 데이터를 로드합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultGameData(Int64 userId);

    /// <summary>
    /// 유저ID를 이용해 출석 데이터를 로드합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceDataByUserId(Int64 userId);
}
