using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;
namespace ReactApp1.Server.Auth
{
	public class JwtTokenService
	{
		private readonly SymmetricSecurityKey _authSigninKey;
		private readonly string _issuer;
		private readonly string _audience;
		public JwtTokenService(IConfiguration configuration)
		{
			_authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]));
			_issuer = configuration["Jwt:ValidIssuer"];
			_audience = configuration["Jwt:ValidAudience"];
		}
		public string CreateAccessToken(string username, string userId, IEnumerable<string> roles)
		{
			var authClaims = new List<Claim>()
			{
				new (ClaimTypes.Name, username),
				new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new (JwtRegisteredClaimNames.Sub, userId)
			};
			authClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
			var token = new JwtSecurityToken
			(
				issuer: _issuer,
				audience: _audience,
				expires: DateTime.UtcNow.AddMinutes(60),
				claims: authClaims,
				signingCredentials: new SigningCredentials(_authSigninKey, SecurityAlgorithms.HmacSha256)
			);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
		public string CreateRefreshToken(string userId)
		{
			var authClaims = new List<Claim>()
			{
				new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new (JwtRegisteredClaimNames.Sub, userId)
			};
			var token = new JwtSecurityToken
			(
				issuer: _issuer,
				audience: _audience,
				expires: DateTime.UtcNow.AddHours(5),
				claims: authClaims,
				signingCredentials: new SigningCredentials(_authSigninKey, SecurityAlgorithms.HmacSha256)
			);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
		public bool TryParseRefreshToken(String refreshToken, out ClaimsPrincipal? claims)
		{
			claims = null;
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var validationParameters = new TokenValidationParameters
				{
					ValidIssuer = _issuer,
					ValidAudience = _audience,
					IssuerSigningKey = _authSigninKey,
					ValidateLifetime = true,
				};
				claims=tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
