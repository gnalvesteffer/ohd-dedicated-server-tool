using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;

namespace DedicatedServerTool.Avalonia.ViewModels;
public class MessageBoxViewModel : ObservableObject
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _message = string.Empty;

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand CloseCommand { get; }

    public MessageBoxViewModel()
    {
        CloseCommand = new RelayCommand(() => { });
    }

    public MessageBoxViewModel(string title, string message, Action onClose)
    {
        Title = title;
        Message = message;
        CloseCommand = new RelayCommand(onClose);
    }
}
