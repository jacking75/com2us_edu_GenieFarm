using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace WebAPIServer.Controllers;

[ApiController]
[Route("api/mail")]
public class MailController : ControllerBase
{
    readonly ILogger<MailController> _logger;
    readonly IMailService _mailService;

    public MailController(ILogger<MailController> Logger, IMailService mailService)
    {
        _logger = Logger;
        _mailService = mailService;
    }

    /// <summary>
    /// ������ ������ �� ��ȸ API <br/>
    /// Ŭ���̾�Ʈ�� ��û�� �������� �ش��ϴ� ���� ����Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    [HttpPost("loadByPage")]
    public async Task<ResLoadPageDTO> LoadMailListByPage(ReqLoadPageDTO request)
    {
        // ������ ��ȣ�� �ش��ϴ� ���� ����Ʈ �ε�
        (var errorCode, var mailList) = await _mailService.GetMailListByPage(request.UserID, request.Page);
        if (!Successed(errorCode))
        {
            return new ResLoadPageDTO() { Result = ErrorCode.LoadMailsByPage_InvalidPage, MailList = null };
        }

        LogInfoOnSuccess("LoadMailListByPage", new { UserID = request.UserID, Page = request.Page });
        return new ResLoadPageDTO() { Result = ErrorCode.None, MailList = mailList };
    }

    /// <summary>
    /// ������ API ��û�� ���� ���� �α׸� ����ϴ�.
    /// </summary>
    void LogInfoOnSuccess<TPayload>(string method, TPayload payload)
    {
        _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method), payload, "Statistic");
    }

    /// <summary>
    /// ������ ������ üũ�մϴ�.
    /// </summary>
    bool Successed(ErrorCode errorCode)
    {
        return errorCode == ErrorCode.None;
    }
}