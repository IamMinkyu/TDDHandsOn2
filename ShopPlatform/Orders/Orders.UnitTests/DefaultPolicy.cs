using Polly;

namespace Orders;

public static class DefaultPolicy
{
    private readonly static Random Random = new();
    public static IAsyncPolicy Instance { get; } =
        Policy.Handle<Exception>().WaitAndRetryAsync(5, CalculateDelay);

    private static TimeSpan CalculateDelay(int retries)
    {
        int delayMilliseconds = 100;
        return TimeSpan.FromMilliseconds(delayMilliseconds);
    }
}
