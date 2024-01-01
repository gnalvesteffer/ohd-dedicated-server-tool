using Avalonia.Controls;
using DedicatedServerTool.Avalonia.ViewModels;

namespace DedicatedServerTool.Avalonia.Views;

public partial class MessageBoxWindow : Window
{
    public MessageBoxWindow(string title, string message)
    {
        InitializeComponent();
        DataContext = new MessageBoxViewModel(title, message, Close);
    }
}