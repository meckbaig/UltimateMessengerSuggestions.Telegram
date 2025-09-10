using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.Telegram.Models.Db;

namespace UltimateMessengerSuggestions.Telegram.DbContexts;

internal interface IAppDbContext
{
	DbSet<User> Users { get; }
	DbSet<MessengerAccount> MessengerAccounts { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
