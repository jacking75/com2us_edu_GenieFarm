using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/load")]

public partial class LoadDataController : ControllerBase
{
    ILogger<LoadDataController> _logger;
    IGameDb _gameDb;
    IMasterDb _masterDb;

    public LoadDataController(ILogger<LoadDataController> logger, IGameDb gameDb, IMasterDb masterDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _masterDb = masterDb;
    }

    [HttpPost("defaultData")]
    public async Task<ResDefaultDataDTO> LoadDefaultData(ReqDefaultDataDTO request)
    {
        // 게임 데이터 로드
        (var defaultDataResult, var defaultData) = await _gameDb.GetDefaultDataByUserId(request.UserID);
        if (defaultDataResult != ErrorCode.None)
        {
            return new ResDefaultDataDTO() { Result = defaultDataResult };
        }

        LogResult(ErrorCode.None, "LoadDefaultData", request.UserID, request.AuthToken);
        return new ResDefaultDataDTO() { Result = ErrorCode.None, DefaultData = defaultData };
    }

    [HttpPost("attendData")]
    public async Task<ResAttendDataDTO> LoadAttendData(ReqAttendDataDTO request)
    {
        var monthlyRewardList = _masterDb._attendanceRewardList;

        // 출석 데이터 로드
        (var attendDataResult, var attendData) = await _gameDb.GetAttendanceDataByUserId(request.UserID);
        if (attendDataResult != ErrorCode.None)
        {
            return new ResAttendDataDTO() { Result = attendDataResult };
        }

        LogResult(ErrorCode.None, "LoadAttendData", request.UserID, request.AuthToken);
        return new ResAttendDataDTO() { Result = ErrorCode.None, MonthlyRewardList = monthlyRewardList, AttendData = attendData };
    }
}