using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RE_CLASS
    {

        /// unsigned char
        public byte negated;

        /// unsigned char[32]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string bitmap;
    }

}
