using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CANableSharp
{
    public unsafe class CANable : IDisposable
    {
        private bool opened = false;

        public static List<CANable> EnumerateDevices()
        {
            var api = CandleInvoke.CandleAPI;
            IntPtr list;
            if (api.candle_list_scan(&list) == 0)
            {
                var err = api.candle_list_last_error(list);
                throw new EnumerationException($"CANable enumeration failed: {err}", err);
            }
            
            byte len;
            api.candle_list_length(list, &len);

            List<CANable> devices = new List<CANable>(len);

            for (int i = 0; i < len; i++)
            {
                IntPtr hdev;
                if (api.candle_dev_get(list, (byte)i, &hdev) != 0)
                {
                    devices.Add(new CANable(hdev, api));
                }
            }

            api.candle_list_free(list);

            return devices;
        }

        private readonly IntPtr handle;
        private readonly ICandleAPI api;

        private CANable(IntPtr id, ICandleAPI api)
        {
            this.handle = id;
            this.api = api;
        }

        public string Descriptor
        {
            get
            {
                return Marshal.PtrToStringUni((IntPtr)api.candle_dev_get_path(handle));
            }
        }

        public string Name
        {
            get
            {
                return Marshal.PtrToStringUni((IntPtr)api.candle_dev_get_name(handle));
            }
        }

        public CandleInvoke.DeviceState DeviceState
        {
            get
            {
                CandleInvoke.DeviceState state;
                if (api.candle_dev_get_state(handle, &state) == 0)
                {
                    throw new CANableException("Error Reading Device State", api.candle_dev_last_error(handle));
                }
                return state;
            }
        }

        private List<Channel>? channels;

        public List<Channel> GetChannels()
        {
            if (channels != null) return channels;

            byte numChannels = 0;
            api.candle_channel_count(handle, &numChannels);

            channels = new List<Channel>();

            for (int i = 0; i < numChannels; i++)
            {
                channels.Add(new Channel(handle, (byte)i, api));
            }

            return channels;
        }

        public void Open()
        {
            if (api.candle_dev_open(handle) == 0)
            {
                throw new CANableException("Failed to open device", api.candle_dev_last_error(handle));
            }
            opened = true;
        }

        public void Close()
        {
            if (!opened) return;
            api.candle_dev_close(handle);
            opened = false;
        }

        public TimeSpan CurrentTime
        {
            get
            {
                uint us;
                if (api.candle_dev_get_timestamp_us(handle, &us) == 0)
                {
                    throw new CANableException("Failed to get time", api.candle_dev_last_error(handle));
                }
                return TimeSpan.FromMilliseconds(us / 1000.0);
            }
        }

        public bool ReadFrame(out CANFrame frame, TimeSpan timeout)
        {
            CandleInvoke.Frame lowLevelFrame;
            uint timeoutMs = (uint)timeout.TotalMilliseconds;
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                timeoutMs = uint.MaxValue;
            }
            
            if (api.candle_frame_read(handle, &lowLevelFrame, timeoutMs) == 0)
            {
                var err = api.candle_dev_last_error(handle);
                if (err == CandleInvoke.Error.ReadTimeout)
                {
                    frame = default;
                    return false;
                }
                throw new CANableException("Failed to read frame", err);
            }

            frame = new CANFrame(&lowLevelFrame, api);
            return true;
        }

        public override string ToString()
        {
            return Name;
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

                if (opened)
                {
                    api.candle_dev_close(handle);
                }
                api.candle_dev_free(handle);

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
