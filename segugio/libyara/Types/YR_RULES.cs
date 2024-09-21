using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_RULES
    {
        /// YR_ARENA*
        public IntPtr arena;

        /// YR_RULE*
        public IntPtr rules_list_head;

        /// YR_STRING*
        public IntPtr strings_list_head;

        /// YR_EXTERNAL_VARIABLE*
        public IntPtr externals_list_head;

        /// YR_AC_TRANSITION_TABLE->YR_AC_TRANSITION*
        public IntPtr ac_transition_table;

        /// YR_AR_MATCH*
        public IntPtr ac_match_pool;

        /// YR_AC_MATCH_TABLE->YR_AC_MATCH_TABLE_ENTRY*
        public IntPtr ac_match_table;

        public IntPtr code_start;

        // Total number of rules.
        public uint num_rules;

        // Total number of strings.
        public uint num_strings;

        // Total number of namespaces.
        public uint num_namespaces;
    }
}
