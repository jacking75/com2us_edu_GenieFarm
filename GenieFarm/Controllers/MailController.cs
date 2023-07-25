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
            return new ResLoadPageDTO() { Result = ErrorCode.LoadMailsByPage_InvalidPage, MailList = null };
        }

        LogInfoOnSuccess("LoadMailListByPage", new { UserID = request.UserID, Page = request.Page });
        return new ResLoadPageDTO() { Result = ErrorCode.None, MailList = mailList };
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