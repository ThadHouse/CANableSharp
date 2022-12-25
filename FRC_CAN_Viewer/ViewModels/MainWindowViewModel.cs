using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using CANableSharp;
using CANViewer.Models;
using FRC_CAN_Viewer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CANViewer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CANable> CANDevices { get; } = new ObservableCollection<CANable>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private CANable selectedValue;

        public CANable SelectedValue
        {
            get => selectedValue;
            set
            {
                UpdateDevice(value, selectedValue);
                selectedValue = value;
                RaisePropertyChanged();
            }
        }

        public void Refresh()
        {
            SelectedValue = null;
            foreach (var d in CANDevices)
            {
                d.Dispose();
            }

            CANDevices.Clear();
            var devices = CANable.EnumerateDevices();
            foreach (var device in devices)
            {
                CANDevices.Add(device);
            }

            if (devices.Count > 0)
            {
                SelectedValue = devices[0];
            }
            else
            {
                SelectedValue = null;
            }
        }

        private void UpdateDevice(CANable newValue, CANable oldValue)
        {
            if (oldValue != null)
            {
                CloseDevice();
            }
            if (newValue == null)
            {
                CANMessages.Clear();
                return;
            }
            Initialize(newValue);
        }

        private void CloseDevice()
        {
            keepGoing = false;
            readThread?.Join();

            channel.Stop();
            selectedValue?.Close();
        }

        public ObservableCollection<CANMessage> CANMessages { get; } = new ObservableCollection<CANMessage>();

        private MainWindow window;
        private volatile bool keepGoing;
        private Thread readThread;
        private Channel channel;

        public MainWindowViewModel(MainWindow window)
        {
            this.window = window;
        }

        public void SetupGeneration()
        {
            var grid = window.Get<DataGrid>("DataGrid");
            grid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            window.Closing += Window_Closing;
            Refresh();
        }

        private void Initialize(CANable device)
        {
            CANMessages.Clear();

            if (Design.IsDesignMode) return;

            // Device will not be opened
            device.Open();
            channel = device.GetChannels()[0];
            try {
                channel.Bitrate = 1_000_000;
            } catch {
                // Ignore, the Spark Max can't take this flag
            }
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
                if (selectedValue.ReadFrame(out var frame, TimeSpan.FromSeconds(1)))
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
                else
                {
                    ;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Design.IsDesignMode) return;

            CloseDevice();

            foreach (var device in CANDevices)
            {
                device.Dispose();
            }

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
