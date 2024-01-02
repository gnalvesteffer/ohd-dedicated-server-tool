using System;
using System.Diagnostics;

namespace DedicatedServerTool.Avalonia.Core;
internal static class UrlUtility
{
    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening URL: " + ex.Message);
        }
    }
}
