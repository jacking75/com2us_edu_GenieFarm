public interface IAuthCheckService
{
    /// <summary>
    /// PlayerID와 Nickname으로 기본 게임 데이터를 생성합니다.
    /// </summary>
    public Task<ErrorCode> CreateDefaultGameData(string playerId, string nickname);

    /// <summary>
    /// Hive PlayerID로 기본 게임 데이터를 로드합니다.
    /// </summary>
    public Task<Tuple<ErrorCode, DefaultDataDTO?>> GetDefaultGameData(string playerId);

    /// <summary>
    /// PlayerID로 된 계정이 DB에 존재하는지 확인하고, <br/>
    /// 존재한다면 ErrorCode.None을 리턴합니다.
    /// </summary>
    public Task<ErrorCode> CheckPlayerExists(string playerId);

    /// <summary>
    /// 최종 로그인 시각을 갱신합니다.<br/>
    /// 로그인 시에 사용됩니다.
    /// </summary>
    public Task<ErrorCode> UpdateLastLoginAt(Int64 userId);

    /// <summary>
    /// Redis에 토큰을 저장합니다.
    /// </summary>
    public Task<ErrorCode> SetTokenOnRedis(Int64 userId, string token);

    /// <summary>
    /// Redis에 있는 토큰을 삭제합니다.
    /// </summary>
    public Task<ErrorCode> DeleteTokenOnRedis(Int64 userId);

    /// <summary>
    /// Hive 서버에 인증을 요청합니다.
    /// </summary>
    public Task<bool> AuthCheckToHive(string playerId, string token);
}
