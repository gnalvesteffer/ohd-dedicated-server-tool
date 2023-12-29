using DedicatedServerTool.Avalonia.Models;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DedicatedServerTool.Avalonia.Core;
internal static class ServerUtility
{
    public static void StartServer(ServerProfile profile)
    {
        var queryParameters = new StringBuilder();
        queryParameters.Append($"?game={profile.GameModePath.Trim()}");
        if (profile.MaxPlayers.HasValue)
        {
            queryParameters.Append($"?MaxPlayers={profile.MaxPlayers}");
        }
        if (profile.BluforBotCount.HasValue)
        {
            queryParameters.Append($"?BluforNumBots={profile.BluforBotCount}");
        }
        if (profile.OpforBotCount.HasValue)
        {
            queryParameters.Append($"?OpforNumBots={profile.OpforBotCount}");
        }
        if (profile.BluforTickets.HasValue)
        {
            queryParameters.Append($"?BluforNumTickets={profile.BluforTickets}");
        }
        if (profile.OpforTickets.HasValue)
        {
            queryParameters.Append($"?OpforNumTickets={profile.OpforTickets}");
        }
        if (!string.IsNullOrWhiteSpace(profile.BluforFactionName))
        {
            queryParameters.Append($"?BluforFaction={profile.BluforFactionName.Trim()}");
        }
        if (!string.IsNullOrWhiteSpace(profile.OpforFactionName))
        {
            queryParameters.Append($"?OpforFaction={profile.OpforFactionName.Trim()}");
        }
        if (profile.DisableKitRestrictions)
        {
            queryParameters.Append($"?DisableKitRestrictions={profile.OpforFactionName.Trim()}");
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            WorkingDirectory = profile.InstallDirectory,
            FileName = Path.Combine(profile.InstallDirectory, @"HarshDoorstop\Binaries\Win64\HarshDoorstopServer-Win64-Shipping.exe"),
            Arguments = $"{profile.InitialMapName}{queryParameters} -log -Port={profile.Port} -QueryPort={profile.QueryPort} -SteamServerName=\"{profile.ServerName}\" %*",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        using Process process = new Process { StartInfo = startInfo };
        process.Start();
    }
}
