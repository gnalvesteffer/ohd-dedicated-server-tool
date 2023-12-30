using DedicatedServerTool.Avalonia.Models;
using System.IO;
using System.Text.Json;

namespace DedicatedServerTool.Avalonia.Core
{
    internal static class AppStateRepository
    {
        private static readonly string FilePath = "appstate.json";

        public static void Save(AppState appState)
        {
            // Serialize the AppState object to JSON
            string jsonString = JsonSerializer.Serialize(appState);

            // Write the JSON string to the file
            File.WriteAllText(FilePath, jsonString);
        }

        public static AppState Load()
        {
            // Check if the file exists
            if (File.Exists(FilePath))
            {
                // Read the JSON string from the file
                string jsonString = File.ReadAllText(FilePath);

                // Deserialize the JSON string to an AppState object
                return JsonSerializer.Deserialize<AppState>(jsonString) ?? new();
            }

            // If the file doesn't exist, return a new instance of AppState
            return new();
        }
    }
}
