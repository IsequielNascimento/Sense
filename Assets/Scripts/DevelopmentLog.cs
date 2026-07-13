using System.Diagnostics;

/// <summary>
/// Verbose tracing that is compiled out of release builds.
/// </summary>
public static class DevelopmentLog
{
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message)
    {
        UnityEngine.Debug.Log(message);
    }
}
