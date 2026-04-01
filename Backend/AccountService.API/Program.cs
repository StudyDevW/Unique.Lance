using AccountService.Application.Features.Auth.Commands;
using AccountService.Application.Interfaces;
using AccountService.Domain.Repositories;
using AccountService.Infrastructure.Persistence;
using AccountService.Infrastructure.Persistence.Repositories;
using AccountService.Infrastructure.Services;
using DotNetEnv;
using DotNetEnv.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Middleware.Shared.Interfaces;
using Middleware.Shared.Services;
using System.Security.Cryptography;

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

            builder.Services.AddSwaggerGen(o =>
            {
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Authorize accessToken",
                });

                o.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "AccountService.API",
                    Description = "Unique.Lance, Practice"
                });
            });

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = "Asymmetric";
                o.DefaultChallengeScheme = "Asymmetric";
                o.DefaultScheme = "Asymmetric";

            }).AddJwtBearer("Asymmetric", o =>
            {
                var rsa = RSA.Create();

                rsa.ImportFromPem(builder.Configuration["RSA_PUBLIC_KEY"]);

                o.IncludeErrorDetails = true;
                o.RequireHttpsMetadata = false;
                o.SaveToken = false;

                TokenValidationParameters tk_valid = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    ValidateLifetime = false,
                    RequireExpirationTime = false
                };

                o.TokenValidationParameters = tk_valid;
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddControllers();

            builder.Services.AddSingleton<IDatabaseAutoMigrations, DatabaseAutoMigrations>();

            builder.Services.AddScoped<IAccountRepository, AccountRepository>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


            builder.Services.AddScoped<ICacheService, CacheService>();

            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblyContaining<TelegramAuthCommand>();
                cfg.RegisterServicesFromAssemblyContaining<SessionLogOutCommand>();
                cfg.RegisterServicesFromAssemblyContaining<RefreshTokensCommand>();
            });

            var app = builder.Build();

            using (var serviceScope = app.Services.CreateScope())
            {
                var migrations = serviceScope.ServiceProvider.GetService<IDatabaseAutoMigrations>();

                if (migrations != null) await migrations.EnsureDatabaseInitializedAsync();
            }

            app.UseRouting();

            app.UseForwardedHeaders();

            app.UseAuthorization();

            app.UseAuthentication();

            app.MapControllers();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AccountService.API");
                c.RoutePrefix = "ui-swagger";
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/ui-swagger/");
                }
                else
                {
                    await next();
                }
            });


            await app.RunAsync();
        }
    }
}