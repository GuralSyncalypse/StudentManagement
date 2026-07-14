using System.Security.Cryptography;

namespace LuongChiHai_QLSV.Server.Helpers
{
    public class TokenService
    {
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
