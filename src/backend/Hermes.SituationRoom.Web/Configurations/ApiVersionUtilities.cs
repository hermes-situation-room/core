using System.Reflection;

namespace Hermes.SituationRoom.Api.Configurations;

internal static class ApiVersionUtilities
{
    private const string FallbackApiVersion = "1.0.0";
    private const string NotSetAssemlbyVersion = "0.0.0.0";

    internal static string GetApiAssemblyName()
    {
        return Assembly.GetExecutingAssembly().GetName().Name!;
    }

    internal static string GetApiVersion()
    {
        return GetAssemblyVersion() ?? FallbackApiVersion;
    }

    private static string GetAssemblyVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version!;
        if (version.ToString() == NotSetAssemlbyVersion)
        {
            // The Assembly version is set by GitVersion during the CI build. 
            // When the service is started on a local machine, we use the fallback. 
            return null;
        }

        // version.Build: Equivalent to the “patch” in semantic versioning, incremented for bug fixes and small improvements.
        return $"{version.Major}.{version.Minor}.{version.Build}";
    }
}