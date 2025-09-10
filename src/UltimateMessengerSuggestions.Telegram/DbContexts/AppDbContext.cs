using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.Telegram.Models.Db;

namespace UltimateMessengerSuggestions.Telegram.DbContexts;

/// <inheritdoc/>
internal class AppDbContext : DbContext, IAppDbContext
{
	/// <inheritdoc/>
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{

	}

	/// <inheritdoc/>
	public DbSet<User> Users => Set<User>();

	/// <inheritdoc/>
	public DbSet<MessengerAccount> MessengerAccounts => Set<MessengerAccount>();

	/// <inheritdoc/>
	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await base.SaveChangesAsync(cancellationToken);
	}

	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}

	/// <inheritdoc/>
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSnakeCaseNamingConvention();
		base.OnConfiguring(optionsBuilder);
	}
}
