using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FRC_CAN_Viewer
{
    public class ArbCalc : Window
    {
        public ArbCalc()
        {
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
