using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_ARENA_PAGE
    {

        /// uint8_t*
        [MarshalAs(UnmanagedType.LPStr)]
        public string new_address;

        /// uint8_t*
        [MarshalAs(UnmanagedType.LPStr)]
        public string address;

        /// size_t->unsigned __int64
        public ulong size;

        /// size_t->unsigned __int64
        public ulong used;

        /// YR_RELOC*
        public IntPtr reloc_list_head;

        /// YR_RELOC*
        public IntPtr reloc_list_tail;

        /// _YR_ARENA_PAGE*
        public IntPtr next;

        /// _YR_ARENA_PAGE*
        public IntPtr prev;
    }

}
