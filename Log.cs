using BepInEx.Logging;
using System;

internal static class Log
{
    private static ManualLogSource _instance;

    internal static void CreateInstance(ManualLogSource source)
    {
        _instance = source;
    }

    public static void Info(object msg) => _instance.LogInfo(FormatMessage(msg));

    public static void Message(object msg) => _instance.LogMessage(FormatMessage(msg));

    public static void Debug(object msg) => _instance.LogDebug(FormatMessage(msg));

    public static void Warning(object msg) => _instance.LogWarning(FormatMessage(msg));

    public static void Error(object msg) => _instance.LogError(FormatMessage(msg));

    public static void Fatal(object msg) => _instance.LogFatal(FormatMessage(msg));

    private static string FormatMessage(object msg) => $"[{DateTime.UtcNow:G}] {msg}";
}
