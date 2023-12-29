using DedicatedServerTool.Avalonia.Models;
using System.Diagnostics;
using System.IO;

namespace DedicatedServerTool.Avalonia.Core;
internal static class ServerUtility
{
    public static void StartServer(ServerProfile profile)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            WorkingDirectory = profile.InstallDirectory,
            FileName = Path.Combine(profile.InstallDirectory, @"HarshDoorstop\Binaries\Win64\HarshDoorstopServer-Win64-Shipping.exe"),
            Arguments = $"{profile.InitialMapName}?{profile.MaxPlayers} -log -Port={profile.Port} -QueryPort={profile.QueryPort} -SteamServerName=\"{profile.ServerName}\" %*",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        using Process process = new Process { StartInfo = startInfo };
        process.Start();
    }
}
