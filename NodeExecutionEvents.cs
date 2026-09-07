#if UNITY_EDITOR

using System;

public static class NodeExecutionEvents
{
    public static Action<int> OnNodeStarted;
    public static Action<int> OnNodeFinished;
}
#endif
