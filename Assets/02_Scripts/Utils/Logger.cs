using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 유니티 에디터 전용 커스텀 로거
/// </summary>
public static class Logger
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.DebuggerStepThrough]
    public static void Log(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0
        )
    {
#if UNITY_EDITOR
        string className = System.IO.Path.GetFileNameWithoutExtension(filePath);
        Debug.Log($"[{className}.{memberName}:{lineNumber}] {message}");
#endif
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.DebuggerStepThrough]
    public static void LogWarning(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0
        )
    {
#if UNITY_EDITOR
        string className = System.IO.Path.GetFileNameWithoutExtension(filePath);
        Debug.LogWarning($"[{className}.{memberName}:{lineNumber}] {message}");
#endif
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.DebuggerStepThrough]
    public static void LogError(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0
        )
    {
#if UNITY_EDITOR
        string className = System.IO.Path.GetFileNameWithoutExtension(filePath);
        Debug.LogError($"[{className}.{memberName}:{lineNumber}] {message}");
#endif
    }
}

