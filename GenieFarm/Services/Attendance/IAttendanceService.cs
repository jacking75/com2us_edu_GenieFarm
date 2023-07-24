public interface IAttendanceService
{
    /// <summary>
    /// 유저ID를 기준으로 출석 데이터를 로드한다.
    /// </summary>
    /// <param name="userId">출석 데이터를 조회할 유저ID</param>
    /// <returns></returns>
    public Task<Tuple<ErrorCode, AttendanceModel?>> GetAttendanceData(Int64 userId);

    /// <summary>
    /// 유저ID를 현재 시각 기준으로 출석 처리한다.
    /// </summary>
    /// <param name="userId">출석 처리할 유저ID</param>
    /// <param name="attendanceData">현재 시점의 유저 출석 정보</param>
    /// <returns></returns>
    public Task<ErrorCode> Attend(Int64 userId, AttendanceModel attendanceData);
}
