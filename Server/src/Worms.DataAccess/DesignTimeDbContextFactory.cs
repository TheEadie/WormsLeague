using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Worms.DataAccess
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext> 
    { 
        public DataContext CreateDbContext(string[] args) 
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@Directory.GetCurrentDirectory() + "/../Worms.Gateway/appsettings.json").Build(); 
            var builder = new DbContextOptionsBuilder<DataContext>(); 
            var connectionString = configuration.GetConnectionString("DataContext"); 
            builder.UseSqlServer(connectionString); 
            return new DataContext(builder.Options); 
        } 
    }
}