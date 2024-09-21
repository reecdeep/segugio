using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_MATCHES
    {

        /// Anonymous_28ff42bc_8abe_4b79_9a59_b52bac15297f
        public IntPtr head;

        /// Anonymous_215b690c_5de2_45cc_beb5_cde3daeb9b5b
        public IntPtr tail;

        /// int
        public int count;

    }

}
