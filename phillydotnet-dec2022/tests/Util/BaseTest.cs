using Microsoft.Extensions.Configuration;
using Serilog;
using Xunit.Abstractions;

namespace tests.Util;

public class BaseTest
{
    protected readonly IConfigurationRoot ConfigurationRoot;

    protected BaseTest(ConfigurationFixture configuration, ITestOutputHelper output)
    {
        //
        //     Pass the ITestOutputHelper object to the TestOutput sink
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output)
            .CreateLogger();

        ConfigurationRoot = configuration.Config;
    }
}