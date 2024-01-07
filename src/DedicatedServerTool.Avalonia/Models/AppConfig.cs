using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace DedicatedServerTool.Avalonia.Models;
public class AppConfig : ObservableObject
{
    public ObservableCollection<FactionDefinition> Factions { get; init; } = new(Enumerable.Empty<FactionDefinition>());
    public ObservableCollection<MapDefinition> Maps { get; init; } = new(Enumerable.Empty<MapDefinition>());
}
