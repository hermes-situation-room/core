namespace Hermes.SituationRoom.Api;

using Data.Context;
using Data.Interface;
using Domain.Hubs;
using Hermes.SituationRoom.Api.Configurations;
using Hermes.SituationRoom.Api.Extensions;
using Hermes.SituationRoom.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
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
                options.Cookie.HttpOnly = true; // Secure in production
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Works with both HTTP and HTTPS
                options.Cookie.SameSite = SameSiteMode.None; // Allow cross-site requests
                options.Events.OnRedirectToLogin = ctx =>
                {
                    // Return 401 instead of redirect
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    // Return 403 instead of redirect
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        services.AddControllers();

        services.RegisterDependencies(Configuration);

        services.AddSignalR();
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowCredentials",
                builder =>
                {
                    builder.AllowAnyOrigin()
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
