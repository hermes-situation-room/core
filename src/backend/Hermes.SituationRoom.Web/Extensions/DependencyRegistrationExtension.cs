namespace Hermes.SituationRoom.Api.Extensions;

using Api.Configurations;
using Controllers.Base;
using Data.Context;
using Data.Interface;
using Data.Migrations;
using Data.Repositories;
using Domain.Services;
using Profiles;

public static class DependencyRegistrationExtension
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Web
        services.AddSwagger();

        // Database
        services.AddTransient<IHermessituationRoomContext, HermessituationRoomContext>();
        
        // Data
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IActivistRepository, ActivistRepository>();
        services.AddTransient<IJournalistRepository, JournalistRepository>();
        
        // Domain
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IActivistService, ActivistService>();
        services.AddTransient<IJournalistService, JournalistService>();
        
        var connectionString = configuration.GetConnectionString("SituationRoomDb");
        services.AddDatabaseMigrations(connectionString,
            Path.Combine(AppContext.BaseDirectory, "Migrations"));

        // REST Infrastructure
        services.AddScoped<IControllerInfrastructure, ControllerInfrastructure>();

        // Logging
        services.AddLogging();

        // Mapping
        services.AddAutoMapper(typeof(SituationRoomProfile));

        return services;
    }
}