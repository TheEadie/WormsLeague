using System;
using System.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Worms.DataAccess
{
    internal class DatabaseMigrator : IDatabaseMigrator
    {
        private readonly DataContext _context;
        private readonly ILogger<DatabaseMigrator> _logger;

        public DatabaseMigrator(DataContext context, ILogger<DatabaseMigrator> logger)
        {
            _context = context;
            _logger = logger;
        }
        public void MigrateDatabase()
        {
            const int maxRetries = 3;
            var waitBetweenRetries = TimeSpan.FromSeconds(10);

            for (var i = 1; i <= maxRetries; i++)
            {
                try
                {
                    _logger.LogInformation($"Migrating database. Attempt {i} of {maxRetries}");
                    _context.Database.Migrate();
                    _logger.LogInformation("Database migrated");
                    break;
                }
                catch (SqlException exception)
                {
                    _logger.LogWarning($"Database migration failed: {exception.Message}");
                    if (i == maxRetries)
                        throw;

                    _logger.LogInformation($"Retrying database migration in {waitBetweenRetries}");
                    Thread.Sleep(waitBetweenRetries);
                }
            }
        }
    }
}
