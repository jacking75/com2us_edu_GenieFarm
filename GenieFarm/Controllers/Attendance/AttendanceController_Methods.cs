using ZLogger;

public partial class AttendanceController
{
    bool ValidateAttend(AttendanceModel attendData)
    {
        // 오늘 이미 출석했는지 체크
        if (!IsAnotherDay(attendData.LastAttendance))
        {
            return false;
        }

        return true;
    }

    bool IsAnotherDay(DateTime lastAttendDate)
    {
        // 현재 날짜와 마지막 출석 날짜가 1일 이상 차이가 나면 True
        var currentDate = DateTime.Parse(DateTime.Now.ToShortDateString());
        lastAttendDate = DateTime.Parse(lastAttendDate.ToShortDateString());
        var diff = currentDate - lastAttendDate;

        return diff > TimeSpan.FromDays(0);
    }

    void LogInfoOnSuccess<TPayload>(string method, TPayload payload)
    {
        _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method), payload, "Statistic");
    }

    bool Successed(ErrorCode errorCode)
    {
        return errorCode == ErrorCode.None;
    }
}
