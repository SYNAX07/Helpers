using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Helpers.AspNetCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     A ServiceCollection extension method to add
    /// an in-memory collection as configuration.
    ///
    /// var data = new Dictionary<string, string?>
    /// {
    ///     { "Key", "Value" }
    /// };
    /// var services = new ServiceCollection()
    ///                  .WithConfiguration(data);
    /// services.AddMyModule();
    ///
    /// var provider = services.BuildServiceProvider();
    /// provider.GetRequiredService<SomeService>();
    ///
    /// </summary>
    /// <param name="self">The @this to act on.</param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static ServiceCollection WithConfiguration(
        this ServiceCollection self,
        Dictionary<string, string?> data)
    {
        var configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(data)
                        .Build();

        self.AddSingleton<IConfiguration>(configuration);

        return self;
    }

    public static ServiceCollection Remove<T>(
        this ServiceCollection self)
    {
        var descriptor = self.FirstOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
        {
            self.Remove(descriptor);
        }

        return self;
    }
}