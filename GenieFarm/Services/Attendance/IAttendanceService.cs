public interface IAttendanceService
{
    /// <summary>
    /// 유저ID를 기준으로 출석 데이터를 로드한다.
    /// </summary>
    public Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceData(Int64 userId);

    /// <summary>
    /// 유저ID를 현재 시각 기준으로 출석 처리한다.
    /// </summary>
    public Task<ErrorCode> Attend(Int64 userId, AttendanceModel attendanceData, bool usingPass);

    /// <summary>
    /// 유저ID를 이용해 월간 구독 이용권 만료일을 조회하고, <br/>
    /// 이용권이 유효하면 true를 반환한다.
    /// </summary>
    public Task<bool> CheckUsingPass(Int64 userId);
}
