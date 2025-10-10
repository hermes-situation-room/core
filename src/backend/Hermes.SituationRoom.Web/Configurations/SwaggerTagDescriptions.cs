using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Hermes.SituationRoom.Api.Configurations;

public class SwaggerTagDescriptions : IDocumentFilter
{
    public const string ENDPOINT_TAG_EXTERNAL = "External";
    public const string ENDPOINT_TAG_INTERNAL_ACTIVIST = "Internal Activist";
    public const string ENDPOINT_TAG_INTERNAL_CHAT = "Internal Chat";
    public const string ENDPOINT_TAG_INTERNAL_POST = "Internal Post";
    public const string ENDPOINT_TAG_INTERNAL_TAG = "Internal Tag";
    public const string ENDPOINT_TAG_INTERNAL_COMMENT = "Internal Comment";
    public const string ENDPOINT_TAG_INTERNAL_JOURNALIST = "Internal Journalist";
    public const string ENDPOINT_TAG_INTERNAL_USER = "Internal User";
    public const string ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL = "Internal Privacy Level";
    public const string ENDPOINT_TAG_INTERNAL_MESSAGE = "Internal Message";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        const string internalScopeDescription = $"Internal endpoints for internal system operations within the SituationRoom system. Scope: \"Internal\"";

        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new() { Name = ENDPOINT_TAG_EXTERNAL, Description = $"Public-facing endpoints. Scope: \"External\"" },
            new() { Name = ENDPOINT_TAG_INTERNAL_ACTIVIST, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_CHAT, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_POST, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_TAG, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_COMMENT, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_JOURNALIST, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_USER, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_MESSAGE, Description = internalScopeDescription },
        };
    }
}
