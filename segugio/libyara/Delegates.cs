using System;
using System.Runtime.InteropServices;

namespace dnYara.Interop
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int YaraScanCallback(
        IntPtr context,
        int message,
        IntPtr message_data,
        IntPtr user_data);
}
