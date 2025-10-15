namespace Hermes.SituationRoom.Api.Extensions;

using Configurations;
using Controllers.Base;
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
        
        // Data
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IActivistRepository, ActivistRepository>();
        services.AddScoped<IJournalistRepository, JournalistRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPrivacyLevelPersonalRepository, PrivacyLevelPersonalRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IUserChatReadStatusRepository, UserChatReadStatusRepository>();
        
        // Domain
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IActivistService, ActivistService>();
        services.AddScoped<IJournalistService, JournalistService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPrivacyLevelPersonalService, PrivacyLevelPersonalService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IUserChatReadStatusService, UserChatReadStatusService>();

        services.AddScoped<IEncryptionService, EncryptionService>();

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