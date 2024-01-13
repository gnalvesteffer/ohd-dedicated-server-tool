using DedicatedServerTool.Avalonia.Models;
using DedicatedServerTool.Avalonia.Views;
using Open.Nat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        ProcessStartInfo startInfo = new ProcessStartInfo
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
                using Process process = new Process { StartInfo = startInfo };
                Thread modCheckTask = new Thread(async () => await CheckForOutOfDateProfileModsAsync(profile, process));
                process.Start();

                if (profile.ShouldAutoUpdateMods)
                {
                    modCheckTask.Start();
                }
              
                if (profile.ShouldUpdateBeforeStarting)
                {
                    await UpdateServerAndModsAsync(profile);
                }

                var isRestarting = false;
                using Process process = new() { StartInfo = startInfo };
                process.Start();

                var tasks = new List<Task>
                {
                    process.WaitForExitAsync()
                };
                if (profile.RestartIntervalHours > 0)
                {
                    tasks.Add(Task.Delay(TimeSpan.FromHours(profile.RestartIntervalHours.Value)).ContinueWith(_ => isRestarting = true));
                }
                await Task.WhenAny(tasks); // wait for either the server to crash or to restart

                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                }

                await process.WaitForExitAsync();

                if (CleanExitCodes.Contains(process.ExitCode))
                {
                    break;
                }
                else if (!profile.ShouldRestartOnCrash && !profile.ShouldAutoUpdateMods)
                {
                    break; // allow server to crash if should restart setting isn't enabled
                }
                
                if (profile.ShouldAutoUpdateMods)
                {

                    var updateModsTask = Parallel.ForEachAsync(profile.GetInstalledWorkshopIds(), async (workshopId, cancellationToken) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        await SteamCmdUtility.DownloadOrUpdateModAsync(profile.InstallDirectory, workshopId);
                    });

                    await Task.WhenAll(updateModsTask);
                }
              
                if (!isRestarting && CleanExitCodes.Contains(process.ExitCode))
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

    private static async Task CheckForOutOfDateProfileModsAsync(ServerProfile profile, Process process)
    {
        while (true)
        {
            Thread.Sleep((1000 * 5));
            process.Refresh();
            try
            {
                if (!process.HasExited)
                {
                    var updateModsTask = Parallel.ForEachAsync(profile.GetInstalledWorkshopIds(), async (workshopId, cancellationToken) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        await SteamCmdUtility.DownloadOrUpdateModAsync(profile.InstallDirectory, workshopId);
                    });

                    await Task.WhenAll(updateModsTask);

                    bool hasOutOfDateMods = await PofileHasOutOfDateModsAsync(profile);

                    if (hasOutOfDateMods)
                    {
                        process.Kill(); // TODO: this is not very nice

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // the process has likely been manually stopped.
                break;
            }
        }
        // Perform checks for outdated mods
    }

    private static async Task<bool> PofileHasOutOfDateModsAsync(ServerProfile profile)
    {
        try
        {
            //DateTime modLastUpdated = await InstalledWorkshopModUtility.ScrapeWorkshopItemLastUpdated(3127339017);
            return await InstalledWorkshopModUtility.HasOutOfDateModsAsync(profile.InstallDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong with update check");
        }

        return false;
    }

    public static Task UpdateServerAndModsAsync(ServerProfile serverProfile)
    {
        var updateServerTask = SteamCmdUtility.DownloadOrUpdateDedicatedServerAsync(serverProfile.InstallDirectory);
        var updateModsTask = Parallel.ForEachAsync(serverProfile.GetInstalledWorkshopIds(), async (workshopId, cancellationToken) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            await SteamCmdUtility.DownloadOrUpdateModAsync(serverProfile.InstallDirectory, workshopId);
        });
        return Task.WhenAll(updateServerTask, updateModsTask);
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
