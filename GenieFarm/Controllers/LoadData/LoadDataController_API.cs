﻿using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/load")]

public partial class LoadDataController : ControllerBase
{
    ILogger<LoadDataController> _logger;
    IGameDb _gameDb;

    public LoadDataController(ILogger<LoadDataController> logger, IGameDb gameDb)
    {
        _logger = logger;
        _gameDb = gameDb;
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
}