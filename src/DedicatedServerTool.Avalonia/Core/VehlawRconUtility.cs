using DedicatedServerTool.Avalonia.Views;
using System;
using System.Diagnostics;

namespace DedicatedServerTool.Avalonia.Core;
internal static class VehlawRconUtility
{
    private static string ExecutablePath = "VehlawRcon.exe";

    internal static Process Start(string ipAddress, ushort port, string password)
    {
        try
        {
            // Specify the executable file path

            // Specify the command-line arguments
            string arguments = $"{ipAddress} {port} {password}";

            // Create the process
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ExecutablePath,
                    Arguments = arguments,
                    UseShellExecute = true,
                }
            };

            // Start the process
            process.Start();

            return process;
        }
        catch (Exception ex)
        {
            MessageBoxWindow.Show("Error", $"Failed to start RCON: {ex.Message}");
        }

        return null;
    }
}
