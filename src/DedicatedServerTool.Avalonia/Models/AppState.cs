using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DedicatedServerTool.Avalonia.Models;
public class AppState : ObservableObject
{
    public ObservableCollection<ServerProfile> ServerProfiles { get; set; } = new();
}
