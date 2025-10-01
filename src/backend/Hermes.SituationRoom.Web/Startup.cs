namespace Hermes.SituationRoom.Api;

using Data.Context;
using Hermes.SituationRoom.Api.Configurations;
using Hermes.SituationRoom.Api.Extensions;
using Hermes.SituationRoom.Api.Middlewares;
using Microsoft.EntityFrameworkCore;
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
        services.AddDbContext<SituationContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddControllers();

        services.RegisterDependencies(Configuration);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                builder =>
                {
                    builder.WithOrigins("http://localhost:4300")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        services.AddSignalR();
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsProduction())
        {
            app.UseSwaggerAndSwaggerUI();
        }

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection()
            .UseRouting()
            .UseCors("AllowFrontend")
            .UseRequestLocalization()
            .UseMiddleware<ExceptionMiddleware>()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chatHub").RequireCors("AllowFrontend");
            });
    }
}
