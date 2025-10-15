namespace Hermes.SituationRoom.Api;

using Configurations;
using Data.Context;
using Data.Interface;
using Domain.Hubs;
using Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Middlewares;
using Serilog;
using Microsoft.AspNetCore.SignalR;

public class Startup
{
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _env = env;
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        Configuration = builder.Build();
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("SituationRoomDb");
        services.AddDbContext<IHermessituationRoomContext, HermessituationRoomContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddHttpContextAccessor();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "SituationRoom.Auth";
                options.Cookie.HttpOnly = true;
                
                if (_env.IsDevelopment())
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                }
                else
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.None;
                }
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        services.AddControllers();

        services.RegisterDependencies(Configuration);

        services.AddSignalR();

        services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowCredentials", builder =>
            {
                builder
                    .WithOrigins(
                        "https://hermes-situation-room.release",
                        "https://hermes-situation-room.stage",
                        "http://localhost:13500/swagger",
                        "http://localhost:5005/swagger",
                        "http://localhost:5005",
                        "http://localhost:4300"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwaggerAndSwaggerUI();
        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection()
            .UseRouting()
            .UseCors("AllowCredentials")
            .UseRequestLocalization()
            .UseMiddleware<ExceptionMiddleware>()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chatHub");
            });
    }
}
