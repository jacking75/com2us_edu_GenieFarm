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
    /// 우편함 페이지 별 조회 API <br/>
    /// 클라이언트가 요청한 페이지에 해당하는 우편 리스트를 반환합니다.
    /// </summary>
    [HttpPost("loadByPage")]
    public async Task<ResLoadPageDTO> LoadMailListByPage(ReqLoadPageDTO request)
    {
        // 페이지 번호에 해당하는 우편 리스트 로드
        (var errorCode, var mailList) = await _mailService.GetMailListByPage(request.UserID, request.Page);
        if (!Successed(errorCode))
        {
            return new ResLoadPageDTO() { Result = ErrorCode.LoadMailListByPage_Fail_InvalidPage, MailList = null };
        }

        LogInfoOnSuccess("LoadMailListByPage", new { UserID = request.UserID, Page = request.Page });
        return new ResLoadPageDTO() { Result = ErrorCode.None, MailList = mailList };
    }

    /// <summary>
    /// 개별 우편 조회 API <br />
    /// 클라이언트가 요청한 메일 데이터를 반환하고, 읽음 처리합니다.
    /// </summary>
    [HttpPost("load")]
    public async Task<ResLoadMailDTO> LoadMail(ReqLoadMailDTO request)
    {
        // 메일 ID에 해당하는 우편 데이터를 로드하고 읽음 처리
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
    /// 성공한 API 요청에 대해 통계용 로그를 남깁니다.
    /// </summary>
    void LogInfoOnSuccess<TPayload>(string method, TPayload payload)
    {
        _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method), payload, "Statistic");
    }

    /// <summary>
    /// 에러가 없는지 체크합니다.
    /// </summary>
    bool Successed(ErrorCode errorCode)
    {
        return errorCode == ErrorCode.None;
    }
}