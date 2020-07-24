using Microsoft.EntityFrameworkCore;
using Worms.DataAccess.Models;

namespace Worms.DataAccess
{
    public class DataContext: DbContext
    {
        public DataContext (DbContextOptions<DataContext> options)
            : base(options)
        {
        }
        
        public DbSet<GameEf> Games { get; set; }
    }
}