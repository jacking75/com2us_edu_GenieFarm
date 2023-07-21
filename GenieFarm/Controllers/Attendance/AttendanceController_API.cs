using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/attend")]
public partial class AttendanceController : ControllerBase
{
    ILogger<AttendanceController> _logger;
    IGameDb _gameDb;

    public AttendanceController(ILogger<AttendanceController> logger, IGameDb gameDb)
    {
        _logger = logger;
        _gameDb = gameDb;
    }

    [HttpPost]
    public async Task<ResAttendDTO> Attend(ReqAttendDTO request)
    {
        // 출석 데이터 로드
        (var getAttendDataResult, var attendData) = await _gameDb.GetAttendanceDataByUserId(request.UserID);
        if (getAttendDataResult != ErrorCode.None)
        {
            return new ResAttendDTO() { Result = ErrorCode.Attend_Fail_GetAttendData };
        }

        // 출석 가능한지 확인
        var validateResult = ValidateAttend(attendData!);
        if (validateResult != ErrorCode.None)
        {
            return new ResAttendDTO() { Result = validateResult };
        }

        // 출석 체크 및 보상 지급
        var attendResult = await _gameDb.Attend(request.UserID, attendData!);
        if (attendResult != ErrorCode.None)
        {
            return new ResAttendDTO() { Result = ErrorCode.Attend_Fail_AttendException };
        }

        LogResult(ErrorCode.None, "Attend", request.UserID, request.AuthToken);
        return new ResAttendDTO() { Result = ErrorCode.None };
    }
}