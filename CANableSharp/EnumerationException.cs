using System;
using System.Collections.Generic;
using System.Text;
using static CANableSharp.CandleInvoke;

namespace CANableSharp
{
    public class EnumerationException : Exception
    {
        public Error Error { get; }

        public EnumerationException(string msg, Error err) : base(msg)
        {
            Error = err;
        }
    }
}
