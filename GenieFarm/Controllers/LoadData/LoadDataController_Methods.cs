using Microsoft.AspNetCore.Mvc;
using ZLogger;

public partial class LoadDataController : ControllerBase
{
    ILogger<LoadDataController> _logger;
    IGameDb _gameDb;

    public LoadDataController(ILogger<LoadDataController> logger, IGameDb gameDb)
    {
        _logger = logger;
        _gameDb = gameDb;
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
