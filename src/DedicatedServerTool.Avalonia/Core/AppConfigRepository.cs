using DedicatedServerTool.Avalonia.Models;
using System.IO;
using System.Text.Json;

namespace DedicatedServerTool.Avalonia.Core
{
    internal static class AppConfigRepository
    {
        private static readonly string FilePath = "appconfig.json";

        private static AppConfig? _appConfigInstance;

        public static void Save(AppState appState)
        {
            string jsonString = JsonSerializer.Serialize(appState);
            File.WriteAllText(FilePath, jsonString);
        }

        public static AppConfig Load()
        {
            if (_appConfigInstance != null)
            {
                return _appConfigInstance;
            }
            if (File.Exists(FilePath))
            {
                string jsonString = File.ReadAllText(FilePath);
                return _appConfigInstance = JsonSerializer.Deserialize<AppConfig>(jsonString) ?? new();
            }
            return new();
        }
    }
}
