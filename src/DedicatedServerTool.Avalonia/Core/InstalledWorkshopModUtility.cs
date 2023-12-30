using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DedicatedServerTool.Avalonia.Core
{
    internal static class InstalledWorkshopModUtility
    {
        internal static string GetModName(string serverInstallDirectory, long workshopId)
        {
            var workshopItemDirectory = Path.Combine(serverInstallDirectory, @$"steamapps\workshop\content\736590\{workshopId}");
            if (Directory.Exists(workshopItemDirectory))
            {
                var modFolderName = Directory.GetDirectories(workshopItemDirectory).FirstOrDefault();
                return Path.GetFileName(modFolderName) ?? string.Empty;
            }
            return string.Empty;
        }

        internal static IEnumerable<long> GetInstalledWorkshopIds(string serverInstallDirectory)
        {
            var workshopDirectory = Path.Combine(serverInstallDirectory, @"steamapps\workshop\content\736590\");
            if (!Directory.Exists(workshopDirectory))
            {
                return Enumerable.Empty<long>();
            }

            var workshopItemDirectories = Directory.GetDirectories(workshopDirectory)
                .Where(directory => Directory.GetDirectories(directory).Any());
            var workshopIds = new List<long>();
            foreach (var directory in workshopItemDirectories)
            {
                var workshopItemFolderName = Path.GetFileName(directory); // the folder name should just be the workshop item id
                if (long.TryParse(workshopItemFolderName, out var workshopItemId))
                {
                    workshopIds.Add(workshopItemId);
                }
            }
            return workshopIds;
        }
    }
}
