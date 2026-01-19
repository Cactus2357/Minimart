namespace MinimartApi.Dtos.Authentication {
    public class AccessTokenResponse {
        public string TokenType { get; } = "Bearer";
        public required string AccessToken { get; init; }

    }
}
