using System;

namespace dnYara.Interop
{
    [Flags]
    public enum YR_SCAN_FLAGS
    {
        None = 0,
        Fast = Constants.SCAN_FLAGS_FAST_MODE
    };
}