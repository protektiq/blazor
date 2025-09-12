using Microsoft.Playwright;

namespace CustomerSupportSystem.Tests;

public static class TestConfiguration
{
    public const string BaseUrl = "http://localhost:5231";
    public const int Timeout = 30000; // 30 seconds
    public const int NavigationTimeout = 10000; // 10 seconds
}
