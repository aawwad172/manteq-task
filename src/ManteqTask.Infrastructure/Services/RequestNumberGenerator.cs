using ManteqTask.Domain.Interfaces.Infrastructure;
using ManteqTask.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ManteqTask.Infrastructure.Services;

public class RequestNumberGenerator(ApplicationDbContext dbContext) : IRequestNumberGenerator
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    // nextval is atomic and concurrency-safe on its own — no transaction/locking needed.
    // Aliased AS "Value" so EF's scalar SqlQueryRaw<long> projection binds the column.
    public async Task<string> NextAsync()
    {
        long next = await _dbContext.Database
            .SqlQueryRaw<long>("SELECT nextval('request_number_seq') AS \"Value\"")
            .SingleAsync();

        return $"PA-{DateTime.UtcNow.Year}-{next:D5}";
    }
}
