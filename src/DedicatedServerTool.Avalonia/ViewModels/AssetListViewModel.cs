using DedicatedServerTool.Avalonia.Core;
using DedicatedServerTool.Avalonia.Models;
using System.Collections.Generic;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class AssetListViewModel
{
    public IEnumerable<MapDefinition> Maps { get; }
    public IEnumerable<FactionDefinition> Factions { get; }

    public AssetListViewModel()
    {
        var appConfig = AppConfigRepository.Load();
        Maps = appConfig.Maps;
        Factions = appConfig.Factions;
    }
}
