using System.Runtime.InteropServices;

namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_STOPWATCH_WIN
    {
        public ulong frequency;
        public ulong start;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct mach_timebase_info
    {
        public uint numer;
        public uint denom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct YR_STOPWATCH_OSX
    {
        public mach_timebase_info timebase;
        public ulong start;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct timeval {
        public long tv_sec;
        public int tc_usec;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct timespec {
        public long tv_sec;
        public int tc_nsec;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct YR_STOPWATCH_LINUX
    {
        public timeval tv_start;
        public timespec ts_start;
    }
}
