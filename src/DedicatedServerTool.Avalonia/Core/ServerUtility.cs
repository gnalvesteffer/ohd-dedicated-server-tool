using DedicatedServerTool.Avalonia.Models;
using System;
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
        if (!string.IsNullOrWhiteSpace(profile.ServerPassword))
        {
            queryParameters.Append($"?Password={profile.ServerPassword}");
        }
        if (profile.MinPlayers.HasValue)
        {
            queryParameters.Append($"?MinPlayers={profile.MinPlayers}");
        }
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
        if (profile.ShouldDisableKitRestrictions)
        {
            queryParameters.Append($"?DisableKitRestrictions");
        }
        if (profile.IsBotAutoFillEnabled)
        {
            queryParameters.Append($"?bBotAutofill");
        }
        if (profile.TeamIdToAutoAssignHumans.HasValue)
        {
            queryParameters.Append($"?AutoAssignHuman={profile.TeamIdToAutoAssignHumans}");
        }

        WriteIniAndConfigFiles(profile);

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

    private static void WriteIniAndConfigFiles(ServerProfile profile)
    {
        WriteGameIni(profile);
        WriteEngineIni(profile);
        WriteMapCycleConfig(profile);
        WriteAdminConfig(profile);
    }

    private static void WriteGameIni(ServerProfile profile)
    {
        var gameIniContents = new StringBuilder();

        var gameIniPath = Path.Combine(profile.InstallDirectory, @"HarshDoorstop\Saved\Config\WindowsServer\Game.ini");
        Directory.CreateDirectory(Path.GetDirectoryName(gameIniPath)!);
        File.WriteAllText(gameIniPath, gameIniContents.ToString());
    }

    private static void WriteEngineIni(ServerProfile profile)
    {
        var engineIniContents = new StringBuilder();
        engineIniContents.AppendLine("[SystemSettings]");
        engineIniContents.AppendLine($"Game.FriendlyFire={Convert.ToInt32(profile.IsFriendlyFireEnabled)}");
        engineIniContents.AppendLine($"HD.Game.MinRespawnDelayOverride={profile.RespawnDurationSeconds}");
        engineIniContents.AppendLine($"HD.Game.DisableKitRestrictionsOverride={Convert.ToInt32(profile.ShouldDisableKitRestrictions)}");
        engineIniContents.AppendLine($"Game.AutoBalanceTeamsOverride={Convert.ToInt32(profile.IsAutoBalanceEnabled)}");

        var engineIniPath = Path.Combine(profile.InstallDirectory, @"HarshDoorstop\Saved\Config\WindowsServer\Engine.ini");
        Directory.CreateDirectory(Path.GetDirectoryName(engineIniPath)!);
        File.WriteAllText(engineIniPath, engineIniContents.ToString());
    }

    private static void WriteMapCycleConfig(ServerProfile profile)
    {
        var stringBuilder = new StringBuilder(profile.MapCycleText.Trim());
        stringBuilder.AppendLine(); // Blank line to "finalize" the file

        var mapCycleConfigPath = Path.Combine(profile.InstallDirectory, @"HarshDoorstop\Saved\Config\WindowsServer\MapCycle.cfg");
        Directory.CreateDirectory(Path.GetDirectoryName(mapCycleConfigPath)!);
        File.WriteAllText(mapCycleConfigPath, stringBuilder.ToString());
    }

    private static void WriteAdminConfig(ServerProfile profile)
    {
        var stringBuilder = new StringBuilder(profile.AdminSteamIdsText.Trim());
        stringBuilder.AppendLine(); // Blank line to "finalize" the file

        var adminsConfigPath = Path.Combine(profile.InstallDirectory, @"HarshDoorstop\Saved\Config\WindowsServer\Admins.cfg");
        Directory.CreateDirectory(Path.GetDirectoryName(adminsConfigPath)!);
        File.WriteAllText(adminsConfigPath, stringBuilder.ToString());
    }

}
