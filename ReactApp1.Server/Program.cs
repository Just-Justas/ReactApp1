using Microsoft.AspNetCore.Cors.Infrastructure;
using ReactApp1.Server.Services;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
//!builder.Services.AddScoped(provider => new AppDbContext(connectionString));
//!builder.Services.AddScoped<DormService>();
builder.Services.AddScoped(provider => new DormService(connectionString));
builder.Services.AddScoped(provider => new ResidentService(connectionString));
builder.Services.AddScoped(provider => new PostService(connectionString));
/*
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
*/


//builder.Services.AddDbContext<>(forumDbContext)


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	/*var services = scope.ServiceProvider;
	var context = services.GetRequiredService<AppDbContext>();
	//context.Database.Migrate();
	var dormService = services.GetRequiredService<DormService>();
	dormService.PrintDorms();*/
	var dormService = scope.ServiceProvider.GetRequiredService<DormService>();
	var residentService = scope.ServiceProvider.GetRequiredService<ResidentService>();
	var postService = scope.ServiceProvider.GetRequiredService<PostService>();
	dormService.PrintDorms();
}

/*
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");
*/
//




var dorms = app.MapGroup("/api");
dorms.MapGet("/dorms", (DormService dormService) => {
	var dormsList = dormService.GetDorms();
	if (dormsList.Count == 0)
	{
		return Results.NoContent();//204
	}
	else
		return Results.Ok(dormsList);//200
});
dorms.MapGet("/dorms/{dormId}", (int dormId, DormService dormService) => {
	var dorm = dormService.GetDormById(dormId);
	return dorm != null ? Results.Ok(dorm) : Results.NotFound();//200 or 404
});
dorms.MapPost("/dorms", (CreateDormDto dto, DormService dormService) => {
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
dorms.MapPut("/dorms/{dormId}", (UpdateDormDto dto, DormService dormService, int dormId) =>
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
dorms.MapDelete("/dorms/{dormId}", (int dormId, DormService dormService, ResidentService residentService) =>
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
dorms.MapPost("/dorms/{dormId}/residents", (CreateResidentDto dto, int dormId, ResidentService residentService, DormService dormService) => {
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
dorms.MapPut("/dorms/{dormId}/residents/{residentsId}", (UpdateResidentDto dto, ResidentService residentService, int dormId, int residentsId) =>
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
dorms.MapDelete("/dorms/{dormId}/residents/{residentsId}", (int dormId, int residentsId, ResidentService residentService) =>
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
dorms.MapPost("/dorms/{dormId}/residents/{residentsId}/posts", (CreatePostDto dto, int dormId, int residentsId, PostService postService, DormService dormService, ResidentService residentService) => {
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
dorms.MapPut("/dorms/{dormId}/residents/{residentsId}/posts/{postId}", (UpdatePostDto dto, PostService postService, int dormId, int residentsId, int postId) =>
{
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
dorms.MapDelete("/dorms/{dormId}/residents/{residentsId}/posts/{postId}", (int dormId, int residentsId, int postId, PostService postService) =>
{
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
//
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