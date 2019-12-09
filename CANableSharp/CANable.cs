using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static CANableSharp.CandleInvoke;

namespace CANableSharp
{
    public unsafe class CANable : IDisposable
    {
        public static List<CANable> EnumerateDevices()
        {
            
            IntPtr list;
            if (candle_list_scan(&list) == 0)
            {
                var err = candle_list_last_error(list);
                throw new EnumerationException($"CANable enumeration failed: {err}", err);
            }
            
            byte len;
            candle_list_length(list, &len);

            List<CANable> devices = new List<CANable>(len);

            for (int i = 0; i < len; i++)
            {
                IntPtr hdev;
                if (candle_dev_get(list, (byte)i, &hdev) != 0)
                {
                    devices.Add(new CANable(hdev));
                }
            }

            candle_list_free(list);

            return devices;
        }

        readonly IntPtr handle;

        private CANable(IntPtr id)
        {
            this.handle = id;
        }

        public string Descriptor
        {
            get
            {
                return Marshal.PtrToStringUni((IntPtr)candle_dev_get_path(handle));
            }
        }

        public DeviceState DeviceState
        {
            get
            {
                DeviceState state;
                if (candle_dev_get_state(handle, &state) == 0)
                {
                    throw new CANableException("Error Reading Device State", candle_dev_last_error(handle));
                }
                return state;
            }
        }

        private List<Channel>? channels;

        public List<Channel> GetChannels()
        {
            if (channels != null) return channels;

            byte numChannels = 0;
            candle_channel_count(handle, &numChannels);

            channels = new List<Channel>();

            for (int i = 0; i < numChannels; i++)
            {
                channels.Add(new Channel(handle, (byte)i));
            }

            return channels;
        }

        public void Open()
        {
            if (candle_dev_open(handle) == 0)
            {
                throw new CANableException("Failed to open device", candle_dev_last_error(handle));
            }
        }

        public TimeSpan CurrentTime
        {
            get
            {
                uint us;
                if (candle_dev_get_timestamp_us(handle, &us) == 0)
                {
                    throw new CANableException("Failed to get time", candle_dev_last_error(handle));
                }
                return TimeSpan.FromMilliseconds(us / 1000.0);
            }
        }

        public bool ReadFrame(out CANFrame frame, TimeSpan timeout)
        {
            Frame lowLevelFrame;
            uint timeoutMs = (uint)timeout.TotalMilliseconds;
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                timeoutMs = uint.MaxValue;
            }
            
            if (candle_frame_read(handle, &lowLevelFrame, timeoutMs) == 0)
            {
                var err = candle_dev_last_error(handle);
                if (err == Error.ReadTimeout)
                {
                    frame = default;
                    return false;
                }
                throw new CANableException("Failed to read frame", err);
            }

            frame = new CANFrame(&lowLevelFrame);
            return true;
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                candle_dev_close(handle);
                candle_dev_free(handle);

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~CANable()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
