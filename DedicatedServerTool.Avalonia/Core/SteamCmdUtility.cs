using System.Diagnostics;
using System.Threading.Tasks;

namespace DedicatedServerTool.Avalonia.Core;
internal static class SteamCmdUtility
{
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
}
