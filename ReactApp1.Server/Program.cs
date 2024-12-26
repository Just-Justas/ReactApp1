using Microsoft.AspNetCore.Cors.Infrastructure;
using ReactApp1.Server.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ReactApp1.Server.Auth.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using ReactApp1.Server.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;//???

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped(provider => new DormService(connectionString));
builder.Services.AddScoped(provider => new ResidentService(connectionString));
builder.Services.AddScoped(provider => new PostService(connectionString));
builder.Services.AddTransient<JwtTokenService>();
builder.Services.AddScoped<AuthDbSeeder>();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseMySql(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		new MySqlServerVersion(new Version(8, 0, 33)) // Adjust based on your MySQL version
	));
builder.Services.AddIdentity<ForumRestUser, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.TokenValidationParameters.ValidAudience=builder.Configuration["Jwt:ValidAudience"];
	options.TokenValidationParameters.ValidIssuer=builder.Configuration["Jwt:ValidIssuer"];
	options.TokenValidationParameters.IssuerSigningKey= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]));
	//options.TokenValidationParameters.RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
});
builder.Services.AddAuthorization();
var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
	var dormService = scope.ServiceProvider.GetRequiredService<DormService>();
	var residentService = scope.ServiceProvider.GetRequiredService<ResidentService>();
	var postService = scope.ServiceProvider.GetRequiredService<PostService>();
	var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthDbSeeder>();
	await dbSeeder.SeedAsync();
	dormService.PrintDorms();
}





var dorms = app.MapGroup("/api");

dorms.MapGet("/role", (ResidentService residentService, HttpContext httpContext) => {
	var userRole = httpContext.User?.FindFirstValue(ClaimTypes.Role);
	if (string.IsNullOrEmpty(userRole))
	{
		return Results.Ok(0); // Not logged in
	}
	if (httpContext.User.IsInRole(ForumRoles.Admin))
	{
		return Results.Ok(2); // Admin
	}
	return Results.Ok(1); // User
});

dorms.MapGet("/dorms", (DormService dormService) => {
	var dormsList = dormService.GetDorms();
	if (dormsList.Count == 0)
	{
		return Results.NoContent();//204
	}
	else
		return Results.Ok(dormsList);//200
});

dorms.MapGet("/dorms/{dormId}/posts", (int dormId, DormService dormService, PostService postService) => {
	var dorm = dormService.GetDormById(dormId);
	if(dorm == null)
	{
		return Results.NotFound($"Dorm with ID {dormId} not found.");//404
	}
	var posts = postService.GetPosts(dormId);
	return Results.Ok(posts);//200
});

dorms.MapPost("/dorms", [Authorize(Roles = ForumRoles.Admin)] (CreateDormDto dto, DormService dormService) => {
	if (string.IsNullOrWhiteSpace(dto.Name))
	{
		return Results.BadRequest("Dorm name is required.");//400
	}
	if (dto.Name.Length < 4 || dto.Name.Length > 100) 
	{
		return Results.UnprocessableEntity("Dorm Name cannot be shorther than 4 characters or longer than 100");//422
	}
	var newDorm = dormService.CreateDorm(dto);
	return Results.Created($"/dorms/{newDorm.Id}", newDorm);//201
});
dorms.MapPut("/dorms/{dormId}", [Authorize(Roles = ForumRoles.Admin)] (UpdateDormDto dto, DormService dormService, int dormId) =>
{
	if (string.IsNullOrWhiteSpace(dto.Name))
	{
		return Results.BadRequest("Dorm name is required.");//400
	}
	var existingDorm = dormService.GetDormById(dormId);
	if (dto.Name.Length < 4 || dto.Name.Length > 100)
	{
		return Results.UnprocessableEntity("Dorm Name cannot be shorther than 4 characters or longer than 100");//422
	}
	if (existingDorm == null)
	{
		return Results.NotFound($"Dorm with ID {dormId} not found.");//404
	}
	dormService.UpdateDorm(dto, dormId);
	return Results.Ok();//200
});
dorms.MapDelete("/dorms/{dormId}", [Authorize(Roles = ForumRoles.Admin)] (int dormId, DormService dormService, ResidentService residentService) =>
{
	if (residentService.GetResidents(dormId).Count > 0)
	{
		return Results.Conflict("Cannot delete dorm with existing residents.");//409
	}
	else if (dormService.GetDormById(dormId) == null)
	{
		return Results.NotFound($"Dorm with ID: {dormId} cannot be found.");//404
	}
	else
	{
		dormService.DeleteDorm(dormId);
		return Results.NoContent();//204
	}
});




dorms.MapGet("/dorms/{dormId}/residents", (int dormId, ResidentService residentService, DormService dormService) => {
	if (dormService.GetDormById(dormId) == null)
	{
		return Results.NotFound($"Dorm with id {dormId} does not exist");//404
	}
	var residentsList = residentService.GetResidents(dormId);
	if (residentsList.Count == 0)
	{
		return Results.NoContent();//204
	}
	return Results.Ok(residentsList);//200
});
dorms.MapGet("/dorms/{dormId}/residents/{residentsId}", (int dormId, int residentsId, ResidentService residentService) => {
	var resident = residentService.GetResidentsById(dormId, residentsId);
	return resident != null ? Results.Ok(resident) : Results.NotFound();//200 or 404
});
dorms.MapPost("/dorms/{dormId}/residents", [Authorize(Roles = ForumRoles.Admin)] (CreateResidentDto dto, int dormId, ResidentService residentService, DormService dormService) => {
	if (dormService.GetDormById(dormId) == null)
	{
		return Results.NotFound($"Dorm with id {dormId} does not exist");//404
	}
	if (string.IsNullOrWhiteSpace(dto.Firstname) || string.IsNullOrWhiteSpace(dto.Lastname) || dto.RoomNumber <= 0) 
	{
		return Results.BadRequest("BadRequest");//400
	}
	if (dto.RoomNumber > 2000)
	{
		return Results.UnprocessableEntity("Max room number is 2000");//422
	}
	var newDorm= residentService.CreateResidents(dto, dormId);
	return Results.Created($"/dorms/{dormId}/residents/{newDorm.Id}", newDorm);//201
});
dorms.MapPut("/dorms/{dormId}/residents/{residentsId}", [Authorize(Roles = ForumRoles.Admin)] (UpdateResidentDto dto, ResidentService residentService, int dormId, int residentsId) =>
{
	if (string.IsNullOrWhiteSpace(dto.Firstname) || string.IsNullOrWhiteSpace(dto.Lastname) || dto.RoomNumber <= 0)
	{
		return Results.BadRequest("BadRequest");//400
	}
	var existingDorm = residentService.GetResidentsById(dormId, residentsId);
	if (existingDorm == null)
	{
		return Results.NotFound($"Resident with ID {residentsId} not found in dorm with ID {dormId}.");//404
	}
	if (dto.RoomNumber > 2000)
	{
		return Results.UnprocessableEntity("Max room number is 2000");//422
	}
	residentService.UpdateResidents(dto, dormId, residentsId);
	return Results.Ok();//200
});
dorms.MapDelete("/dorms/{dormId}/residents/{residentsId}", [Authorize(Roles = ForumRoles.Admin)] (int dormId, int residentsId, ResidentService residentService) =>
{
	if (residentService.GetResidentsById(dormId, residentsId) == null)
	{
		return Results.NotFound($"Resident with ID {residentsId} not found in dorm with ID {dormId}.");//404
	}
	else
	{
		residentService.DeleteResidents(dormId, residentsId);
		return Results.Ok();//200
	}
});


dorms.MapGet("/dorms/{dormId}/residents/{residentsId}/posts", (int dormId, int residentsId, PostService postService, DormService dormService, ResidentService residentService) => {
	if (dormService.GetDormById(dormId) == null)
	{
		return Results.NotFound($"Dorm with id {dormId} does not exist");//404
	}
	if (residentService.GetResidentsById(dormId,residentsId) == null)
	{
		return Results.NotFound($"resident with id {residentsId} does not exist in dorm {dormId}");//404
	}
	var residentsList = postService.GetPosts(dormId, residentsId);
	if (residentsList.Count == 0)
	{
		return Results.NoContent();//204
	}
	return Results.Ok(residentsList);//200
});
dorms.MapGet("/dorms/{dormId}/residents/{residentsId}/posts/{postId}", (int dormId, int residentsId, int postId, PostService postService) => {
	var resident = postService.GetPostById(dormId, residentsId, postId);
	return resident != null ? Results.Ok(resident) : Results.NotFound();//200 or 204
});
dorms.MapPost("/dorms/{dormId}/residents/{residentsId}/posts", [Authorize(Roles = ForumRoles.ForumUser)] (CreatePostDto dto, int dormId, int residentsId, PostService postService, DormService dormService, ResidentService residentService, HttpContext httpContext) => {
	if (httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != residentService.FindUserIdOfResident(residentsId))
	{
		return Results.Unauthorized();
	}
	if (dormService.GetDormById(dormId) == null)
	{
		return Results.NotFound($"Dorm with id {dormId} does not exist");//404
	}
	if (residentService.GetResidentsById(dormId, residentsId) == null)
	{
		return Results.NotFound($"resident with id {residentsId} does not exist in dorm {dormId}");//404
	}
	if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Text))
	{
		return Results.BadRequest("BadRequest");//400
	}
	if (dto.Title.Length < 3)
	{
		return Results.UnprocessableEntity("min title length is 3");//422
	}

	var newDorm = postService.CreatePost(dto, dormId, residentsId);
	return Results.Created($"/dorms/{dormId}/residents/{residentsId}/posts/{newDorm.Id}", newDorm);//201
});
dorms.MapPut("/dorms/{dormId}/residents/{residentsId}/posts/{postId}", [Authorize(Roles = ForumRoles.ForumUser)] (UpdatePostDto dto, PostService postService, ResidentService residentService, int dormId, int residentsId, int postId, HttpContext httpContext) =>
{
	if (httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != residentService.FindUserIdOfResident(residentsId))
	{
		return Results.Unauthorized();
	}
	if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Text))
	{
		return Results.BadRequest("BadRequest");//400
	}
	var existingDorm = postService.GetPostById(dormId, residentsId, postId);
	if (existingDorm == null)
	{
		return Results.NotFound($"Post with ID {postId} of resident with ID {residentsId} not found in dorm with ID {dormId} not found.");//404
	}
	if (dto.Title.Length < 3)
	{
		return Results.UnprocessableEntity("min title length is 3");//422
	}
	postService.UpdatePost(dto, dormId, residentsId, postId);
	return Results.Ok();//200
});
dorms.MapDelete("/dorms/{dormId}/residents/{residentsId}/posts/{postId}", [Authorize(Roles = ForumRoles.ForumUser)] (int dormId, int residentsId, int postId, PostService postService, HttpContext httpContext, ResidentService residentService) =>
{
	if ((httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != residentService.FindUserIdOfResident(residentsId)) && !httpContext.User.IsInRole(ForumRoles.Admin))
	{
		return Results.Unauthorized();
	}
	if (postService.GetPostById(dormId, residentsId, postId) == null)
	{
		return Results.NotFound($"Post with ID {postId} of resident with ID {residentsId} not found in dorm with ID {dormId} not found.");//404
	}
	else
	{
		postService.DeletePost(dormId, residentsId, postId);
		return Results.Ok();//200
	}
});

dorms.MapDelete("/dorms/{dormId}/posts/{postId}", [Authorize(Roles = ForumRoles.ForumUser)] (int postId, PostService postService, HttpContext httpContext) =>
{
	if (!httpContext.User.IsInRole(ForumRoles.Admin))
	{
		return Results.Unauthorized();
	}
		postService.DeletePost(postId);
		return Results.Ok();//200
});
//

app.AddAuthApi();

app.UseAuthentication();
app.UseAuthorization();

//using var scope = app.Services.CreateScope();
//var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthDbSeeder>();


app.Run();
//
//
//

public record DormDto(int Id, string Name);
public record CreateDormDto(string Name);
public record UpdateDormDto(string Name);

public record ResidentDto(int Id, string Firstname, string Lastname, int RoomNumber, int DormId);
public record CreateResidentDto(string Firstname, string Lastname, int RoomNumber);
public record UpdateResidentDto(string Firstname, string Lastname, int RoomNumber);

public record PostDto(string Title, string Text, int PosterId, int DormId, int Id, DateTime posted_timestamp);
public record CreatePostDto(string Title, string Text);
public record UpdatePostDto(string Title, string Text);



