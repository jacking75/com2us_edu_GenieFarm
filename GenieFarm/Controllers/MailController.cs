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
    public async Task<ResMailOpenDTO> Open(ReqMailOpenDTO request)
    {
        // 페이지 번호가 유효한지 확인
        if (request.Page < 1)
        {
            return new ResMailOpenDTO() { Result = ErrorCode.InvalidMailPage, Page = request.Page };
        }

        // 페이지 번호에 해당하는 우편 리스트 로드
        var mailList = await _gameDb.OpenMail(request.UserID, request.Page);

        // 우편 리스트가 비어있다면 존재하지 않는 페이지
        if (mailList == null || mailList.Count == 0)
        {
            return new ResMailOpenDTO() { Result = ErrorCode.MailPageNotExists, Page = request.Page };
        }

        return new ResMailOpenDTO() { Result = ErrorCode.None, MailList = mailList, Page = request.Page };
    }


    [HttpPost("read")]
    public async Task<ResMailInfoDTO> Read(ReqMailInfoDTO request)
    {
        // 단일 우편 로드
        var mailData = await _gameDb.GetMail(request.UserID, request.MailID);

        // 우편이 존재하지 않음
        if (mailData == null)
        {
            return new ResMailInfoDTO() { Result = ErrorCode.MailNotExists };
        }

        return new ResMailInfoDTO() { Result = ErrorCode.None, Mail = mailData };
    }


    [HttpDelete("delete")]
    public async Task<ErrorCodeDTO> Delete(ReqMailDeleteDTO request)
    {
        // 우편 삭제
        if (!await _gameDb.DeleteMail(request.UserID, request.MailID))
        {
            return new ErrorCodeDTO() { Result = ErrorCode.MailNotExists };
        }

        return new ErrorCodeDTO() { Result = ErrorCode.None };
    }


    [HttpPost("send")]
    public async Task<ErrorCodeDTO> Send(ReqMailSendDTO request)
    {
        var errorCode = await _gameDb.SendMail(request);
        
        return new ErrorCodeDTO() { Result = errorCode };
    }
}