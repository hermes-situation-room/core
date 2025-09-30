namespace Hermes.SituationRoom.Api.Extensions;

using Api.Configurations;
using Controllers.Base;
using Data.Context;
using Data.Interface;
using Profiles;

public static class DependencyRegistrationExtension
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Web
        services.AddSwagger();

        // Database
        services.AddTransient<ISituationContext, SituationContext>();

        // REST Infrastructure
        services.AddScoped<IControllerInfrastructure, ControllerInfrastructure>();

        // Logging
        services.AddLogging();

        // Mapping
        services.AddAutoMapper(typeof(SituationRoomProfile));

        return services;
    }
}