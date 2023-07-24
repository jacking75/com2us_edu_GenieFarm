using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

public partial class AuthCheckController : ControllerBase
{
    void LogInfoOnSuccess<TPayload>(string method, TPayload payload)
    {
        _logger.ZLogInformationWithPayload(EventIdGenerator.Create(0, method), payload, "Statistic");
    }

    async Task<bool> AuthCheckToHive(string playerID, string authToken)
    {
        try
        {
            // 인증 요청
            HttpClient client = new();
            var hiveResponse = await client.PostAsJsonAsync(_hiveServerUrl, new { AuthID = playerID, AuthToken = authToken });

            // 응답 체크
            if (hiveResponse == null || hiveResponse.StatusCode != HttpStatusCode.OK)
            {
                var statusCode = hiveResponse == null ? 0 : hiveResponse.StatusCode;

                var errorCode = ErrorCode.Hive_Fail_InvalidResponse;

                _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode),
                                             new
                                             {
                                                 PlayerID = playerID,
                                                 AuthToken = authToken,
                                                 StatusCode = statusCode
                                             }, "Failed");

                return false;
            }

            // 인증 정보(ErrorCode) 체크
            var authResult = await hiveResponse.Content.ReadFromJsonAsync<ErrorCodeDTO>();
            if (authResult == null || authResult.Result != ErrorCode.None)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            var errorCode = ErrorCode.Hive_Fail_AuthCheckException;

            _logger.ZLogDebugWithPayload(EventIdGenerator.Create(errorCode), ex,
                                         new { PlayerID = playerID, AuthToken = authToken }, "Failed");

            return false;
        }
    }

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

    public class Security
    {
        private const string AllowableCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static string CreateAuthToken()
        {
            // 랜덤하게 토큰을 생성
            var bytes = new Byte[25];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }

            return new string(bytes.Select(x => AllowableCharacters[x % AllowableCharacters.Length]).ToArray());
        }
    }
}
