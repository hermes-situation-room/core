using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Hermes.SituationRoom.Api.Configurations;

internal static class SwaggerSetupExtensions
{
    internal static IApplicationBuilder UseSwaggerAndSwaggerUI(this IApplicationBuilder app)
    {
        app.UseSwagger();

        return app.UseSwaggerUI(config =>
        {
            var version = ApiVersionUtilities.GetApiVersion();
            var assemblyName = ApiVersionUtilities.GetApiAssemblyName();

            config.SwaggerEndpoint($"v{version}/swagger.json", $"{assemblyName} v{version}");
            config.DocumentTitle = "Swagger SituationRoom API";
        });
    }

    internal static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            AddSwaggerCommonConfig(options);

            options.EnableAnnotations();
            options.DocumentFilter<SwaggerTagDescriptions>();
        });

        return services;
    }

    private static void AddSwaggerCommonConfig(SwaggerGenOptions options)
    {
        var version = ApiVersionUtilities.GetApiVersion();
        var assemblyName = ApiVersionUtilities.GetApiAssemblyName();

        options.SwaggerDoc($"v{version}", new OpenApiInfo { Title = assemblyName, Version = $"v{version}" });
        options.OrderActionsBy(
            (apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
    }
}