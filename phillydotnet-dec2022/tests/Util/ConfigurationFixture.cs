using Microsoft.Extensions.Configuration;

namespace tests.Util;

// ReSharper disable once ClassNeverInstantiated.Global
public class ConfigurationFixture
{
    public ConfigurationFixture()
    {
        Config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
    }

    public IConfigurationRoot Config { get; }
}