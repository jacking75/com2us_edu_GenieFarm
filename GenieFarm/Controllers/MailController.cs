using Microsoft.AspNetCore.Mvc;
using ZLogger;

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
            return new ResLoadPageDTO() { Result = ErrorCode.LoadMailListByPage_Fail_InvalidPage, MailList = null };
        }

        LogInfoOnSuccess("LoadMailListByPage", new { UserID = request.UserID, Page = request.Page });
        return new ResLoadPageDTO() { Result = ErrorCode.None, MailList = mailList };
    }

    /// <summary>
    /// ���� ���� ��ȸ API <br />
    /// Ŭ���̾�Ʈ�� ��û�� ���� �����͸� ��ȯ�ϰ�, ���� ó���մϴ�.
    /// </summary>
    [HttpPost("load")]
    public async Task<ResLoadMailDTO> LoadMail(ReqLoadMailDTO request)
    {
        // ���� ID�� �ش��ϴ� ���� �����͸� �ε��ϰ� ���� ó��
        (var errorCode, var mail) = await _mailService.GetMailAndSetRead(request.UserID, request.MailID);
        if (!Successed(errorCode))
        {
            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                        new { UserID = request.UserID, MailID = request.MailID },
                                        "Failed");

            return new ResLoadMailDTO() { Result = ErrorCode.LoadMail_Fail, Mail = null };
        }

        LogInfoOnSuccess("LoadMail", new { UserID = request.UserID, MailID = request.MailID });
        return new ResLoadMailDTO() { Result = ErrorCode.None, Mail = mail };
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