using Avalonia.Controls;
using Avalonia.Interactivity;
using DedicatedServerTool.Avalonia.ViewModels;

namespace DedicatedServerTool.Avalonia.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is AppViewModel viewModel)
        {
            viewModel.TopLevel = GetTopLevel(this);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (DataContext is AppViewModel viewModel)
        {
            viewModel.SaveAppStateCommand.Execute(null);
        }
    }
}