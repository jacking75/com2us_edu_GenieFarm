public interface IAuthCheckService
{
    /// <summary>
    /// PlayerID와 Nickname으로 기본 게임 데이터를 생성한다. <br/>
    /// 기본 유저 데이터 생성, 출석 데이터 생성, 농장 기본 데이터 생성, 기본 아이템 Insert를 수행한다.
    /// </summary>
    /// <param name="playerId">Hive PlayerID</param>
    /// <param name="nickname">닉네임</param>
    /// <returns></returns>
    public Task<ErrorCode> CreateDefaultGameData(string playerId, string nickname);

    /// <summary>
    /// Hive PlayerID로 기본 게임 데이터를 로드한다.
    /// </summary>
    /// <param name="playerId">Hive PlayerID</param>
    /// <returns></returns>
    public Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultGameData(string playerId);

    /// <summary>
    /// PlayerID로 된 계정이 DB에 존재하는지 확인하고, 존재한다면 ErrorCode.None을 리턴한다.
    /// </summary>
    /// <param name="playerId">조회하고자 하는 Hive PlayerID</param>
    /// <returns></returns>
    public Task<ErrorCode> CheckPlayerExists(string playerId);

    /// <summary>
    /// 최종 로그인 시각을 갱신한다.<br/>
    /// 로그인 시에 사용된다.
    /// </summary>
    /// <param name="userId">로그인한 유저의 ID</param>
    /// <returns></returns>
    public Task<ErrorCode> UpdateLastLoginAt(Int64 userId);

    /// <summary>
    /// Redis에 토큰을 저장한다.
    /// </summary>
    /// <param name="userId">토큰을 저장할 유저ID</param>
    /// <param name="token">세션 토큰</param>
    /// <returns></returns>
    public Task<ErrorCode> SetTokenOnRedis(Int64 userId, string token);
}
