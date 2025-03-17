using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Utilities.Security
{
    public static class JwtHelper
    {
        private const string SecretKey = "THIS_IS_A_SECURE_SECRET_KEY_THAT_IS_AT_LEAST_32_CHARACTERS_LONG";

        // Accepts only the necessary primitives instead of a full User object.
        public static string GenerateToken(int userId, string email, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, role)
    };

            var token = new JwtSecurityToken(
       issuer: "https://localhost:7233", // Ensure this matches the ValidIssuer
       audience: "https://localhost:7233", // Ensure this matches the ValidAudience
       claims: claims,
       expires: DateTime.UtcNow.AddDays(7),
       signingCredentials: creds
   );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
