using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Worms.DataAccess.Repositories;

namespace Worms.DataAccess
{
    public class DataAccessModule
    {
        private readonly IConfiguration _configuration;

        public DataAccessModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("DataContext")));

            services.AddScoped<IGamesRepo, GamesRepo>();
            services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        }
    }
}
