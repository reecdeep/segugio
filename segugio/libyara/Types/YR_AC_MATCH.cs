using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YR_AC_MATCH
    {

        /// uint16_t->unsigned short
        public ushort backtrack;

        /// Anonymous_322d3d81_44e2_4124_aab1_d6999178ed23
        public IntPtr Union1;

        /// Anonymous_bd9ed406_ce78_4bea_b418_cf28493d2328
        public IntPtr Union2;

        /// Anonymous_76fe19f7_2b22_4012_933d_b325bbddf15a
        public IntPtr Union3;

        /// Anonymous_2b90270d_a194_4dd5_b795_9b8f91840a31
        public IntPtr Union4;
    }

}
