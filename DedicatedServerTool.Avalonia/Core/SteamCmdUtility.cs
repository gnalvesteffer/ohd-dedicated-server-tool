using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DedicatedServerTool.Avalonia.Core
{
    internal static class SteamCmdUtility
    {
        private static readonly int GameAppId = 736590;
        private static readonly int DedicatedServerAppId = 950900;
        private static readonly string steamCmdPath = "steamcmd.exe";

        public static Task<bool> DownloadOrUpdateDedicatedServerAsync(string installDirectory)
        {
            return Task.Run(() =>
            {
                // Create the process
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = steamCmdPath,
                        Arguments = $"+force_install_dir \"{installDirectory}\" +login anonymous +app_update {DedicatedServerAppId} validate +quit",
                        UseShellExecute = false,
                        CreateNoWindow = false
                    }
                };

                // Start the process
                process.Start();

                // Wait for the process to exit
                process.WaitForExit();

                // Return the result
                return process.ExitCode == 0;
            });
        }

        public static Task<bool> DownloadOrUpdateModAsync(string installDirectory, long workshopId)
        {
            return Task.Run(() =>
            {
                // Create mod directory
                var modDirectory = Path.Combine(installDirectory, @"HarshDoorstop\Mods");
                Directory.CreateDirectory(modDirectory);

                // Create the process
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = steamCmdPath,
                        Arguments = $"+force_install_dir \"{installDirectory}\" +login anonymous +workshop_download_item {GameAppId} {workshopId} +quit",
                        UseShellExecute = false,
                        CreateNoWindow = false
                    }
                };

                // Start the process
                process.Start();

                // Wait for the process to exit
                process.WaitForExit();

                // Check if mod was downloaded
                var workshopItemDirectory = Path.Combine(installDirectory, @$"steamapps\workshop\content\{GameAppId}\{workshopId}");
                if (!Directory.Exists(workshopItemDirectory))
                {
                    return false;
                }

                // Create a symbolic link to map the SteamCMD download location to the game mod directory
                var workshopNestedModDirectory = Directory.GetDirectories(workshopItemDirectory).FirstOrDefault();
                if (workshopNestedModDirectory == null)
                {
                    return false;
                }
                var modFolderName = Path.GetFileName(workshopNestedModDirectory)!;
                var gameDesiredModFolderPath = Path.Combine(modDirectory, modFolderName);
                if (!Directory.Exists(gameDesiredModFolderPath))
                {
                    Directory.CreateSymbolicLink(gameDesiredModFolderPath, workshopNestedModDirectory);
                }

                // Return the result
                return true;
            });
        }

        public static Task<bool> UnsubscribeFromModAsync(string installDirectory, long workshopId)
        {
            return Task.Run(() =>
            {
                // Unsubscribe using SteamCMD
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = steamCmdPath,
                        Arguments = $"+force_install_dir \"{installDirectory}\" +login anonymous +workshop_download_item {GameAppId} -{workshopId} +quit",
                        UseShellExecute = false,
                        CreateNoWindow = false
                    }
                };

                // Start the process
                process.Start();

                // Wait for the process to exit
                process.WaitForExit();

                // Check if the unsubscribe was successful
                if (process.ExitCode != 0)
                {
                    return false;
                }

                // Remove the symbolic link
                var modDirectory = Path.Combine(installDirectory, @"HarshDoorstop\Mods");
                var workshopItemDirectory = Path.Combine(installDirectory, @$"steamapps\workshop\content\{GameAppId}\{workshopId}");
                var workshopNestedModDirectory = Directory.GetDirectories(workshopItemDirectory).FirstOrDefault();
                if (workshopNestedModDirectory == null)
                {
                    return false;
                }

                var modFolderName = Path.GetFileName(workshopNestedModDirectory)!;
                var gameDesiredModFolderPath = Path.Combine(modDirectory, modFolderName);

                if (Directory.Exists(gameDesiredModFolderPath))
                {
                    // Remove the symbolic link
                    Directory.Delete(gameDesiredModFolderPath, true);

                    // Optionally, you can delete the actual mod files if needed
                    Directory.Delete(workshopNestedModDirectory, true);
                }

                // Return the result
                return true;
            });
        }
    }
}
