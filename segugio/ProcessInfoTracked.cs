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

    public class ProcessInfoTracked:ProcessInfo
    {
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
        public struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RTL_USER_PROCESS_PARAMETERS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Reserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public IntPtr[] Reserved2;
            public UNICODE_STRING ImagePathName;
            public UNICODE_STRING CommandLine;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PEB_32
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Reserved1;
            public byte BeingDebugged;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] Reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public IntPtr[] Reserved3;
            public IntPtr Ldr;
            public IntPtr ProcessParameters;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public IntPtr[] Reserved4;
            public IntPtr AtlThunkSListPtr;
            public IntPtr Reserved5;
            public uint Reserved6;
            public IntPtr Reserved7;
            public uint Reserved8;
            public uint AtlThunkSListPtr32;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 45)]
            public IntPtr[] Reserved9;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
            public byte[] Reserved10;
            public IntPtr PostProcessInitRoutine;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] Reserved11;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public IntPtr[] Reserved12;
            public uint SessionId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PEB_64
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Reserved1;
            public byte BeingDebugged;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
            public byte[] Reserved2;
            public IntPtr LoaderData;
            public IntPtr ProcessParameters;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 520)]
            public byte[] Reserved3;
            public IntPtr PostProcessInitRoutine;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 136)]
            public byte[] Reserved4;
            public uint SessionId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public IntPtr[] Reserved2;
            public IntPtr UniqueProcessID;
            public IntPtr Reserved3;
        }


        // Definizione di OpenProcess da kernel32.dll
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        // Definizione di CloseHandle da kernel32.dll
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        // Costanti per i diritti di accesso al processo
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_READ = 0x0010;



        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryInformationProcess(IntPtr processHandle,
            int processInformationClass,
            ref PROCESS_BASIC_INFORMATION processInformation,
            int processInformationLength,
            out int returnLength);


        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead);


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool IsWow64Process(IntPtr hProcess,
            out Boolean Wow64Process);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("shell32.dll", EntryPoint = "ExtractIcon")]
        private static extern IntPtr ExtractIcon(IntPtr hInst,
            string lpszExeFileName,
            int nIconIndex);






        public Boolean isStartingProcess { get; set; }
        public Boolean canBeAttached { get; set; }
        public string CommandLine { get; set; }
        public Process process { get; set; }
        public Icon processIcon { get; set; }

        public ProcessTreeNode treenode { get; set; }


        //whitelist to avoid scanning certain processes
        //HashSet<string> whitelistProcessesNotToBeScanned = new HashSet<string> { "Memory Compression", "ShellExperienceHost.exe", "dllhost.exe", "RuntimeBroker.exe", "TextInputHost.exe", "ServiceHub.Host.CLR.x86.exe", "[System Process]", "System", "devenv.exe", "segugio.exe", "ProcessHacker.exe", "svchost.exe", "wininit.exe", "services.exe", "spoolsv.exe", "SearchIndexer.exe", "lsass.exe", "winlogon.exe", "explorer.exe", "csrss.exe", "MSBuild.exe", "conhost.exe", "VBCSCompiler.exe", "IntelliTrace.exe", "PerfWatson2.exe", "Microsoft.ServiceHub.Controller.exe", "SearchFilterHost.exe", "ScriptedSandBox64.exe", "ScriptedSandbox64.exe", "SecurityHealthSystray.exe" };

        //set for gettin' all  matched yaraRule for a single process
        public HashSet<Yara> matchedYaraRules { get; set; }

        //config extractor dictionary
        public Dictionary<Yara, string> dictMalwareConfiguration { get; set; }

        //track number of dumps for a process
        int numberoOfdumps = 0;

        //process ha parent process running or not
        public Boolean isRootProcess; 


        // new tracked process structure
        public ProcessInfoTracked(uint parentPID, string processName, uint processID)
        : base(parentPID, processName, processID)
        {
            matchedYaraRules = new HashSet<Yara>();

            dictMalwareConfiguration = new Dictionary<Yara, string>();
        }





        
        public int getNumberOfDumps()
        {
            return this.numberoOfdumps;
        }

        
        public void setNumberOfDumps(int number)
        {
            this.numberoOfdumps = number;
        }


        // update malware configuration 
        public void addMalwareConfiguration(Yara YaraRule, string malConf)
        {
            if (!dictMalwareConfiguration.ContainsKey(YaraRule))
            {
                dictMalwareConfiguration.Add(YaraRule, malConf);
            }
        }

        // update detection and its description
        public void addYaraDetection(Yara matchYara)
        {
            bool added = matchedYaraRules.Add(matchYara);

            if (added)
            {
                Console.WriteLine("Yara Rule attribuita con successo a "+ this.ProcessName);
            }
            else
            {
                Console.WriteLine("Yara Rule esiste già per " + this.ProcessName);
            }

        }


        //method return true if yara has been already matched
        public Boolean isProcessMatchedByYara(Yara matchedYara)
        {
            Boolean matched = false;

            if(this.matchedYaraRules.Contains(matchedYara))
            {
                matched = true;
            }

            return matched;
        }

        //method return true if configuration has been already matched
        public Boolean hasProcessGotConfigExtraction(Yara matchedYara)
        {
            Boolean gotConfig = false;

            if (dictMalwareConfiguration.ContainsKey(matchedYara))
            {
                gotConfig = true;
            }

            return gotConfig;
        }



        public void getProcessIcon()
        {

            try
            {
                if (!(this.process.HasExited))
                {
                    string filePath = this.process.MainModule.FileName;
                    IntPtr iconHandle = ExtractIcon(IntPtr.Zero, filePath, 0);

                    if (iconHandle != IntPtr.Zero)
                    {
                        this.processIcon = Icon.FromHandle(iconHandle);
                    }
                    else
                    {
                        using (Bitmap bmp = new Bitmap(Properties.Resources._39368_executable_icon))
                        {
                            IntPtr hIcon = bmp.GetHicon();
                            this.processIcon = Icon.FromHandle(hIcon);
                        }
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                // Log the exception or handle it as necessary
                Console.WriteLine($"Errore: {ex.Message}");

                // Utilizza l'icona di riserva in caso di errore
                using (Bitmap bmp = new Bitmap(Properties.Resources._39368_executable_icon))
                {
                    IntPtr hIcon = bmp.GetHicon();
                    this.processIcon = Icon.FromHandle(hIcon);
                }
            }
            catch (Exception ex)
            {
                // Gestisce altre eccezioni generiche
                Console.WriteLine($"Errore inatteso: {ex.Message}");

                // Utilizza l'icona di riserva in caso di errore
                using (Bitmap bmp = new Bitmap(Properties.Resources._39368_executable_icon))
                {
                    IntPtr hIcon = bmp.GetHicon();
                    this.processIcon = Icon.FromHandle(hIcon);
                }
            }


        }


        //method for setting Icon to process not running anymore
        public void getDefaultIcon()
        {
            using (Bitmap bmp = new Bitmap(Properties.Resources._39368_executable_icon))
            {
                IntPtr hIcon = bmp.GetHicon();
                this.processIcon = Icon.FromHandle(hIcon);
            }
        }

        //method for verify handle to process
        public void CanAttachToProcess(uint processID)
        {
            
            // try to opern process with QUERY_INFORMATION
            IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION, false, process.Id);

            if (processHandle != IntPtr.Zero)
            {
               
                CloseHandle(processHandle);
                canBeAttached =  true;
            }
            else
            {
                
                canBeAttached = false;
            }

          
        }

        //get process command line
        public void GetCommandLine()
        {
            

            bool is64BitPeb = false;
            if (Environment.Is64BitOperatingSystem)
            {
                if (!Is32BitProcessOn64BitOs(this.process))
                {
                    is64BitPeb = true;
                }
               
            }

            //using (ProcessMemoryReader memoryReader = new ProcessMemoryReader(targetProcess))
            //{
                PROCESS_BASIC_INFORMATION processInfo = new PROCESS_BASIC_INFORMATION();
                int result = NtQueryInformationProcess(this.process.Handle, 
                    0, 
                    ref processInfo, 
                    Marshal.SizeOf(processInfo), out _);
                if (result != 0)
                {
                    throw new Exception("Unable to open process for reading");
                    //throw new error
                    //throw new System.ComponentModel.Win32Exception(RtlNtStatusToDosError(result));
                }

                int pebLength = is64BitPeb ? Marshal.SizeOf(typeof( PEB_64)) : Marshal.SizeOf(typeof( PEB_32));

            //read Related bytes
            byte[] pebBytes = new byte[pebLength];
            bool resultReadPeb = ReadProcessMemory(this.process.Handle, processInfo.PebBaseAddress, pebBytes, pebLength, out _);

            GCHandle pebBytesPtr = GCHandle.Alloc(pebBytes, GCHandleType.Pinned);
                try
                {
                    IntPtr procParamsPtr;
                    if (is64BitPeb)
                    {
                    var PEB = (PEB_64)Marshal.PtrToStructure(pebBytesPtr.AddrOfPinnedObject(), typeof(PEB_64));
                    procParamsPtr = PEB.ProcessParameters;
                    }
                    else
                    {
                    var PEB = (PEB_32)Marshal.PtrToStructure(pebBytesPtr.AddrOfPinnedObject(), typeof(PEB_32));
                    procParamsPtr = PEB.ProcessParameters;
                    }


                //read Related bytes
                byte[] procParamsBytes = new byte[Marshal.SizeOf(typeof(RTL_USER_PROCESS_PARAMETERS))];
                bool resultProcParamsBytes = ReadProcessMemory(this.process.Handle, 
                    procParamsPtr,
                    procParamsBytes,
                    Marshal.SizeOf(typeof(RTL_USER_PROCESS_PARAMETERS)), out _);


                    GCHandle procParamsBytesPtr = GCHandle.Alloc(procParamsBytes, GCHandleType.Pinned);
                    try
                    {
                    RTL_USER_PROCESS_PARAMETERS procParams = (RTL_USER_PROCESS_PARAMETERS)Marshal.PtrToStructure(procParamsBytesPtr.AddrOfPinnedObject(), typeof(RTL_USER_PROCESS_PARAMETERS));
                    UNICODE_STRING cmdLineUnicodeString = procParams.CommandLine;

                    //read Related bytes
                    byte[] cmdLineBytes = new byte[cmdLineUnicodeString.Length];
                    bool resultCmdLineBytes = ReadProcessMemory(this.process.Handle,
                        cmdLineUnicodeString.Buffer,
                        cmdLineBytes,
                        cmdLineUnicodeString.Length, out _);


                        this.CommandLine = Encoding.Unicode.GetString(cmdLineBytes);
                    }
                    finally
                    {
                        if (procParamsBytesPtr.IsAllocated)
                        {
                            procParamsBytesPtr.Free();
                        }
                    }
                }
                finally
                {
                    if (pebBytesPtr.IsAllocated)
                    {
                        pebBytesPtr.Free();
                    }
                }
            //}
        }

        public bool Is32BitProcessOn64BitOs(Process targetProcess)
        {
            bool isWow64 = false;
            if (MethodExistsInDll("kernel32.dll", "IsWow64Process"))
            {
                
                IsWow64Process(targetProcess.Handle, out isWow64);
               
               
            }
            return isWow64;
        }
        public static bool MethodExistsInDll(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return GetProcAddress(moduleHandle, methodName) != IntPtr.Zero;
        }



    }
}
