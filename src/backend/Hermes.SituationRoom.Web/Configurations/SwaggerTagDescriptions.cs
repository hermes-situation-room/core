using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Hermes.SituationRoom.Api.Configurations;

public class SwaggerTagDescriptions : IDocumentFilter
{
    public const string ENDPOINT_TAG_EXTERNAL = "External";
    public const string ENDPOINT_TAG_INTERNAL = "Internal";
    public const string ENDPOINT_TAG_INTERNAL_CODETABLES = "Codetable";
    public const string ENDPOINT_TAG_INTERNAL_ACTIVIST = "Internal Activist";
    public const string ENDPOINT_TAG_INTERNAL_CHAT = "Internal Chat";
    public const string ENDPOINT_TAG_INTERNAL_POST = "Internal Post";
    public const string ENDPOINT_TAG_INTERNAL_COMMENT = "Internal Comment";
    public const string ENDPOINT_TAG_INTERNAL_JOURNALIST = "Internal Journalist";
    public const string ENDPOINT_TAG_INTERNAL_USER = "Internal User";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        const string internalScopeDescription = $"Internal endpoints for internal system operations within the SituationRoom system. Scope: \"Internal\"";

        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new() { Name = ENDPOINT_TAG_EXTERNAL, Description = $"Public-facing endpoints. Scope: \"External\"" },
            new() { Name = ENDPOINT_TAG_INTERNAL, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_CODETABLES, Description = $"Internal endpoints for internal codetable operations within the Customer system. Scope: \"Codetable\"" },
            new() { Name = ENDPOINT_TAG_INTERNAL_ACTIVIST, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_CHAT, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_POST, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_COMMENT, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_JOURNALIST, Description = internalScopeDescription },
            new() { Name = ENDPOINT_TAG_INTERNAL_USER, Description = internalScopeDescription },
        };
    }
}
