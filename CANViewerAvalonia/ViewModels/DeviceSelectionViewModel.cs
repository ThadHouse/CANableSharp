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
                selectedValue = value;
                RaisePropertyChanged();
            }
        }

        public DeviceSelectionViewModel()
        {
            if (Design.IsDesignMode) return;
            Refresh();
        }

        public void Select()
        {

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
                SelectedValue = null;
            }
        }
    }
}
