using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Explicit)]
    public struct RE_NODE_UNION1
    {

        /// int
        [FieldOffset(0)]
        public int value;

        /// int
        [FieldOffset(0)]
        public int count;

        /// int
        [FieldOffset(0)]
        public int start;
    }

}
