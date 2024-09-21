using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    /// <summary>
    /// Data structure representing a metadata value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_MATCH
    {

        /// <summary>
        /// Base offset/address for the match. While scanning a file this field is usually zero, while scanning a
        /// process memory space this field is the virtual address of the memory block where the match was found.
        /// </summary>
        public long @base;

        /// <summary>
        /// Offset of the match relative to base.
        /// </summary>
        public long offset;

        /// <summary>
        /// Length of the matching string
        /// </summary>
        public int match_length;

        /// <summary>
        /// Length of data buffer. data_length is the minimum of match_length and MAX_MATCH_DATA.
        /// </summary>
        public int data_length;

        /// Pointer to a buffer containing a portion of the matched data. The size of
        /// the buffer is data_length. data_length is always <= length and is limited
        /// to YR_CONFIG_MAX_MATCH_DATA bytes.
        public IntPtr dataPtr;

        /// YR_MATCH*
        public IntPtr prev;

        /// YR_MATCH*
        public IntPtr next;

        /// If the match belongs to a chained string chain_length contains the
        /// length of the chain. This field is used only in unconfirmed matches.
        public int chain_length;

        /// True if this is match for a private string.
        public bool is_private;
    }

}
