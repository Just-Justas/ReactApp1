using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Migrations;
using ReactApp1.Server.Entities;
public class AppDbContext : DbContext
{
	public AppDbContext(string connectionString) : base(connectionString)
	{
		Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Configuration>());
	}
	public AppDbContext() : base()
	{
		Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Configuration>());
	}
	public DbSet<Dorm> Dorms { get; set; }
}

internal sealed class Configuration : DbMigrationsConfiguration<AppDbContext>
{
	public Configuration()
	{
		AutomaticMigrationsEnabled = true;
	}
}
