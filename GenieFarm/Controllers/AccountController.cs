using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using ZLogger;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    ILogger<AccountController> _logger;
    IGameDb _gameDb;
    IRedisDb _redisDb;
    Dictionary<string, string> _authUserData = new();


    public AccountController(ILogger<AccountController> logger, IGameDb gameDb, IRedisDb redisDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _redisDb = redisDb;

        InitDictionary();
    }


    [HttpPost("login")]
    public async Task<ResLoginDTO> Login(ReqLoginDTO request)
    {

        // 플랫폼 서버에서 받아온 인증ID와 인증토큰 체크
        if (!AuthCheck(request.AuthID, request.AuthToken))
        {
            return new ResLoginDTO() { Result = ErrorCode.AuthCheckFail };
        }
        
        // 앱 버전, 마스터 데이터 버전 체크
        if (!VersionCheck(request.AppVersion, request.MasterDataVersion))
        {
            return new ResLoginDTO() { Result = ErrorCode.InvalidVersion };
        }

        // GameDB에 기본 게임 데이터가 있는지 체크
        var accountData = await _gameDb.GetDefaultDataByAuthId(request.AuthID);
        if (accountData == null)
        {
            return new ResLoginDTO() { Result = ErrorCode.UserNotExists };
        }

        // Redis 토큰 세팅
        if (ErrorCode.None != await SetTokenOnRedis(request.AuthID, request.AuthToken, accountData.UserId))
        {
            return new ResLoginDTO() { Result = ErrorCode.SessionSettingFail };
        }

        // 최종 로그인 시각 변경
        await _gameDb.UpdateLastLoginAt(accountData.UserId);

        // 유저 데이터 반환
        return new ResLoginDTO() { Result = ErrorCode.None, UserData = accountData };
    }


    [HttpPost("register")]
    public async Task<ResRegisterDTO> Register(ReqRegisterDTO request)
    {
        // 플랫폼 서버에서 받아온 인증ID와 인증토큰 체크
        if (!AuthCheck(request.AuthID, request.AuthToken))
        {
            return new ResRegisterDTO() { Result = ErrorCode.AuthCheckFail };
        }

        // GameDB에 해당 인증ID가 이미 존재하는지 확인
        if (await _gameDb.CheckAuthIdExists(request.AuthID))
        {
            return new ResRegisterDTO() { Result = ErrorCode.UserAlreadyExists };
        }

        // GameDB에 기본 게임 데이터 생성
        var errorCode = await _gameDb.CreateDefaultData(request.AuthID, request.Nickname);

        return new ResRegisterDTO() { Result = errorCode };
    }


    [HttpPost("logout")]
    public async Task<ResLogoutDTO> Logout(ReqLogoutDTO request)
    {
        if (!await _redisDb.DeleteSessionDataAsync(request.AuthID, request.AuthToken, request.UserID))
        {
            return new ResLogoutDTO() { Result = ErrorCode.LogoutFail };
        }

        return new ResLogoutDTO() { Result = ErrorCode.None };
    }


    [HttpPut("nickname")]
    public async Task<ResChangeNicknameDTO> ChangeNickname(ReqChangeNicknameDTO request)
    {
        // 유저 닉네임 변경
        if (!(await _gameDb.TryChangeNickname(request.AuthID, request.Nickname)))
        {
            // 중복 닉네임
            return new ResChangeNicknameDTO() { Result = ErrorCode.DuplicateNickname };
        }

        // 변경 성공
        return new ResChangeNicknameDTO() { Result = ErrorCode.None };
    }


    void InitDictionary()
    {
        _authUserData.Add("test01", "DUWPQCFN5DQF4P");
        _authUserData.Add("test02", "DYG5R07M7RUV07");
        _authUserData.Add("test03", "5GZF7OFY05P4TT");
        _authUserData.Add("test04", "94ILRSD4LRXE6N");
        _authUserData.Add("test05", "GPKJ442KR1BK0U");
        _authUserData.Add("test06", "P2H95LNF6NT8UC");
        _authUserData.Add("test07", "JXOU845OYZJUXG");
        _authUserData.Add("test08", "N21SK6AXKQWS5B");
        _authUserData.Add("test09", "X7S4WCTKMY6YVK");
        _authUserData.Add("test10", "HIB0KU1A6FGVT1");
        _authUserData.Add("test11", "0HM20Q8A4GFCBX");
        _authUserData.Add("test12", "9IPHAAF6P88BMP");
        _authUserData.Add("test13", "D58RFSAAAP1RWG");
        _authUserData.Add("test14", "MYQOR56M574OIG");
        _authUserData.Add("test15", "M0A7BOS0CVVN5L");
        _authUserData.Add("test16", "0KJLTAMCVQBRLX");
        _authUserData.Add("test17", "1E4XH0PL1XRGI8");
        _authUserData.Add("test18", "FK4K9SYSB63L7R");
    }


    bool AuthCheck(string authID, string authToken)
    {
        // 인증ID와 인증토큰을 체크한다.
        return _authUserData.TryGetValue(authID, out var originAuthToken) && originAuthToken == authToken;
    }


    async Task<ErrorCode> SetTokenOnRedis(string authId, string authToken, Int64 userId)
    {
        // 같은 키의 토큰이 있어도 무조건 Overwrite하여 기존 토큰을 무효화
        if (!await _redisDb.SetAsync(authId, authToken, TimeSpan.FromDays(7)))
        {
            return ErrorCode.SessionSettingFail;
        }
        if (!await _redisDb.SetAsync(authToken, userId, TimeSpan.FromDays(7)))
        {
            return ErrorCode.SessionSettingFail;
        }

        return ErrorCode.None;
    }


    private bool VersionCheck(string appVersion, string masterDataVersion)
    {
        // TODO : MasterData로부터 버전 가져와서 체크 Condition 작성해야 함
        return true;
    }
}