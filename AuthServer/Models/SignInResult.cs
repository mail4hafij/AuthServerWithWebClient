namespace AuthServer.Models
{
    public class SignInResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public DateTime? Expires { get; set; } = null;
        public string RefreshToken { get; set; } = string.Empty;
        public UserContract? UserContract { get; set; } = null;
        
        public SignInResult(bool success, string error, 
            string accessToken, string refreshToken,
            DateTime? expires = null, UserContract? userContract = null)
        {
            Success = success;
            Error = error;
            AccessToken = accessToken;
            Expires = expires;
            RefreshToken = refreshToken;
            UserContract = userContract;
        }
    }
}
