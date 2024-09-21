using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct YR_ATOM_QUALITY_TABLE_ENTRY
    {

        /// char[4]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string atom;

        /// unsigned char
        public byte quality;
    }

}
