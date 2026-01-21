using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Riffle.Data;

public class MusicDbContextFactory : IDesignTimeDbContextFactory<MusicDbContext>
{
    public MusicDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MusicDbContext>();
        optionsBuilder.UseSqlite($"Data Source={MusicDbContext.DbPath}"); // same connection string

        return new MusicDbContext(optionsBuilder.Options);
    }
}