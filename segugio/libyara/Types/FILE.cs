using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FILE
    {
        /// void*
        public IntPtr _Placeholder;
    }

}
