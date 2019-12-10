using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CANViewerAvalonia.ViewModels;

namespace CANViewerAvalonia
{
    public class DeviceSelection : Window
    {
        public DeviceSelection()
        {
            DataContext = new DeviceSelectionViewModel(this);
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
