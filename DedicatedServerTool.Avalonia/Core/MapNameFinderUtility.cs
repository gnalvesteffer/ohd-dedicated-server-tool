using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DedicatedServerTool.Avalonia.Core;
internal static class MapNameFinderUtility
{
    public delegate void OnPakFileProcessed(string pakFilePath, IEnumerable<string> mapNames);

    public static async Task<IEnumerable<string>> FindMapNamesInPaksAsync(IEnumerable<string> pakFilePaths, OnPakFileProcessed? pakFileProcessedHandler = null)
    {
        var tasks = pakFilePaths.Select(async pakFilePath =>
        {
            var mapNames = await FindMapNamesInPakAsync(pakFilePath);
            pakFileProcessedHandler?.Invoke(pakFilePath, mapNames);
            return mapNames;
        });

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(x => x);
    }


    private static async Task<IEnumerable<string>> FindMapNamesInPakAsync(string pakFilePath)
    {
        try
        {
            var mapNames = new List<string>();
            var buffer = new char[4096]; // Adjust the buffer size as needed
            var remainingText = string.Empty;

            using (var fileStream = new FileStream(pakFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (var streamReader = new StreamReader(fileStream))
            {
                while (!streamReader.EndOfStream)
                {
                    var bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length);
                    var currentText = remainingText + new string(buffer, 0, bytesRead);

                    // Use a regular expression to find occurrences of ".umap"
                    var pattern = @"\.umap";
                    foreach (Match match in Regex.Matches(currentText, pattern))
                    {
                        // Walk backward to find the first slash
                        var startIndex = match.Index;
                        while (startIndex >= 0 && currentText[startIndex] != '/' && currentText[startIndex] != '\\')
                        {
                            startIndex--;
                        }

                        // Extract the map name
                        if (startIndex >= 0)
                        {
                            var mapName = currentText.Substring(startIndex + 1, match.Index - startIndex - 1);
                            mapNames.Add(mapName);
                        }
                    }

                    // Save the remaining text after the last match for the next iteration
                    var lastMatchEnd = currentText.LastIndexOfAny(new[] { '/', '\\' }) + 1;
                    remainingText = lastMatchEnd >= 0 ? currentText.Substring(lastMatchEnd) : currentText;
                }
            }

            return mapNames;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return Enumerable.Empty<string>(); // Return an empty enumerable in case of an error
        }
    }
}
