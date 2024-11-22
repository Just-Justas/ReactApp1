using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Migrations;
using ReactApp1.Server.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ReactApp1.Server.Auth.Model;
public class AppDbContext : IdentityDbContext<ForumRestUser>
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	//public DbSet<Dorm> Dorms { get; set; }
	//public DbSet<Resident> Residents { get; set; }
	//public DbSet<Post> Posts { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		// Configure entity relationships or constraints if needed
	}

	/*
	internal sealed class Configuration : DbMigrationsConfiguration<AppDbContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = true;
		}
	}*/
}
