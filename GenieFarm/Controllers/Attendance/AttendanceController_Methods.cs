using ZLogger;

public partial class AttendanceController
{
    ErrorCode ValidateAttend(AttendanceModel attendData)
    {
        // 마지막 출석날짜의 연월이 다른 경우
        if (attendData.LastAttendance.Year != DateTime.Now.Year
            && attendData.LastAttendance.Month != DateTime.Now.Month)
        {
            return ErrorCode.None;
        }

        // 누적 출석일이 28일 미만인지
        if (attendData.AttendanceCount > 27)
        {
            return ErrorCode.Attend_Fail_ReceivedAllMonthlyRewards;
        }

        // 오늘 이미 출석했는지 체크
        if (!IsAnotherDay(attendData.LastAttendance))
        {
            return ErrorCode.Attend_Fail_AlreadyAttended;
        }

        return ErrorCode.None;
    }

    bool IsAnotherDay(DateTime lastAttendDate)
    {
        // 현재 날짜와 마지막 출석 날짜가 1일 이상 차이가 나면 True
        var currentDate = DateTime.Parse(DateTime.Now.ToShortDateString());
        lastAttendDate = DateTime.Parse(lastAttendDate.ToShortDateString());
        var diff = currentDate - lastAttendDate;

        return diff > TimeSpan.FromDays(0);
    }

    void LogResult(ErrorCode errorCode, string method, Int64 userId, string authToken)
    {
        if (errorCode != ErrorCode.None)
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create((UInt16)errorCode, method),
                                         new { UserID = userId, AuthToken = authToken }, "Failed");
        }
        else
        {
            _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method),
                                               new { UserID = userId, AuthToken = authToken }, "Statistic");
        }
    }
}
