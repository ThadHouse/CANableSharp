using Avalonia.Controls;
using CANableSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CANViewerAvalonia.ViewModels
{
    public class DeviceSelectionViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CANable> CANDevices { get; } = new ObservableCollection<CANable>();

        private bool allowNullValue = false;
        private CANable selectedValue;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CANable SelectedValue
        {
            get => selectedValue;
            set
            {
                if (value == null && !allowNullValue) return;
                selectedValue = value;
                RaisePropertyChanged();
            }
        }

        private DeviceSelection window;

        public DeviceSelectionViewModel(DeviceSelection window)
        {
            this.window = window;
            if (Design.IsDesignMode) return;
            window.Initialized += (o, e) =>
            {
                CandleInvoke.Initialize();
                Refresh();
            };
            
        }

        public async void Select()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Initialize(SelectedValue);
            window.Hide();
            object keepGoing = await mainWindow.ShowDialog<object>(window);
            if (keepGoing == null) window.Close();
        }

        public void Refresh()
        {
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
                allowNullValue = true;
                SelectedValue = null;
                allowNullValue = false;
            }
        }
    }
}
