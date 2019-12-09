using CANableSharp;
using CANViewer.Models;
using CANViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CANViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel = new MainWindowViewModel();
        public unsafe MainWindow()
        {
            DataContext = viewModel;
            InitializeComponent();

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
                                viewModel.CANMessages.Add(new Models.CANMessage(id, sData, ts));
                            }
                        };

                        Dispatcher.BeginInvoke(toInvoke);
                    }
                }
            });
            readThread.IsBackground = true;
            readThread.Name = "CANReadThread";
            readThread.Start();
            
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
