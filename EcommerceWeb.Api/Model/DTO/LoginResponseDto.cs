namespace NZWalks.API.Models.DTO
{
    public class LoginResponseDto
    {
        public string JwtToken { get; set; }
    }
    public class TwoFactorVerifyDto
    {
        public string Code { get; set; }
    }

    public class TwoFactorLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string TwoFactorCode { get; set; }
    }
}
