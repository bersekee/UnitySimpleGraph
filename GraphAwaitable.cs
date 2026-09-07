using UnityEngine;

public static class GraphAwaitable
{
    public static Awaitable CompletedAwaitable => Complete();

    private static Awaitable Complete()
    {
        var source = new AwaitableCompletionSource();
        source.SetResult();
        return source.Awaitable;
    }
}
