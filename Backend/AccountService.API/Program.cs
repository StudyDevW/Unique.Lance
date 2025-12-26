using AccountService.Application.Interfaces;
using AccountService.Infrastructure.Persistence;
using AccountService.Infrastructure.Services;
using DotNetEnv;
using DotNetEnv.Configuration;
using Microsoft.EntityFrameworkCore;

namespace AccountService.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());
   
            builder.Services.AddDbContext<AccountContext>(options =>
            {
                var connectString = builder.Configuration["ACCOUNT_DATABASE"];

                if (connectString != null)
                    options.UseNpgsql(connectString, npgopt =>
                    {
                        npgopt.MigrationsAssembly("AccountService.Infrastructure");
                        npgopt.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    });
            });

            builder.Services.AddControllers();

            builder.Services.AddSingleton<IDatabaseAutoMigrations, DatabaseAutoMigrations>();

            var app = builder.Build();

            using (var serviceScope = app.Services.CreateScope())
            {
                var migrations = serviceScope.ServiceProvider.GetService<IDatabaseAutoMigrations>();

                if (migrations != null) await migrations.EnsureDatabaseInitializedAsync();

            }

            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}