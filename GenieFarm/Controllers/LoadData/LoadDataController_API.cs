using Microsoft.AspNetCore.Mvc;
using ZLogger;

[ApiController]
[Route("api/load")]

public partial class LoadDataController : ControllerBase
{
    ILogger<LoadDataController> _logger;
    IGameDb _gameDb;
    IMasterDb _masterDb;
    ILoadDataService _loadDataService;

    public LoadDataController(ILogger<LoadDataController> logger, IGameDb gameDb, IMasterDb masterDb, ILoadDataService loadDataService)
    {
        _logger = logger;
        _gameDb = gameDb;
        _masterDb = masterDb;
        _loadDataService = loadDataService;
    }

    [HttpPost("defaultData")]
    public async Task<ResDefaultDataDTO> LoadDefaultData(ReqDefaultDataDTO request)
    {
        // 게임 데이터 로드
        (var defaultDataResult, var defaultData) = await _loadDataService.GetDefaultGameData(request.UserID);
        if (!SuccessOrLogDebug(defaultDataResult, new { UserID = request.UserID }))
        {
            return new ResDefaultDataDTO() { Result = ErrorCode.LoadDefaultData_Fail };
        }

        LogInfoOnSuccess("LoadDefaultData", new { UserID = request.UserID });
        return new ResDefaultDataDTO() { Result = ErrorCode.None, DefaultData = defaultData };
    }

    [HttpPost("attendData")]
    public async Task<ResAttendDataDTO> LoadAttendData(ReqAttendDataDTO request)
    {
        // 출석 데이터 로드
        (var attendDataResult, var attendData) = await _loadDataService.GetAttendanceDataByUserId(request.UserID);
        if (!SuccessOrLogDebug(attendDataResult, new { UserID = request.UserID }))
        {
            return new ResAttendDataDTO() { Result = ErrorCode.LoadAttendData_Fail };
        }

        // 마스터DB에서 출석 보상 로드
        var monthlyRewardList = _masterDb._attendanceRewardList;

        LogInfoOnSuccess("LoadAttendData", new { UserID = request.UserID });
        return new ResAttendDataDTO() { Result = ErrorCode.None, MonthlyRewardList = monthlyRewardList, AttendData = attendData };
    }
}