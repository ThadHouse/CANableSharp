using FRC.NativeLibraryUtilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable IDE1006 // Naming Styles

namespace CANableSharp
{
    public static unsafe class CandleInvoke
    {
        public const string LibName = @"C:\Users\thadh\Documents\GitHub\thadhouse\CANableSharp\CANableSharp\x64\Debug\CANableDLL.DLL";

        public enum DeviceState
        {
            Available,
            InUse
        }

        public enum FrameType
        {
            Unknown,
            Receive,
            Echo,
            Error,
            TimestampOverflow
        }

        public enum IdMasks : uint
        {
            Extended = 0x80000000,
            RTR = 0x40000000,
            ERR = 0x20000000
        }

        [Flags]
        public enum Mode : uint
        {
            Normal = 0x00,
            ListenOnly = 0x01,
            LoopBack = 0x02,
            TripleSample = 0x04,
            OneShot = 0x08,
            HWTimestamp = 0x10
        }

        public enum Error
        {
            OK,
            CreateFile,
            WinUSBInitialize,
            QueryInterface,
            QueryPipe,
            ParseIfDescriptor,
            SetHostFormat,
            GetDeviceInfo,
            GetBittimingConst,
            PrepareRead,
            SetDeviceMode,
            SetBittiming,
            SetBitrateFCLK,
            SetBitrateUnsupported,
            SendFrame,
            ReadTimeout,
            ReadWait,
            ReadResult,
            ReadSize,
            SetupDiIfDetails,
            SetupDiIfDetails2,
            Malloc,
            PathLen,
            CLSID,
            GetDevices,
            SetupDiIfEnum,
            SetTimestampMode,
            DeviceOutOfRange,
            GetTimestamp,
            SetPipeRawIO
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Frame
        {
            public uint EchoId;
            public uint CanId;
            public byte CanDLC;
            public byte Channel;
            public byte Flags;
            public byte Reserved;
            public ulong Data;
            public uint TimestampUs;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Capabilities
        {
            public uint Feature;
            public uint FCLKCan;
            public uint Tseg1Min;
            public uint Tseg1Max;
            public uint Tseg2Min;
            public uint Tseg2Max;
            public uint SJWMax;
            public uint BRPMin;
            public uint BRPMax;
            public uint BRPInc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Bittiming
        {
            public uint PropSeg;
            public uint PhaseSeg1;
            public uint PhaseSeg2;
            public uint SJW;
            public uint BRP;
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private static ICandleAPI m_candleApi;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public static void Initialize()
        {
            var nativeLoader = new NativeLibraryLoader();

            const string resourceRoot = "CANableSharp.";

            nativeLoader.AddLibraryLocation(OsType.Windows32,
                resourceRoot + "CANableDLL.dll");
            nativeLoader.AddLibraryLocation(OsType.Windows64,
                resourceRoot + "CANableDLL.dll");
            nativeLoader.LoadNativeLibraryFromReflectedAssembly("CANableSharp");
            
            var candle = nativeLoader.LoadNativeInterface<ICandleAPI>();
            m_candleApi = candle ?? throw new Exception("Failed to load native interface?");
        }

        
        public static byte candle_list_scan(IntPtr* list)
        {
            return m_candleApi.candle_list_scan(list);
        }

        
        public static byte candle_list_free(IntPtr list)
        {
            return m_candleApi.candle_list_free(list);
        }

        

        public static byte candle_list_length(IntPtr list, byte* len)
        {
            return m_candleApi.candle_list_length(list, len);
        }

        
        public static byte candle_dev_get(IntPtr list, byte devNum, IntPtr* hdev)
        {
            return m_candleApi.candle_dev_get(list, devNum, hdev);
        }

        
        public static byte candle_dev_get_state(IntPtr hdev, DeviceState* state)
        {
            return m_candleApi.candle_dev_get_state(hdev, state);
        }

        
        // Return is a wide string
        public static ushort* candle_dev_get_path(IntPtr hdev)
        {
            return m_candleApi.candle_dev_get_path(hdev);
        }

        
        public static byte candle_dev_open(IntPtr hdev)
        {
            return m_candleApi.candle_dev_open(hdev);
        }

        
        public static byte candle_dev_get_timestamp_us(IntPtr hdev, uint* timestampUs)
        {
            return m_candleApi.candle_dev_get_timestamp_us(hdev, timestampUs);
        }

        
        public static byte candle_dev_close(IntPtr hdev)
        {
            return m_candleApi.candle_dev_close(hdev);
        }

        
        public static byte candle_dev_free(IntPtr hdev)
        {
            return m_candleApi.candle_dev_free(hdev);
        }

        
        public static byte candle_channel_count(IntPtr hdev, byte* numChannels)
        {
            return m_candleApi.candle_channel_count(hdev, numChannels);
        }

        
        public static byte candle_channel_get_capabilities(IntPtr hdev, byte channel, Capabilities* cap)
        {
            return m_candleApi.candle_channel_get_capabilities(hdev, channel, cap);
        }

        
        public static byte candle_channel_set_timing(IntPtr hdev, byte channel, Bittiming* data)
        {
            return m_candleApi.candle_channel_set_timing(hdev, channel, data);
        }

        
        public static byte candle_channel_set_bitrate(IntPtr hdev, byte channel, uint bitrate)
        {
            return m_candleApi.candle_channel_set_bitrate(hdev, channel, bitrate);
        }

        
        public static byte candle_channel_start(IntPtr hdev, byte channel, uint flags)
        {
            return m_candleApi.candle_channel_start(hdev, channel, flags);
        }

        
        public static byte candle_channel_stop(IntPtr hdev, byte channel)
        {
            return m_candleApi.candle_channel_stop(hdev, channel);
        }

        
        public static byte candle_frame_send(IntPtr hdev, byte channel, Frame* frame)
        {
            return m_candleApi.candle_frame_send(hdev, channel, frame);
        }

        
        public static byte candle_frame_read(IntPtr hdev, Frame* frame, uint timeoutMs)
        {
            return m_candleApi.candle_frame_read(hdev, frame, timeoutMs);
        }

        
        public static FrameType candle_frame_type(Frame* frame)
        {
            return m_candleApi.candle_frame_type(frame);
        }
        
        public static uint candle_frame_id(Frame* frame)
        {
            return m_candleApi.candle_frame_id(frame);
        }
        
        public static byte candle_frame_is_extended_id(Frame* frame)
        {
            return m_candleApi.candle_frame_is_extended_id(frame);
        }
        
        public static byte candle_frame_is_rtr(Frame* frame)
        {
            return m_candleApi.candle_frame_is_rtr(frame);
        }
        
        public static byte candle_frame_dlc(Frame* frame)
        {
            return m_candleApi.candle_frame_dlc(frame);
        }
        
        public static byte* candle_frame_data(Frame* frame)
        {
            return m_candleApi.candle_frame_data(frame);
        }
        
        public static uint candle_frame_timestamp_us(Frame* frame)
        {
            return m_candleApi.candle_frame_timestamp_us(frame);
        }

        
        public static Error candle_dev_last_error(IntPtr hdev)
        {
            return m_candleApi.candle_dev_last_error(hdev);
        }

        
        public static Error candle_list_last_error(IntPtr list)
        {
            return m_candleApi.candle_list_last_error(list);
        }
    }
}
