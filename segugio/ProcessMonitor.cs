using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{
    public class ProcessMonitor
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize; 
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }


        [Flags]
        private enum SnapshotFlags : uint
        {
            Process = 0x00000002,
            Thread = 0x00000004
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

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
        }

        private enum ThreadAccess : int
        {
            SUSPEND_RESUME = 0x0002 //THREAD_SUSPEND_RESUME
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);


        private Gateway gw;
        public Boolean isRunning = true;

        public Boolean firstIteration = true;

        //default value 100
        public int monitorInterval =100;


        public ProcessMonitor(Gateway gw)
        {
            this.gw = gw;

            //get settings for Monitor time interval
            String monitorIntervalOptionString = gw.settings.GetSetting("MonitorInterval");
            if(gw.ut.IsStringNumeric(monitorIntervalOptionString))
            {
                monitorInterval = int.Parse(monitorIntervalOptionString);
            }
            else
            {
                MessageBox.Show($"Unable to read MonitorInterval option from {gw.settings.settingFileName}). Check the value out!",
                      "Failed to read settingFileName option", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Environment.Exit(0);
            }

        }

        //main process monitor Thread
        public void ProcessMonitoringLoop()
        {
            try
            {
                while (isRunning)
                {
                    gw.pauseEvent.Wait();
                    //System.Diagnostics.Debug.WriteLine("New Thread updating processes");
                    UpdateProcessList();

                    //if the scan is not running
                    if(gw.ps.scanHasStarted==false && firstIteration== true)
                    {
                        //send the signal to the GUI
                        gw.mainForm.progress.Report(new UpdateSignal
                        {
                            Action = UpdateSignal.UpdateAction.Idle,
                        });

                        //reset firstIteration
                        firstIteration = false;
                    }

                    // Codice di monitoraggio del processo
                    if (!isRunning)
                    {
                        break;
                    }

                    Thread.Sleep(monitorInterval); // Sleep for a short time or adjust based on your requirement
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("exception:" +ex);

            }

        }

        private void UpdateProcessList()
        {
            {
                IntPtr snapshot = CreateToolhelp32Snapshot(0x00000002, 0); // TH32CS_SNAPPROCESS
                if (snapshot == IntPtr.Zero)
                    return;

                PROCESSENTRY32 processEntry = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32)) };
                if (Process32First(snapshot, ref processEntry))
                {
                    do
                    {
                        ProcessInfo newProcess = new ProcessInfo(
                            processEntry.th32ParentProcessID, // parent PID
                            processEntry.szExeFile, // Process name
                            processEntry.th32ProcessID);  // process ID

                        // Simply add to a light list
                        gw.plm.AddRunningProcess(processEntry.th32ProcessID, newProcess);
                    } 
                    while (Process32Next(snapshot, ref processEntry));
                }

                CloseHandle(snapshot);
            }
        }

    }
}