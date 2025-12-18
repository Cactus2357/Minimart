namespace MinimartApi.Dtos {
    public class AccessTokenResponse {
        public string TokenType { get; } = "Bearer";
        public required string AccessToken { get; init; }

    }
}
