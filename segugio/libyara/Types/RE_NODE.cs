using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RE_NODE
    {

        /// int
        public int type;

        /// Anonymous_04e35de7_8a47_45f8_a2a4_86258eafb8fd
        public RE_NODE_UNION1 Union1;

        /// Anonymous_8068bfc1_65a4_42a4_aae3_8bdcc7b56eb9
        public RE_NODE_UNION2 Union2;

        /// int
        public int greedy;

        /// RE_CLASS*
        public IntPtr re_class;

        /// RE_NODE*
        public IntPtr left;

        /// RE_NODE*
        public IntPtr right;

        /// unsigned char*
        [MarshalAs(UnmanagedType.LPStr)]
        public string forward_code;

        /// unsigned char*
        [MarshalAs(UnmanagedType.LPStr)]
        public string backward_code;
    }

}
