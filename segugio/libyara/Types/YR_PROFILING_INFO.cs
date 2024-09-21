
using System;
using System.Runtime.InteropServices;

namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_PROFILING_INFO
    {
        /// Number of times that some atom belonging to the rule matched. Each
        /// matching atom means a potential string match that needs to be verified.
        public uint atom_matches;

        // Amount of time (in nanoseconds) spent verifying atom matches for
        // determining if the corresponding string actually matched or not. This
        // time is not measured for all atom matches, only 1 out of 1024 matches
        // are actually measured.
        public ulong match_time;

        // Amount of time (in nanoseconds) spent evaluating the rule condition.
        public ulong exec_time;

    }
}