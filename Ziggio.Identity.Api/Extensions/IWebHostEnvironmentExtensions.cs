namespace Ziggio.Identity.Api;

public static class IWebHostEnvironmentExtensions
{
    public static bool IsTesting(this IWebHostEnvironment env)
    {
        return env.EnvironmentName == "Testing" || env.EnvironmentName == "Test";
    }
}

