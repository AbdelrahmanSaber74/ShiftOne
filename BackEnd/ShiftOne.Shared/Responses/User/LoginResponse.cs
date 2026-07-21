namespace ShiftOne.Shared.Responses.User
{
    public class LoginResponse : RefreshTokenResponse
    {
    }
    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}


