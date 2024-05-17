using Microsoft.Extensions.Configuration;

namespace Helpers.AspNetCore;

public static class ConfigurationExtensions
{
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Missing connection string '{name}'");
        }

        return connectionString;
    }
}