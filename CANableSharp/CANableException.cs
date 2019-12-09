using System;
using System.Collections.Generic;
using System.Text;
using static CANableSharp.CandleInvoke;

namespace CANableSharp
{
    public class CANableException : Exception
    {
        public Error Error { get; }

        public CANableException(string msg, Error err) : base($"{msg}: {err}")
        {
            Error = err;
        }
    }
}
