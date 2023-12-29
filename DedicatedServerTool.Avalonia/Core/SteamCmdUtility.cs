using SteamCmdFluentApi;
using System.Threading.Tasks;

namespace DedicatedServerTool.Avalonia.Core;
internal static class SteamCmdUtility
{
    private static readonly int DedicatedServerAppId = 950900;
    private static readonly SteamCmd SteamCmd = new("steamcmd.exe");

    public static Task<(bool IsSuccessful, string Output)> DownloadOrUpdateDedicatedServerAsync(string installDirectory)
    {
        return Task.Run<(bool, string)>(() =>
        {
            var isSuccessful = SteamCmd.CreateCommand()
             .WithAnonymousAuthentication()
             .InstallOrUpdateApp(DedicatedServerAppId)
             .WithWorkingDirectory(installDirectory)
             .BuildAndTryToExecute(out var output);
            return (isSuccessful, output);
        });
    }
}
