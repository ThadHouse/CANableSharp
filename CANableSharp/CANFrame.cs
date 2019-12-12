using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static CANableSharp.CandleInvoke;

namespace CANableSharp
{
    [StructLayout(LayoutKind.Auto)]
    public unsafe readonly ref struct CANFrame
    {
        public uint RawId { get; }
        public uint Id { get; }
        public uint DLC { get; }
        private readonly ulong data;
        public ulong DataLong => data;
        public Span<byte> Data => MemoryMarshal.Cast<ulong, byte>(new Span<ulong>(Unsafe.AsPointer(ref Unsafe.AsRef(data)), 1)).Slice(0, (int)DLC);

        public bool IsExtended { get; }
        public bool IsRTR { get; }

        public uint TimestampUs { get; }

        internal unsafe CANFrame(Frame* lowLevelFrame, ICandleAPI api)
        {
            RawId = lowLevelFrame->CanId;
            Id = api.candle_frame_id(lowLevelFrame);
            data = lowLevelFrame->Data;
            DLC = api.candle_frame_dlc(lowLevelFrame);
            IsExtended = api.candle_frame_is_extended_id(lowLevelFrame) != 0;
            IsRTR = api.candle_frame_is_rtr(lowLevelFrame) != 0;
            TimestampUs = api.candle_frame_timestamp_us(lowLevelFrame);
        }
    }
}
