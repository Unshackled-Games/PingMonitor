using System;
using System.IO;
using System.Text.Json;
using PingMonitor.Models;
using PingMonitor.Serialization;

namespace PingMonitor.Services;

internal static class AppStateStorage
{
    private const string FileName = "state.json";

    private static string GetFilePath()
    {
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(dir))
            dir = AppContext.BaseDirectory;

        var path = Path.Combine(dir, "PingMonitor");
        Directory.CreateDirectory(path);
        return Path.Combine(path, FileName);
    }

    public static AppState Load()
    {
        var path = GetFilePath();
        if (!File.Exists(path))
            return new AppState();

        try
        {
            var json = File.ReadAllText(path);
            var state = JsonSerializer.Deserialize(json, AppStateJsonContext.Default.AppState);
            return state ?? new AppState();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new AppState();
        }
    }

    public static void Save(AppState state)
    {
        try
        {
            var path = GetFilePath();
            var json = JsonSerializer.Serialize(state, AppStateJsonContext.Default.AppState);
            File.WriteAllText(path, json);
        }
        catch
        {
            // best-effort only
        }
    }
}
