using System;
using System.Runtime.InteropServices;

namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RE_FIBER_LIST
    {
        public IntPtr head;
        public IntPtr tail;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RE_FIBER_POOL
    {
        public int fiber_count;
        RE_FIBER_LIST fibers;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct YR_SCAN_CONTEXT_WIN
    {
        /// File size of the file being scanned.
        public ulong file_size_bytes;
        /// Entry point of the file being scanned, if the file is PE or ELF.
        public ulong entry_point;
        /// Scanning flags.
        public int flags;
        /// Canary value used for preventing hand-crafted objects from being embedded
        /// in compiled rules and used to exploit YARA. The canary value is initialized
        /// to a random value and is subsequently set to all objects created by
        /// yr_object_create. The canary is verified when objects are used by
        /// yr_execute_code.
        public int canary;

        /// Scan timeout in nanoseconds.
        public ulong timeout_ns;

        /// Pointer to user-provided data passed to the callback function.
        public IntPtr user_data;

        /// Pointer to the user-provided callback function that is called when an
        /// event occurs during the scan (a rule matching, a module being loaded, etc)
        public IntPtr callback;

        /// Pointer to the YR_RULES object associated to this scan context.
        public IntPtr rules;

        /// Pointer to the YR_STRING causing the most recent scan error.
        public IntPtr last_error_string;

        /// Pointer to the iterator used for scanning
        public IntPtr iterator;

        /// Pointer to a table mapping identifiers to YR_OBJECT structures. This table
        /// contains entries for external variables and modules.
        public IntPtr objects_table;

        /// Notebook used for storing YR_MATCH structures associated to the matches found.
        public IntPtr matches_notebook;

        /// Stopwatch used for measuring the time elapsed during the scan.
        public YR_STOPWATCH_WIN stopwatch;

        /// Fiber pool used by yr_re_exec.
        public RE_FIBER_POOL re_fiber_pool;

        /// A bitmap with one bit per rule, bit N is set when the rule with index N
        /// has matched.
        public IntPtr rule_matches_flags;

        /// A bitmap with one bit per namespace, bit N is set if the namespace with
        /// index N has some global rule that is not satisfied.
        public IntPtr ns_unsatisfied_flags;

        /// Array with pointers to lists of matches. Item N in the array has the
        /// list of matches for string with index N.
        public IntPtr matches;

        /// "unconfirmed_matches" is like "matches" but for strings that are part of
        /// a chain. Let's suppose that the string S is split in two chained strings
        /// S1 <- S2. When a match is found for S1, we can't be sure that S matches
        /// until a match for S2 is found (within the range defined by chain_gap_min
        /// and chain_gap_max), so the matches for S1 are put in "unconfirmed_matches"
        /// until they can be confirmed or discarded.
        public IntPtr unconfirmed_matches;

        /// profiling_info is a pointer to an array of YR_PROFILING_INFO structures,
        /// one per rule. Entry N has the profiling information for rule with index N.
        public IntPtr profiling_info;
    }

    
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_SCAN_CONTEXT_OSX
    {
        /// File size of the file being scanned.
        public ulong file_size_bytes;
        /// Entry point of the file being scanned, if the file is PE or ELF.
        public ulong entry_point;
        /// Scanning flags.
        public int flags;
        /// Canary value used for preventing hand-crafted objects from being embedded
        /// in compiled rules and used to exploit YARA. The canary value is initialized
        /// to a random value and is subsequently set to all objects created by
        /// yr_object_create. The canary is verified when objects are used by
        /// yr_execute_code.
        public int canary;

        /// Scan timeout in nanoseconds.
        public ulong timeout_ns;

        /// Pointer to user-provided data passed to the callback function.
        public IntPtr user_data;

        /// Pointer to the user-provided callback function that is called when an
        /// event occurs during the scan (a rule matching, a module being loaded, etc)
        public IntPtr callback;

        /// Pointer to the YR_RULES object associated to this scan context.
        public IntPtr rules;

        /// Pointer to the YR_STRING causing the most recent scan error.
        public IntPtr last_error_string;

        /// Pointer to the iterator used for scanning
        public IntPtr iterator;

        /// Pointer to a table mapping identifiers to YR_OBJECT structures. This table
        /// contains entries for external variables and modules.
        public IntPtr objects_table;

        /// Notebook used for storing YR_MATCH structures associated to the matches found.
        public IntPtr matches_notebook;

        /// Stopwatch used for measuring the time elapsed during the scan.
        public YR_STOPWATCH_OSX stopwatch;

        /// Fiber pool used by yr_re_exec.
        public RE_FIBER_POOL re_fiber_pool;

        /// A bitmap with one bit per rule, bit N is set when the rule with index N
        /// has matched.
        public IntPtr rule_matches_flags;

        /// A bitmap with one bit per namespace, bit N is set if the namespace with
        /// index N has some global rule that is not satisfied.
        public IntPtr ns_unsatisfied_flags;

        /// Array with pointers to lists of matches. Item N in the array has the
        /// list of matches for string with index N.
        public IntPtr matches;

        /// "unconfirmed_matches" is like "matches" but for strings that are part of
        /// a chain. Let's suppose that the string S is split in two chained strings
        /// S1 <- S2. When a match is found for S1, we can't be sure that S matches
        /// until a match for S2 is found (within the range defined by chain_gap_min
        /// and chain_gap_max), so the matches for S1 are put in "unconfirmed_matches"
        /// until they can be confirmed or discarded.
        public IntPtr unconfirmed_matches;

        /// profiling_info is a pointer to an array of YR_PROFILING_INFO structures,
        /// one per rule. Entry N has the profiling information for rule with index N.
        public IntPtr profiling_info;
    }

    
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_SCAN_CONTEXT_LINUX
    {
        /// File size of the file being scanned.
        public ulong file_size_bytes;
        /// Entry point of the file being scanned, if the file is PE or ELF.
        public ulong entry_point;
        /// Scanning flags.
        public int flags;
        /// Canary value used for preventing hand-crafted objects from being embedded
        /// in compiled rules and used to exploit YARA. The canary value is initialized
        /// to a random value and is subsequently set to all objects created by
        /// yr_object_create. The canary is verified when objects are used by
        /// yr_execute_code.
        public int canary;

        /// Scan timeout in nanoseconds.
        public ulong timeout_ns;

        /// Pointer to user-provided data passed to the callback function.
        public IntPtr user_data;

        /// Pointer to the user-provided callback function that is called when an
        /// event occurs during the scan (a rule matching, a module being loaded, etc)
        public IntPtr callback;

        /// Pointer to the YR_RULES object associated to this scan context.
        public IntPtr rules;

        /// Pointer to the YR_STRING causing the most recent scan error.
        public IntPtr last_error_string;

        /// Pointer to the iterator used for scanning
        public IntPtr iterator;

        /// Pointer to a table mapping identifiers to YR_OBJECT structures. This table
        /// contains entries for external variables and modules.
        public IntPtr objects_table;

        /// Notebook used for storing YR_MATCH structures associated to the matches found.
        public IntPtr matches_notebook;

        /// Stopwatch used for measuring the time elapsed during the scan.
        public YR_STOPWATCH_LINUX stopwatch;

        /// Fiber pool used by yr_re_exec.
        public RE_FIBER_POOL re_fiber_pool;

        /// A bitmap with one bit per rule, bit N is set when the rule with index N
        /// has matched.
        public IntPtr rule_matches_flags;

        /// A bitmap with one bit per namespace, bit N is set if the namespace with
        /// index N has some global rule that is not satisfied.
        public IntPtr ns_unsatisfied_flags;

        /// Array with pointers to lists of matches. Item N in the array has the
        /// list of matches for string with index N.
        public IntPtr matches;

        /// "unconfirmed_matches" is like "matches" but for strings that are part of
        /// a chain. Let's suppose that the string S is split in two chained strings
        /// S1 <- S2. When a match is found for S1, we can't be sure that S matches
        /// until a match for S2 is found (within the range defined by chain_gap_min
        /// and chain_gap_max), so the matches for S1 are put in "unconfirmed_matches"
        /// until they can be confirmed or discarded.
        public IntPtr unconfirmed_matches;

        /// profiling_info is a pointer to an array of YR_PROFILING_INFO structures,
        /// one per rule. Entry N has the profiling information for rule with index N.
        public IntPtr profiling_info;
    }
}
