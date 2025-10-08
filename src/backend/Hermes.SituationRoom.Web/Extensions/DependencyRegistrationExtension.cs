namespace Hermes.SituationRoom.Api.Extensions;

using Configurations;
using Controllers.Base;
using Data.Context;
using Data.Interface;
using Data.Migrations;
using Data.Repositories;
using Domain.Interfaces;
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
        services.AddTransient<IPostRepository, PostRepository>();
        services.AddTransient<IPrivacyLevelPersonalRepository, PrivacyLevelPersonalRepository>();
        services.AddTransient<ICommentRepository, CommentRepository>();
        
        // Domain
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IActivistService, ActivistService>();
        services.AddTransient<IJournalistService, JournalistService>();
        services.AddTransient<IPostService, PostService>();
        services.AddTransient<IPrivacyLevelPersonalService, PrivacyLevelPersonalService>();
        services.AddTransient<ICommentService, CommentService>();
        
        var connectionString = configuration.GetConnectionString("SituationRoomDb");
        services.AddDatabaseMigrations(connectionString,
            Path.Combine(AppContext.BaseDirectory, "Migrations"));
        
        // Data
        services.AddScoped<IChatRepository, ChatRepository>();
        
        //Domain
        services.AddScoped<IChatService, ChatService>();

        // REST Infrastructure
        services.AddScoped<IControllerInfrastructure, ControllerInfrastructure>();

        // Logging
        services.AddLogging();

        // Mapping
        services.AddAutoMapper(typeof(SituationRoomProfile));

        return services;
    }
}