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

        private readonly MainWindowViewModel viewModel = new MainWindowViewModel();

        public unsafe MainWindow()
        {
            DataContext = viewModel;
            InitializeComponent();

            var grid = this.Get<DataGrid>("DataGrid");
            grid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
#if DEBUG
            this.AttachDevTools();
#endif
            if (Design.IsDesignMode) return;

            CandleInvoke.Initialize();
            // Initialize CANable
            var device = CANable.EnumerateDevices()[0];
            device.Open();
            var channel = device.GetChannels()[0];
            channel.Bitrate = 1_000_000;
            //channel.Bitrate = 250_000;
            channel.Start();

            Thread readThread = new Thread(() =>
            {
                while (true)
                {
                    if (device.ReadFrame(out var frame, Timeout.InfiniteTimeSpan))
                    {
                        ulong data = frame.DataLong;
                        var dlc = frame.DLC;
                        var id = frame.RawId;
                        var ts = frame.TimestampUs;

                        // Valid
                        Action toInvoke = () =>
                        {
                            ulong store = data;

                            Span<byte> sData = MemoryMarshal.Cast<ulong, byte>(new Span<ulong>(&store, 1)).Slice(0, (int)dlc);

                            var msg = viewModel.CANMessages.Where(x => x.rawId == id).FirstOrDefault();
                            if (msg != null)
                            {
                                // Update message
                                msg.UpdateData(sData, ts);

                            }
                            else
                            {
                                viewModel.CANMessages.Add(new CANViewer.Models.CANMessage(id, sData, ts));
                            }
                        };

                        Dispatcher.UIThread.InvokeAsync(toInvoke);
                    }
                }
            });
            readThread.IsBackground = true;
            readThread.Name = "CANReadThread";
            readThread.Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Id" || e.PropertyName == "TimeStamp")
            {
                var textColumn = e.Column as DataGridTextColumn;
                var b = textColumn.Binding as Binding;
                b.Converter = HexidecimalConverter.Singleton;
                //e.Column.
            }
            else if (e.PropertyName == "ApiId")
            {
                var textColumn = e.Column as DataGridTextColumn;
                var b = textColumn.Binding as Binding;
                b.Converter = ApiHexidecimalConverter.Singleton;
            }
            else if (e.PropertyName == "Data")
            {
                var textColumn = e.Column as DataGridTextColumn;
                var b = textColumn.Binding as Binding;
                b.Converter = MemoryConverter.Singleton;
            }

            ;
        }
    }
}
