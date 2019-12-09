using System;
using System.Collections.Generic;
using System.Text;
using static CANableSharp.CandleInvoke;

namespace CANableSharp
{
    public unsafe interface ICandleAPI
    {
        
         byte candle_list_scan(IntPtr* list);

        
         byte candle_list_free(IntPtr list);

        

         byte candle_list_length(IntPtr list, byte* len);

        
         byte candle_dev_get(IntPtr list, byte devNum, IntPtr* hdev);

        
         byte candle_dev_get_state(IntPtr hdev, DeviceState* state);

        
        // Return is a wide string
         ushort* candle_dev_get_path(IntPtr hdev);

        
         byte candle_dev_open(IntPtr hdev);

        
         byte candle_dev_get_timestamp_us(IntPtr hdev, uint* timestampUs);

        
         byte candle_dev_close(IntPtr hdev);

        
         byte candle_dev_free(IntPtr hdev);

        
         byte candle_channel_count(IntPtr hdev, byte* numChannels);

        
         byte candle_channel_get_capabilities(IntPtr hdev, byte channel, Capabilities* cap);

        
         byte candle_channel_set_timing(IntPtr hdev, byte channel, Bittiming* data);

        
         byte candle_channel_set_bitrate(IntPtr hdev, byte channel, uint bitrate);

        
         byte candle_channel_start(IntPtr hdev, byte channel, uint flags);

        
         byte candle_channel_stop(IntPtr hdev, byte channel);

        
         byte candle_frame_send(IntPtr hdev, byte channel, Frame* frame);

        
         byte candle_frame_read(IntPtr hdev, Frame* frame, uint timeoutMs);

        
         FrameType candle_frame_type(Frame* frame);
        
         uint candle_frame_id(Frame* frame);
        
         byte candle_frame_is_extended_id(Frame* frame);
        
         byte candle_frame_is_rtr(Frame* frame);
        
         byte candle_frame_dlc(Frame* frame);
        
         byte* candle_frame_data(Frame* frame);
        
         uint candle_frame_timestamp_us(Frame* frame);

        
         Error candle_dev_last_error(IntPtr hdev);

        
         Error candle_list_last_error(IntPtr list);
    }
}
