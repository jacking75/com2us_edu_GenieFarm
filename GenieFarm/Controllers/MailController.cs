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
        // ������ ��ȣ�� ��ȿ���� Ȯ��
        if (request.Page < 1)
        {
            return new ResMailOpenDTO() { Result = ErrorCode.InvalidMailPage, Page = request.Page };
        }

        // ������ ��ȣ�� �ش��ϴ� ���� ����Ʈ �ε�
        var mailList = await _gameDb.OpenMail(request.UserID, request.Page);

        // ���� ����Ʈ�� ����ִٸ� �������� �ʴ� ������
        if (mailList == null || mailList.Count == 0)
        {
            return new ResMailOpenDTO() { Result = ErrorCode.MailPageNotExists, Page = request.Page };
        }

        return new ResMailOpenDTO() { Result = ErrorCode.None, MailList = mailList, Page = request.Page };
    }


    [HttpPost("read")]
    public async Task<ResMailInfoDTO> Read(ReqMailInfoDTO request)
    {
        // ���� ���� �ε�
        var mailData = await _gameDb.GetMail(request.UserID, request.MailID);

        // ������ �������� ����
        if (mailData == null)
        {
            return new ResMailInfoDTO() { Result = ErrorCode.MailNotExists };
        }

        return new ResMailInfoDTO() { Result = ErrorCode.None, Mail = mailData };
    }


    [HttpDelete("delete")]
    public async Task<ErrorCodeDTO> Delete(ReqMailDeleteDTO request)
    {
        // ���� ����
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