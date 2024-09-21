using System;
using System.Runtime.InteropServices;

namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_RULE
    {
        // Global flags
        public int flags;

        // Number of atoms generated for this rule.
        public int num_atoms;

        public IntPtr identifier;

        public IntPtr tags;

        public IntPtr metas;

        public IntPtr strings;

        public IntPtr ns;
    }

}
