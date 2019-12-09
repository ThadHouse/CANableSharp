using System;
using System.Collections.Generic;
using System.Text;

using static CANableSharp.CandleInvoke;

namespace CANableSharp
{
    public readonly struct Channel
    {
        private readonly IntPtr device;
        private readonly byte channelNum;

        internal Channel(IntPtr device, byte channelNum)
        {
            this.device = device;
            this.channelNum = channelNum;
        }

        public unsafe void WriteExtendedFrame(Span<byte> data, uint id, bool rtr = false)
        {
            if (data.Length > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Must be 8 bytes or less");
            }
            Frame frame;
            id &= 0x1FFFFFFF;
            frame.CanDLC = (byte)data.Length;
            frame.CanId = id | (uint)IdMasks.Extended;
            frame.CanId |= (rtr ? (uint)IdMasks.RTR : 0);
            if (candle_frame_send(device, channelNum, &frame) == 0)
            {
                throw new CANableException("Error Sending Frame", candle_dev_last_error(device));
            }
        }

        public unsafe void WriteStandardFrame(Span<byte> data, uint id, bool rtr = false)
        {
            if (data.Length > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Must be 8 bytes or less");
            }
            Frame frame;
            id &= 0x7FF;
            frame.CanDLC = (byte)data.Length;
            frame.CanId = id;
            frame.CanId |= (rtr ? (uint)IdMasks.RTR : 0);
            if (candle_frame_send(device, channelNum, &frame) == 0)
            {
                throw new CANableException("Error Sending Frame", candle_dev_last_error(device));
            }
        }

        public void Start()
        {
            if (candle_channel_start(device, channelNum, 0) == 0)
            {
                throw new CANableException("Error Starting Channel", candle_dev_last_error(device));
            }
        }

        public void Stop()
        {
            candle_channel_stop(device, channelNum);
        }

        public uint Bitrate
        {
            set
            {
                if (candle_channel_set_bitrate(device, channelNum, value) == 0)
                {
                    throw new CANableException("Error Setting Bitrate", candle_dev_last_error(device));
                }
            }
        }

        public unsafe Bittiming Timing
        {
            set
            {
                if (candle_channel_set_timing(device, channelNum, &value) == 0)
                {
                    throw new CANableException("Error Setting Timing", candle_dev_last_error(device));
                }
            }
        }

        public unsafe Capabilities Capabilities
        {
            get
            {
                Capabilities cb;
                if (candle_channel_get_capabilities(device, channelNum, &cb) == 0)
                {
                    throw new CANableException("Error Getting Capabilities", candle_dev_last_error(device));
                }
                return cb;
            }
        }
    }
}
