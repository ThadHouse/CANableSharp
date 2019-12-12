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

        private static Lazy<ICandleAPI> m_lazyCandleApi = new Lazy<ICandleAPI>(Initialize, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        public static ICandleAPI CandleAPI => m_lazyCandleApi.Value;

        private static ICandleAPI Initialize()
        {
            var nativeLoader = new NativeLibraryLoader();

            const string resourceRoot = "CANableSharp.";

            nativeLoader.AddLibraryLocation(OsType.Windows32,
                resourceRoot + "CANableDLL.dll");
            nativeLoader.AddLibraryLocation(OsType.Windows64,
                resourceRoot + "CANableDLL.dll");
            nativeLoader.LoadNativeLibraryFromReflectedAssembly("CANableSharp");
            
            var candle = nativeLoader.LoadNativeInterface<ICandleAPI>();
            return candle ?? throw new Exception("Failed to load native interface?");
        }
    }
}
