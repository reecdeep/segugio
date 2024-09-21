using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_ATOMS_CONFIG
    {

        /// YR_ATOMS_QUALITY_FUNC
        public YR_ATOMS_QUALITY_FUNC get_atom_quality;

        /// YR_ATOM_QUALITY_TABLE_ENTRY*
        public IntPtr quality_table;

        /// int
        public int quality_warning_threshold;

        /// int
        public int quality_table_entries;

        /// boolean
        [MarshalAs(UnmanagedType.I1)]
        public bool free_quality_table;
    }

}
