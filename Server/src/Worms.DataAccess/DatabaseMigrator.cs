using Microsoft.EntityFrameworkCore;

namespace Worms.DataAccess
{
    internal class DatabaseMigrator : IDatabaseMigrator
    {
        private readonly DataContext _context;

        public DatabaseMigrator(DataContext context)
        {
            _context = context;
        }
        public void MigrateDatabase()
        {
            _context.Database.Migrate();
        }
    }
}