using CANableSharp;
using System;

namespace CANableTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CandleInvoke.Initialize();
            var devices = CANable.EnumerateDevices();
            foreach (var d in devices)
            {
                Console.WriteLine(d.Name);
            }
            // Dispose all but the first one
            if (devices.Count == 0)
            {
                Console.WriteLine("No Devices Found");
                return;
            }

            Console.WriteLine(devices[0].Descriptor);

            devices[0].Open();
            var channels = devices[0].GetChannels();
            ;
        }
    }
}
