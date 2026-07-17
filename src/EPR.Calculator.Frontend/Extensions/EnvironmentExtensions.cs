using Environment = EPR.Calculator.Frontend.Constants.Environment;

namespace EPR.Calculator.Frontend.Extensions;

public static class EnvironmentExtensions
{
    public static bool IsLocal(this IHostEnvironment hostEnvironment) => hostEnvironment.IsEnvironment(Environment.Local);
}
