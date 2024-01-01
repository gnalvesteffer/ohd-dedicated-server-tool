using System;
using System.Collections.Concurrent;
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
        return FindAssetNamesInPaksAsync(pakFilePaths, @"(?<=\/)([^\/]*[^\/]+)(?=\.umap)", pakFileProcessedHandler, cancellationToken);
    }

    public static Task FindGameModeNamesInPaksAsync(IEnumerable<string> pakFilePaths, OnPakFileProcessed? pakFileProcessedHandler = null, CancellationToken cancellationToken = default)
    {
        return FindAssetNamesInPaksAsync(pakFilePaths, @"(?<=\/)([^\/]+Game[^\/]+)(?=\.uasset)", pakFileProcessedHandler, cancellationToken);
    }

    public static Task FindFactionNamesInPaksAsync(IEnumerable<string> pakFilePaths, OnPakFileProcessed? pakFileProcessedHandler = null, CancellationToken cancellationToken = default)
    {
        return FindAssetNamesInPaksAsync(pakFilePaths, @"(?<=\/)([^\/]+Faction[^\/]+)(?=\.uasset)", pakFileProcessedHandler, cancellationToken);
    }

    private static Task FindAssetNamesInPaksAsync(IEnumerable<string> pakFilePaths, string regexPattern, OnPakFileProcessed? pakFileProcessedHandler = null, CancellationToken cancellationToken = default)
    {
        var tasks = pakFilePaths.Select(async pakFilePath =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var assetNames = await FindAssetNamesInPakAsync(pakFilePath, regexPattern, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            pakFileProcessedHandler?.Invoke(pakFilePath, assetNames);
        });

        return Task.WhenAll(tasks);
    }

    private static async Task<IEnumerable<string>> FindAssetNamesInPakAsync(string pakFilePath, string regexPattern, CancellationToken cancellationToken = default)
    {
        try
        {
            const int bufferSize = 1024 * 1024;
            var assetNames = new ConcurrentBag<string>();
            var buffer = new char[bufferSize];

            using (var fileStream = new FileStream(pakFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: bufferSize, useAsync: true))
            using (var streamReader = new StreamReader(fileStream))
            {
                var tasks = new List<Task>();

                while (!streamReader.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var bytesRead = await streamReader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    var currentText = new string(buffer, 0, bytesRead);

                    tasks.Add(ProcessChunkAsync(currentText, regexPattern, assetNames, cancellationToken));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            return assetNames.Select(name => SanitizeResult(name));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return Enumerable.Empty<string>();
        }
    }

    private static Task ProcessChunkAsync(string chunk, string regexPattern, ConcurrentBag<string> assetNames, CancellationToken cancellationToken)
    {
        var matches = Regex.Matches(chunk, regexPattern);

        return Task.WhenAll(matches.Cast<Match>().Select(async match =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var assetName = match.Value;
            assetNames.Add(assetName);
        }));
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
