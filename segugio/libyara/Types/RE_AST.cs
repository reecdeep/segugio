using System;
using System.Runtime.InteropServices;

namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RE_AST
    {
        /// unsigned int
        public uint flags;

        /// unsigned short
        public ushort levels;

        /// RE_NODE*
        public IntPtr root_node;
    }

}
