using Microsoft.AspNetCore.Mvc;
using ZLogger;

public partial class LoadDataController : ControllerBase
{
    bool Successed(ErrorCode errorCode)
    {
        return errorCode == ErrorCode.None;
    }

    void LogInfoOnSuccess<TPayload>(string method, TPayload payload)
    {
        _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method), payload, "Statistic");
    }
}
