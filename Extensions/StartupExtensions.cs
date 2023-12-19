using System.Text;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using MessengerServer.Configuration;
using MessengerServer.Contexts;
using MessengerServer.Entities;
using MessengerServer.Services;
using MessengerServer.Services.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MessengerServer.Extensions;

public static class StartupExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options => options.AllowEmptyInputInBodyModelBinding = true)
            .AddJsonOptions(
                options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

        builder.Services.AddDbContext<SqlServerDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetRequiredConnectionString("Database"));
        });

        #region Authorization

        builder.Services.AddIdentity<User, IdentityRole>(
                options =>
                {
                    // Password settings
                    if (!builder.Environment.IsProduction())
                    {
                        options.Password.RequiredLength = 3;
                        options.Password.RequireDigit = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireLowercase = false;
                    }
                    else
                    {
                        options.Password.RequireDigit = true;
                        options.Password.RequiredLength = 8;
                        options.Password.RequireNonAlphanumeric = true;
                        options.Password.RequireUppercase = true;
                        options.Password.RequireLowercase = true;
                    }

                    // Lockout settings
                    if (!builder.Environment.IsProduction())
                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                    else
                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(365);

                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;
                })
            .AddEntityFrameworkStores<SqlServerDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"] ??
                                  throw new InvalidOperationException("Jwt:Issuer is not set"),
                    ValidAudience = builder.Configuration["Jwt:Audience"] ??
                                    throw new InvalidOperationException("Jwt:Audience is not set"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"] ??
                        throw new InvalidOperationException("Jwt:Key is not set")))
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Authenticated", new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());
        });

        #endregion

        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        #region Azure

        builder.Services.AddAzureClients(clients =>
        {
            clients.AddBlobServiceClient(builder.Configuration.GetConnectionString("AzureStorage"));
        });

        builder.Services.Configure<Configuration.Azure>(
            builder.Configuration.GetRequiredSection("Azure"));

        builder.Services.Configure<Jwt>(
            builder.Configuration.GetRequiredSection("Jwt"));

        builder.Services.AddTransient<IFileNameGenerator, UniqueFileNameGenerator>();
        builder.Services.AddSingleton<IFormFileManager, AzureFormFileManager>(services =>
        {
            var azureOptions = services.GetRequiredService<IOptions<Configuration.Azure>>();
            azureOptions.Value.Validate();
            var containerName = azureOptions.Value.AvatarImageContainerName!;

            var blobServiceClient = services.GetRequiredService<BlobServiceClient>();
            var fileNameGenerator = services.GetRequiredService<IFileNameGenerator>();
            return new AzureFormFileManager(blobServiceClient, containerName, fileNameGenerator);
        });

        #endregion

        #region Custom services

        //TODO

        #endregion

        return builder;
    }

    public static WebApplication Configure(this WebApplication app)
    {
        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("v1/swagger.json", "Messenger API");
            });
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            "default",
            "api/{controller=Home}/{action=Index}/{id?}");


        app.MapFallbackToFile("index.html");

        return app;
    }
}