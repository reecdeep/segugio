using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{

    public class ProcessInfo
    {


        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        };


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        public static extern bool Process32Next(IntPtr hSnapshot,
            ref PROCESSENTRY32 lppe);

        


        public Boolean isParentPidRunning { get; private set; }
        public uint ParentPID { get; private set; }
        public string ProcessName { get; private set; }
        public uint processID { get; private set; }
        public DateTime? startingProcessTime { get; set; }

        public string machineName = Environment.MachineName;

        private readonly object lockObject = new object();
        public Boolean isRunning { get; set; }


        //New monitored process structure
        public ProcessInfo(uint parentPID, string processName,  uint processID)
        {
            this.ParentPID = parentPID;
            this.ProcessName = processName;
            this.processID = processID;
            this.isParentPidRunning = this.IsProcessRunning(parentPID);
            this.isRunning = this.IsProcessRunning(processID);

        }




        public bool IsProcessRunning(uint pid)
        {
            lock (lockObject)
            {
                IntPtr hSnapshot = CreateToolhelp32Snapshot(0x00000002, 0); //0x00000002 TH32CS_SNAPPROCESS 

                if (hSnapshot == IntPtr.Zero)
                    return false;

                PROCESSENTRY32 pe32 = new PROCESSENTRY32();
                pe32.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));

                if (Process32First(hSnapshot, ref pe32))
                {
                    do
                    {
                        if (pe32.th32ProcessID == pid)
                        {
                            //System.Diagnostics.Debug.WriteLine($"process: {pid} ESISTE! ");

                            return true;
                        }

                    } while (Process32Next(hSnapshot, ref pe32));
                }
                //System.Diagnostics.Debug.WriteLine($"process: {pid} NON ESISTE! ");
                return false;
            }

            
        }



    }
}
