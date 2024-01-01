using Avalonia.Controls;
using DedicatedServerTool.Avalonia.Models;
using DedicatedServerTool.Avalonia.ViewModels;

namespace DedicatedServerTool.Avalonia.Views
{
    public partial class PakScanWindow : Window
    {
        public PakScanWindow(ServerProfile serverProfile)
        {
            InitializeComponent();
            DataContext = new PakScanViewModel(serverProfile);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            if (DataContext is PakScanViewModel viewModel)
            {
                viewModel.CancelScanCommand.Execute(null);
            }
        }
    }
}
