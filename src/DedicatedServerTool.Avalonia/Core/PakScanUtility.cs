using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

internal static class PakScanUtility
{
    public delegate void OnPakFileProcessed(string pakFilePath, IEnumerable<string> results);

    public static Task FindMapNamesInPaksAsync(IEnumerable<string> pakFilePaths, OnPakFileProcessed? pakFileProcessedHandler = null, CancellationToken cancellationToken = default)
    {
        var tasks = pakFilePaths.Select(async pakFilePath =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var mapNames = await FindMapNamesInPakAsync(pakFilePath, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            pakFileProcessedHandler?.Invoke(pakFilePath, mapNames);
        });

        return Task.WhenAll(tasks);
    }

    private static async Task<IEnumerable<string>> FindMapNamesInPakAsync(string pakFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var mapNames = new List<string>();
            var buffer = new char[4096]; // Adjust the buffer size as needed

            using (var fileStream = new FileStream(pakFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (var streamReader = new StreamReader(fileStream))
            {
                var tasks = new List<Task>();

                while (!streamReader.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length);
                    var currentText = new string(buffer, 0, bytesRead);

                    tasks.Add(ProcessChunkAsync(currentText, mapNames, cancellationToken));
                    await Task.Delay(10);
                }

                await Task.WhenAll(tasks);
            }

            return mapNames.Select(name => SanitizeResult(name));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return Enumerable.Empty<string>(); // Return an empty enumerable in case of an error
        }
    }

    private static Task ProcessChunkAsync(string chunk, List<string> mapNames, CancellationToken cancellationToken)
    {
        // Use a regular expression to find occurrences of ".umap"
        var pattern = @"\.umap";
        var matches = Regex.Matches(chunk, pattern);

        // Parallelize the processing of matches
        return Task.WhenAll(matches.Cast<Match>().Select(match => Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Walk backward to find the first slash
            var startIndex = match.Index;
            while (startIndex >= 0 && chunk[startIndex] != '/' && chunk[startIndex] != '\\')
            {
                startIndex--;
            }

            // Extract the map name
            if (startIndex >= 0)
            {
                var mapName = chunk.AsMemory(startIndex + 1, match.Index - startIndex - 1);
                lock (mapNames)
                {
                    mapNames.Add(mapName.ToString());
                }
            }
        }, cancellationToken)));
    }

    private static string SanitizeResult(string input)
    {
        int nullIndex = input.LastIndexOf('\0');
        if (nullIndex != -1)
        {
            return input.Substring(nullIndex + 1); // Start from the character after the null terminator
        }
        else
        {
            // If no null terminator found, return the entire string
            return input;
        }
    }
}
