using Microsoft.AspNetCore.Mvc;
using ZLogger;

public partial class LoadDataController : ControllerBase
{
    bool SuccessOrLogDebug<TPayload>(ErrorCode errorCode, TPayload payload)
    {
        if (errorCode == ErrorCode.None)
        {
            return true;
        }
        else
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), payload, "Failed");
            return false;
        }
    }

    void LogInfoOnSuccess<TPayload>(string method, TPayload payload)
    {
        _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method), payload, "Statistic");
    }
}
