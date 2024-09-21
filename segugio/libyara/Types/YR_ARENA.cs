using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_ARENA
    {

        /// int
        public int flags;

        /// YR_ARENA_PAGE*
        public IntPtr page_list_head;

        /// YR_ARENA_PAGE*
        public IntPtr current_page;
    }

}
