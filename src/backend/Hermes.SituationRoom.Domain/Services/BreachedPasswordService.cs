namespace Hermes.SituationRoom.Domain.Services;

using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Interfaces;
using Microsoft.Extensions.Logging;

public sealed class BreachedPasswordService(IHttpClientFactory httpClientFactory, ILogger<BreachedPasswordService> logger) : IBreachedPasswordService
{
    private const int PrefixLength = 5;

    public async Task<bool> IsBreachedAsync(string password)
    {
        var hash = ComputeSha1Hex(password);
        var prefix = hash[..PrefixLength];
        var suffix = hash[PrefixLength..];

        try
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SituationRoom");

            var response = await httpClient.GetAsync($"https://api.pwnedpasswords.com/range/{prefix}");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var lines = body.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return lines.Any(line =>
            {
                var separatorIndex = line.IndexOf(':');
                var returnedSuffix = separatorIndex >= 0 ? line[..separatorIndex] : line;
                return string.Equals(returnedSuffix, suffix, StringComparison.OrdinalIgnoreCase);
            });
        }
        catch (HttpRequestException exception)
        {
            // Fail open: never block registration on an outage of the breach-check service.
            logger.LogWarning(exception, "Breached-password check failed; allowing password (fail-open).");
            return false;
        }
        catch (TaskCanceledException exception)
        {
            logger.LogWarning(exception, "Breached-password check timed out; allowing password (fail-open).");
            return false;
        }
    }

    private static string ComputeSha1Hex(string password)
    {
        var hashBytes = SHA1.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hashBytes);
    }
}
