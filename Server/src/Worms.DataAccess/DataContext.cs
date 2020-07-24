using Microsoft.EntityFrameworkCore;
using Worms.DataAccess.Models;

namespace Worms.DataAccess
{
    internal class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<GameDb> Games { get; set; }
    }
}