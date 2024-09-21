using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Explicit)]
    public struct RE_NODE_UNION2
    {

        /// int
        [FieldOffset(0)]
        public int mask;

        /// int
        [FieldOffset(0)]
        public int end;
    }

}
