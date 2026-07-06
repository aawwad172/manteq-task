
using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;

using Microsoft.EntityFrameworkCore;

namespace ManteqTask.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
    public async Task<User?> GetUserByEmailAsync(string email)
        => await _dbSet.IgnoreQueryFilters()
                .FirstOrDefaultAsync(user => user.Email == email);

    public async Task<User?> GetUserByUsernameAsync(string username)
        => await _dbSet.FirstOrDefaultAsync(user => user.Username == username);
}
