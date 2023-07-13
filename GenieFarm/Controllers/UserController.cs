using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    ILogger<UserController> _logger;
    IGameDb _gameDb;
    IRedisDb _redisDb;
    Dictionary<string, string> _authUserData = new();
    public UserController(ILogger<UserController> logger, IGameDb gameDb, IRedisDb redisDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _redisDb = redisDb;

        InitDictionary();
    }

    [HttpPost("login")]
    public async Task<LoginResponse> Login(LoginRequest request)
    {
        // 유저가 플랫폼 서버에서 받아온 인증ID와 인증토큰을 체크한다.
        // 인증 실패
        if (!AuthCheck(request.AuthID, request.AuthToken))
        {
            return new LoginResponse() { Result = ErrorCode.AuthCheckFail };
        }

        // TODO : 버전을 확인한다.

        // GameDB에 User의 Default 데이터가 있는지 확인한다.
        UserData userData;
        if ((userData = await _gameDb.GetDefaultDataByAuthId(request.AuthID)) == null)
        {
            // UserNotExists 에러코드를 전송해 가입을 유도한다.
            return new LoginResponse() { Result = ErrorCode.UserNotExists };
        }

        // Redis에 { 인증ID : 인증 토큰 } 쌍으로 저장한다.
        // 이전에 저장된 토큰이 있으면 덮어씌운다.
        await _redisDb.SetAsync(request.AuthID, request.AuthToken, TimeSpan.FromDays(7));
        // Redis에 { 인증 토큰 : 유저ID } 쌍을 저장한다.
        await _redisDb.SetAsync(request.AuthToken, userData.UserId, TimeSpan.FromDays(7));

        // 최종 로그인 시각을 변경한다.
        await _gameDb.UpdateLastLoginAt(userData.UserId);

        // 로드한 유저 데이터를 반환한다.
        return new LoginResponse() { Result = ErrorCode.None, UserData = userData };
    }

    [HttpPost("register")]
    public async Task<RegisterResponse> Register(RegisterRequest request)
    {
        // 유저가 플랫폼 서버에서 받아온 인증ID와 인증토큰을 체크한다.
        // 인증 실패
        if (!AuthCheck(request.AuthID, request.AuthToken))
        {
            return new RegisterResponse() { Result = ErrorCode.AuthCheckFail };
        }

        // GameDB에 해당 인증ID가 이미 존재하는지 확인한다.
        if (await _gameDb.CheckAuthIdExists(request.AuthID))
        {
            // 이미 존재하는 유저
            return new RegisterResponse() { Result = ErrorCode.UserAlreadyExists };
        }

        // GameDB에 User의 Default 데이터를 생성한다.
        UserData userData = null;
        var errorCode = await _gameDb.CreateDefaultData(request.AuthID, request.Nickname);
        if (errorCode != ErrorCode.None)
        {
            return new RegisterResponse() { Result = errorCode };
        }
        userData = await _gameDb.GetDefaultDataByAuthId(request.AuthID);

        // Redis에 { 인증ID : 인증 토큰 } 쌍으로 저장해 로그인 처리까지 한다.
        await _redisDb.SetAsync(request.AuthID, request.AuthToken);

        // 생성한 유저 데이터를 반환한다.
        return new RegisterResponse() { Result = ErrorCode.None, UserData = userData };
    }

    [HttpPost("logout")]
    public async Task<LogoutResponse> Logout(LogoutRequest request)
    {
        // Redis에서 { 인증ID : 인증 토큰 } 쌍을 삭제한다.
        await _redisDb.DeleteAsync(request.AuthID);

        return new LogoutResponse() { Result = ErrorCode.None };
    }

    [HttpPut("nickname")]
    public async Task<ChangeNicknameResponse> ChangeNickname(ChangeNicknameRequest request)
    {
        // 유저의 닉네임을 변경한다.
        if (!(await _gameDb.TryChangeNickname(request.AuthID, request.Nickname)))
        {
            // 중복 닉네임
            return new ChangeNicknameResponse() { Result = ErrorCode.DuplicateNickname };
        }

        // 변경 성공
        return new ChangeNicknameResponse() { Result = ErrorCode.None };
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
}