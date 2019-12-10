using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CANableSharp;
using CANViewer.Models;
using CANViewer.ViewModels;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace CANViewerAvalonia
{
    public class MainWindow : Window
    {

        private readonly MainWindowViewModel viewModel;

        public MainWindow()
        {
            viewModel = new MainWindowViewModel(this);
            DataContext = viewModel;
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        public void Initialize(CANable device)
        {
            viewModel.Initialize(device);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
