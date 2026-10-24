using Lifeline.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Lifeline;

public class Database : DbContext
{
    internal DbSet<LifelineContext> Lifelines { set; get; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!PathHelper.IsDebug)
        {
            if (Directory.Exists("/data/")) Directory.CreateDirectory("/data/");
        }
        options.UseSqlite(
            PathHelper.IsDebug
            ? $"Data Source=Sqlite.db"
            : $"Data Source=/data/Sqlite.db"
        );
    }
}
