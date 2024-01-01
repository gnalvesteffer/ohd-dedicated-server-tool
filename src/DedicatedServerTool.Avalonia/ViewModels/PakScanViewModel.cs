using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DedicatedServerTool.Avalonia.Models;
using DedicatedServerTool.Avalonia.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DedicatedServerTool.Avalonia.ViewModels
{
    public class PakScanViewModel : ObservableObject
    {
        private const string ModDirectory = "HarshDoorstop/Mods";
        private const string VanillaDirectory = "HarshDoorstop/Content/Paks";
        private readonly ServerProfile _profile;
        private CancellationTokenSource _cancellationTokenSource = new();

        private ObservableCollection<PakScanResultViewModel> _results = new();
        public ObservableCollection<PakScanResultViewModel> Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }

        private int _totalPakFiles = new();
        public int TotalPakFiles
        {
            get => _totalPakFiles;
            set => SetProperty(ref _totalPakFiles, value);
        }

        private int _totalProcessedPakFiles = new();
        public int TotalProcessedPakFiles
        {
            get => _totalProcessedPakFiles;
            set => SetProperty(ref _totalProcessedPakFiles, value);
        }

        private bool _isLoading = new();
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand FindModMapsCommand { get; }
        public ICommand FindVanillaMapsCommand { get; }
        public ICommand FindModGameModesCommand { get; }
        public ICommand FindVanillaGameModesCommand { get; }
        public ICommand FindModFactionsCommand { get; }
        public ICommand FindVanillaFactionsCommand { get; }
        public ICommand CancelScanCommand { get; }

        public PakScanViewModel(ServerProfile profile)
        {
            _profile = profile;
            FindModMapsCommand = new AsyncRelayCommand(FindModMapsAsync);
            FindVanillaMapsCommand = new AsyncRelayCommand(FindVanillaMapsAsync);
            FindModGameModesCommand = new AsyncRelayCommand(FindModGameModesAsync);
            FindVanillaGameModesCommand = new AsyncRelayCommand(FindVanillaGameModesAsync);
            FindModFactionsCommand = new AsyncRelayCommand(FindModFactionsAsync);
            FindVanillaFactionsCommand = new AsyncRelayCommand(FindVanillaFactionsAsync);
            CancelScanCommand = new RelayCommand(CancelScan);
        }

        private void CancelScan()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new();
            TotalPakFiles = 0;
            TotalProcessedPakFiles = 0;
            IsLoading = false;
        }

        private Task FindModMapsAsync()
        {
            return FindMapsAsync(ModDirectory);
        }

        private Task FindVanillaMapsAsync()
        {
            return FindMapsAsync(VanillaDirectory);
        }

        private Task FindModGameModesAsync()
        {
            return FindGameModesAsync(ModDirectory);
        }

        private Task FindVanillaGameModesAsync()
        {
            return FindGameModesAsync(VanillaDirectory);
        }

        private Task FindModFactionsAsync()
        {
            return FindFactionsAsync(ModDirectory);
        }

        private Task FindVanillaFactionsAsync()
        {
            return FindFactionsAsync(VanillaDirectory);
        }

        private async Task FindMapsAsync(string directoryPath)
        {
            try
            {
                CancelScan();
                IsLoading = true;
                var pakFilePaths = Directory.GetFiles(Path.Combine(_profile.InstallDirectory, directoryPath), "*.pak", SearchOption.AllDirectories);
                TotalPakFiles = pakFilePaths.Count();
                TotalProcessedPakFiles = 0;
                Results.Clear();

                await PakScanUtility.FindMapNamesInPaksAsync(
                    pakFilePaths,
                    (pakFilePath, mapNames) =>
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                        ++TotalProcessedPakFiles;
                        foreach (var mapName in mapNames)
                        {
                            Results.Add(new(Path.GetFileName(pakFilePath), mapName));
                        }
                    },
                    _cancellationTokenSource.Token
                );
            }
            catch (Exception exception)
            {
                new MessageBoxWindow("Error", exception.Message);
            }
        }

        private async Task FindGameModesAsync(string directoryPath)
        {
            try
            {
                CancelScan();
                IsLoading = true;
                var pakFilePaths = Directory.GetFiles(Path.Combine(_profile.InstallDirectory, directoryPath), "*.pak", SearchOption.AllDirectories);
                TotalPakFiles = pakFilePaths.Count();
                TotalProcessedPakFiles = 0;
                Results.Clear();

                await PakScanUtility.FindGameModeNamesInPaksAsync(
                    pakFilePaths,
                    (pakFilePath, mapNames) =>
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                        ++TotalProcessedPakFiles;
                        foreach (var mapName in mapNames)
                        {
                            Results.Add(new(Path.GetFileName(pakFilePath), mapName));
                        }
                    },
                    _cancellationTokenSource.Token
                );
            }
            catch (Exception exception)
            {
                new MessageBoxWindow("Error", exception.Message);
            }
        }

        private async Task FindFactionsAsync(string directoryPath)
        {
            try
            {
                CancelScan();
                IsLoading = true;
                var pakFilePaths = Directory.GetFiles(Path.Combine(_profile.InstallDirectory, directoryPath), "*.pak", SearchOption.AllDirectories);
                TotalPakFiles = pakFilePaths.Count();
                TotalProcessedPakFiles = 0;
                Results.Clear();

                await PakScanUtility.FindFactionNamesInPaksAsync(
                    pakFilePaths,
                    (pakFilePath, mapNames) =>
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                        ++TotalProcessedPakFiles;
                        foreach (var mapName in mapNames)
                        {
                            Results.Add(new(Path.GetFileName(pakFilePath), mapName));
                        }
                    },
                    _cancellationTokenSource.Token
                );
            }
            catch (Exception exception)
            {
                new MessageBoxWindow("Error", exception.Message);
            }
        }
    }
}