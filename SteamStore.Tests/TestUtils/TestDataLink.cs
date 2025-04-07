using Microsoft.Extensions.Configuration;
using SteamStore.Data;

namespace SteamStore.Tests.TestUtils;

public static class TestDataLink
{
    public static IDataLink GetDataLink()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("testsettings.json")
            .Build();

        return new DataLink(configuration);
    }
}