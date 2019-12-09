using CANViewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CANViewer.ViewModels
{
    public class MainWindowViewModel
    {
        public ObservableCollection<CANMessage> CANMessages { get; } = new ObservableCollection<CANMessage>();
    }
}
