using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using CANableSharp;
using CANViewer.Models;
using CANViewerAvalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CANViewer.ViewModels
{
    public class MainWindowViewModel
    {
        public ObservableCollection<CANMessage> CANMessages { get; } = new ObservableCollection<CANMessage>();

        private MainWindow window;
        private volatile bool keepGoing;
        private Thread readThread;
        private CANable device;
        private Channel channel;

        public MainWindowViewModel(MainWindow window)
        {
            this.window = window;
        }

        public void Initialize(CANable device)
        {
            this.device = device;
            var grid = window.Get<DataGrid>("DataGrid");
            grid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            window.Closing += Window_Closing;

            if (Design.IsDesignMode) return;

            // Device will not be opened
            device.Open();
            channel = device.GetChannels()[0];
            channel.Bitrate = 1_000_000;
            channel.Start();

            readThread = new Thread(ReadThreadMain);
            readThread.Name = "CANReadThread";
            keepGoing = true;
            readThread.Start();
        }

        private unsafe void ReadThreadMain()
        {
            while (keepGoing)
            {
                if (device.ReadFrame(out var frame, TimeSpan.FromSeconds(1)))
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

                        var msg = CANMessages.Where(x => x.rawId == id).FirstOrDefault();
                        if (msg != null)
                        {
                                // Update message
                                msg.UpdateData(sData, ts);

                        }
                        else
                        {
                            CANMessages.Add(new CANViewer.Models.CANMessage(id, sData, ts));
                        }
                    };

                    Dispatcher.UIThread.InvokeAsync(toInvoke);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Design.IsDesignMode) return;

            keepGoing = false;
            readThread.Join();

            channel.Stop();
            device.Dispose();
            
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
