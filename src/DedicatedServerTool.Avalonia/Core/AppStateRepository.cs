﻿using DedicatedServerTool.Avalonia.Models;
using System.IO;
using System.Text.Json;

namespace DedicatedServerTool.Avalonia.Core
{
    internal static class AppStateRepository
    {
        private static readonly string FilePath = "appstate.json";

        public static void Save(AppState appState)
        {
            string jsonString = JsonSerializer.Serialize(appState);
            File.WriteAllText(FilePath, jsonString);
        }

        public static AppState Load()
        {
            if (File.Exists(FilePath))
            {
                string jsonString = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppState>(jsonString) ?? new();
            }
            return new();
        }
    }
}
