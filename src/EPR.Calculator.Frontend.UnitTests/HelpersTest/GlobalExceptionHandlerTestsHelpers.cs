using System.Runtime.ExceptionServices;

internal static class GlobalExceptionHandlerTestsHelpers
{
    public static Exception CreateExceptionWithDummyStackTrace(string message, string dummyStackTrace)
    {
        try
        {
            throw new InvalidOperationException(message);
        }
        catch (Exception ex)
        {
            var edi = ExceptionDispatchInfo.Capture(ex);
            return edi.SourceException;
        }
    }
}