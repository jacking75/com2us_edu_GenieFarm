using Microsoft.AspNetCore.Mvc;

namespace WebAPIServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MailController : ControllerBase
{
    readonly ILogger<MailController> _logger;
    readonly IGameDb _gameDb;

    public MailController(ILogger<MailController> Logger, IGameDb GameDb)
    {
        _logger = Logger;
        _gameDb = GameDb;
    }

    [HttpPost("open")]
    public async Task<MailOpenResponse> Open(MailOpenRequest request)
    {
        if (request.Page < 1)
        {
            return new MailOpenResponse() { Result = ErrorCode.InvalidMailPage, Page = request.Page };
        }
        List<MailData> mailList = await _gameDb.OpenMail(request.UserID, request.Page);
        if (mailList == null || mailList.Count == 0)
        {
            return new MailOpenResponse() { Result = ErrorCode.MailOpenException, Page = request.Page };
        }
        return new MailOpenResponse() { Result = ErrorCode.None, Mails = mailList, Page = request.Page };
    }

    [HttpPost("read")]
    public async Task<MailDataResponse> Read(MailDataRequest request)
    {
        MailData mailData = await _gameDb.GetMail(request.UserID, request.MailID);
        if (mailData == null)
        {
            return new MailDataResponse() { Result = ErrorCode.MailNotExists };
        }
        return new MailDataResponse() { Result = ErrorCode.None, Mail = mailData };
    }

    [HttpDelete("delete")]
    public async Task<ResultResponse> Delete(MailDeleteRequest request)
    {
        if (!await _gameDb.DeleteMail(request.UserID, request.MailID))
        {
            return new ResultResponse() { Result = ErrorCode.MailNotExists };
        }
        return new ResultResponse() { Result = ErrorCode.None };
    }

    [HttpPost("send")]
    public async Task<ResultResponse> Send(MailSendRequest request)
    {
        try
        {
            await _gameDb.SendMail(request);
            return new ResultResponse() { Result = ErrorCode.None };
        }
        catch
        {
            return new ResultResponse() { Result = ErrorCode.MailSendException };
        }
    }
}