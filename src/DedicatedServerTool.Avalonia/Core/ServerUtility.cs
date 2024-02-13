﻿using DedicatedServerTool.Avalonia.Models;
using DedicatedServerTool.Avalonia.Views;
using Open.Nat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DedicatedServerTool.Avalonia.Core;
internal static class ServerUtility
{
    private static readonly HashSet<int> CleanExitCodes = new HashSet<int> { 0, -1073741510 };

    public static async Task StartServerAsync(ServerProfile profile)
    {
        if (!profile.Port.HasValue || !profile.ClientPort.HasValue || !profile.QueryPort.HasValue || !profile.RconPort.HasValue)
        {
            throw new InvalidOperationException("Unable to start server due to undefined port configurations.");
        }

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
        var multihomeIp = string.IsNullOrWhiteSpace(profile.MultihomeIp) ? "0.0.0.0" : profile.MultihomeIp;

        WriteIniAndConfigFiles(profile);

        ProcessStartInfo serverProcessStartInfo = new ProcessStartInfo
        {
            WorkingDirectory = profile.InstallDirectory,
            FileName = Path.Combine(profile.InstallDirectory, @"HarshDoorstop\Binaries\Win64\HarshDoorstopServer-Win64-Shipping.exe"),
            Arguments = $"{profile.InitialMapName}{queryParameters} -log -Port={profile.Port} -QueryPort={profile.QueryPort} -RCONPort={profile.RconPort} -SteamServerName=\"{profile.ServerName}\" -MULTIHOME={multihomeIp} %*"
        };

        var useUpnpForPortForwarding = profile.ShouldUseUpnpForPortForwarding;
        var portForwardManager = new PortForwardManager();
        try
        {
            if (useUpnpForPortForwarding)
            {
                await portForwardManager.OpenPortsAsync(profile.Port.Value, profile.ClientPort.Value, profile.QueryPort.Value, profile.RconPort.Value);
            }

            do
            {
                if (profile.ShouldUpdateBeforeStarting)
                {
                    while (true) // retry until this SteamCMD works -- SteamCMD can sometimes be unreliable, so just keep trying...
                    {
                        var failedWorkshopIds = await UpdateServerAndModsAsync(profile);
                        if (!failedWorkshopIds.Any())
                        {
                            break;
                        }
                    }
                }

                var isRestarting = false;
                using Process serverProcess = new() { StartInfo = serverProcessStartInfo };
                serverProcess.Start();

                var tasks = new List<Task>
                {
                    serverProcess.WaitForExitAsync()
                };
                if (profile.RestartIntervalHours > 0)
                {
                    tasks.Add(Task.Delay(TimeSpan.FromHours(profile.RestartIntervalHours.Value)).ContinueWith(_ => isRestarting = true));
                }
                await Task.WhenAny(tasks); // wait for either the server to crash or to restart

                if (!serverProcess.HasExited)
                {
                    serverProcess.CloseMainWindow();
                }

                await serverProcess.WaitForExitAsync();


                if (!isRestarting && CleanExitCodes.Contains(serverProcess.ExitCode))
                {
                    break;
                }
            } while (profile.ShouldRestartOnCrash || profile.RestartIntervalHours.HasValue);
        }
        catch (MappingException exception)
        {
            new MessageBoxWindow("Failed to start server", $"Failed to configure port forwarding via uPnP. Try again or ensure that the server's ports aren't already reserved. Details: {exception.Message}").Show();
        }
        catch (Exception exception)
        {
            new MessageBoxWindow("Failed to start server", exception.Message).Show();
        }
        finally
        {
            await portForwardManager.ClosePortsAsync();
        }
    }

    public static async Task<IEnumerable<long>> UpdateServerAndModsAsync(ServerProfile serverProfile)
    {
        await SteamCmdUtility.DownloadOrUpdateDedicatedServerAsync(serverProfile.InstallDirectory);
        var failedWorkshopIds = new List<long>();
        foreach (var workshopId in serverProfile.GetInstalledWorkshopIds())
        {
            if (!await SteamCmdUtility.DownloadOrUpdateModAsync(serverProfile.InstallDirectory, workshopId))
            {
                failedWorkshopIds.Add(workshopId);
            }
        }
        return failedWorkshopIds;
    }

    public static void WriteIniAndConfigFiles(ServerProfile profile)
    {
        WriteGameIni(profile);
        WriteEngineIni(profile);
        WriteMapCycleConfig(profile);
        WriteAdminConfig(profile);
    }

    private static void WriteGameIni(ServerProfile profile)
    {
        var gameIniContents = new StringBuilder();

        gameIniContents.AppendLine("[/Script/RCON.RCONServerSystem]");
        gameIniContents.AppendLine($"bEnabled={profile.IsRconEnabled}");
        gameIniContents.AppendLine($"ListenPort={profile.RconPort}");
        gameIniContents.AppendLine($"Password={profile.RconPassword}");
        gameIniContents.AppendLine("MaxActiveConnections=5");
        gameIniContents.AppendLine("MaxAuthAttempts=3");

        gameIniContents.AppendLine("[/Script/DonkehFramework.DFBaseGameMode]");
        gameIniContents.AppendLine($"bAllowVoting={profile.IsVoteKickingEnabled}");

        gameIniContents.AppendLine("[/Script/DonkehFramework.DFVoteIssuePlayerKick]");
        gameIniContents.AppendLine($"BanDuration={profile.VoteKickBanDurationSeconds}");
        gameIniContents.AppendLine($"PassRatio={profile.VoteKickPassRatio}");
        gameIniContents.AppendLine($"Duration={profile.VoteKickPollDurationSeconds}");
        gameIniContents.AppendLine($"PassedVoteCooldown={profile.VoteKickCooldownSeconds}");
        gameIniContents.AppendLine($"FailedVoteCooldown={profile.VoteKickCooldownSeconds}");

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
        engineIniContents.AppendLine($"HD.CP.MinPlayersToCaptureOverride={profile.MinPlayersToCaptureOverride}");

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
