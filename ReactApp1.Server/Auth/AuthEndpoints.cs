using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using ReactApp1.Server.Auth.Model;
using ReactApp1.Server.Services;
using System.Security.Claims;

namespace ReactApp1.Server.Auth
{
	public static class AuthEndpoints
	{
		public static void AddAuthApi(this WebApplication app)
		{
			//register
			app.MapPost("api/register", async (UserManager<ForumRestUser> userManager, RegisterUserDto registerUserDto, ResidentService residentService) =>
			{
				var user = await userManager.FindByNameAsync(registerUserDto.Username);
				if (user != null)
				{
					return Results.UnprocessableEntity("username taken");
				}
				var newUser= new ForumRestUser
				{
					UserName = registerUserDto.Username
				};
				var createUserResult = await userManager.CreateAsync(newUser, registerUserDto.Password);
				if(!createUserResult.Succeeded)
				{
					return Results.UnprocessableEntity();
				}

				await userManager.AddToRoleAsync(newUser,ForumRoles.ForumUser);
				residentService.SetUserIdOfResident(registerUserDto.ResidentId, newUser.Id);
				return Results.Created("api/login", new UserDto(newUser.Id, newUser.UserName));

			});
			//login
			app.MapPost("api/login", async (UserManager<ForumRestUser> userManager,JwtTokenService jwtTokenService, LoginDto loginDto) =>
			{
				var user = await userManager.FindByNameAsync(loginDto.Username);
				if (user == null)
				{
					return Results.UnprocessableEntity("username not found");
				}
				var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
				if (!isPasswordValid)
				{
					return Results.UnprocessableEntity("password not correct");
				}
				user.forceRelogin = false;
				await userManager.UpdateAsync(user);
				var roles = await userManager.GetRolesAsync(user);
				var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
				var refreshToken = jwtTokenService.CreateRefreshToken(user.Id);


				return Results.Ok(new SucessfullLoginDto(accessToken, refreshToken));

			});
			//refresh
			app.MapPost("api/accessToken", 
				async (UserManager<ForumRestUser> userManager, JwtTokenService jwtTokenService, RefreshAccessTokenDto refreshAccessTokenDto) =>
				{
					if(!jwtTokenService.TryParseRefreshToken(refreshAccessTokenDto.RefreshToken, out var claims))
					{
						return Results.UnprocessableEntity("Try parse fail");
					}
					var userId = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);
					var user = await userManager.FindByIdAsync(userId);
				if (user == null)
					{
						return Results.UnprocessableEntity("invalid token");
					}

					if (user.forceRelogin)
					{
						return Results.UnprocessableEntity("Force relogin");
					}

					var roles = await userManager.GetRolesAsync(user);
					var accessToken = jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
					var refreshToken = jwtTokenService.CreateRefreshToken(user.Id);

					return Results.Ok(new SucessfullLoginDto(accessToken, refreshToken));
				});
		}
	}
	/*public class RegisterUserDto
	{

	}*/
	public record RefreshAccessTokenDto(string RefreshToken);
	public record SucessfullLoginDto(string AccessToken, string RefreshToken);
	public record RegisterUserDto(string Username, string Password, int ResidentId);
	public record LoginDto(string Username, string Password);
	public record UserDto(string UserId, string Username);
}
