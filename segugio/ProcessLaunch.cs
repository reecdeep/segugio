using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace segugio
{
    public class ProcessLaunch
    {

        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);
        [DllImport("kernel32.dll")]
        public static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);
        [DllImport("kernel32.dll")]
        public static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);
        [DllImport("kernel32.dll")]
        public static extern IntPtr QueueUserAPC(IntPtr pfnAPC, IntPtr hThread, IntPtr dwData);
        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, Int32 dwSize, UInt32 flAllocationType, UInt32 flProtect);
        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(IntPtr handle, IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);
        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, ref IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);


        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);

        private enum AssocStr
        {
            Command = 1
        }

        [Flags]
        private enum AssocF
        {
            None = 0,
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [Flags]
        public enum ProcessAccessFlags : uint
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

        public static class CreationFlags
        {
            public const uint SUSPENDED = 0x4;
            public const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
            public const uint CREATE_NO_WINDOW = 0x08000000;
        }

        public static readonly UInt32 MEM_COMMIT = 0x1000;
        public static readonly UInt32 MEM_RESERVE = 0x2000;
        public static readonly UInt32 PAGE_EXECUTE_READ = 0x20;
        public static readonly UInt32 PAGE_READWRITE = 0x04;

        public const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
      //  public const long PROCESS_CREATION_MITIGATION_POLICY_BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;
        public const int PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;


        public uint launchedProcessPid { get; set; }
        public uint launchedProcessParentProcessPid { get; set; }
        public String launchedProcessParentProcessName { get; set; }
        //custom command line set by GUI
        public String customCommandLine { get; set; }

        private object lockObject = new object();




        Gateway gw;
        public ProcessLaunch(Gateway gw)
        {
            this.gw = gw;
        }


        String fileToOpen;
        String additionalArguments;


        public void AddStartupInfo(string processToOpen, string additionalArguments)
        {
            lock (lockObject)
            {
                this.fileToOpen = processToOpen;
                this.additionalArguments = additionalArguments;
            }
        }


        public void LaunchProcess()
        {

            try
            {
                
                    gw.startThreadProcessLaunch.WaitOne();

                    string extension = Path.GetExtension(fileToOpen);
                    string programPathTemplate = GetDefaultProgramPath(extension);
                    string cleanedCommand;
                    if(programPathTemplate!=null)
                    {
                        
                        if (programPathTemplate.Contains("%1"))
                        {
                            string commandToExecute = programPathTemplate.Replace("%1", fileToOpen).Replace(" %*", additionalArguments);
                            cleanedCommand = commandToExecute.Replace("\"", " ").Trim();
                        }
                        else
                        {
                            
                            cleanedCommand = (programPathTemplate + " " + fileToOpen).Replace("\"", " ").Trim();
                        }


                        //check for custom commandline
                        if (customCommandLine != null)
                        {
                            cleanedCommand = customCommandLine;
                            cleanedCommand = cleanedCommand.Replace("%FILENAME%", fileToOpen);
                            cleanedCommand = "cmd.exe /c " + cleanedCommand;
                        }


                        //if program for opening file exists
                        if (cleanedCommand.Length > 0)
                        {

                            uint? parentProc = launchedProcessParentProcessPid;

                            if (parentProc.HasValue)
                            {


                                Debug.WriteLine("tento lo spoof");

                                bool result = pidSpoof((int)parentProc, cleanedCommand);


                                if (result)
                                {

                                    //Debug.WriteLine($"PROCESS LAUNCHED {launchedProcessPid}");

                                    // Signal Thread Process Scan to start alternating with Thread Process Monitor
                                    gw.startThreadProcessScan.Set();

                                }
                                else
                                {
                                    MessageBox.Show($"Unable to spawn process using {launchedProcessParentProcessName} ({launchedProcessParentProcessPid}) as parent process.",
                                   "Failed to spawn process", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                    Environment.Exit(0);
                                }
                            }
                            else
                            {
                                Debug.WriteLine("failed to spawn");
                            }

                        }


                    }
                    

                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("exception:" + ex);

            }

            
        }



        public string GetDefaultProgramPath(string extension)
        {
            uint pcchOut = 0;
            AssocQueryString(AssocF.None, AssocStr.Command, extension, null, null, ref pcchOut);

            if (pcchOut == 0)
            {
                return null;
            }

            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            AssocQueryString(AssocF.None, AssocStr.Command, extension, null, pszOut, ref pcchOut);

            return pszOut.ToString();
        }




        public bool pidSpoof(int parentProc, string commandLine)
        {
            bool result = false;
            STARTUPINFOEX siex = new STARTUPINFOEX();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            IntPtr procHandle = IntPtr.Zero, lpValueProc = IntPtr.Zero;

            try
            {
                procHandle = OpenProcess(ProcessAccessFlags.CreateProcess, false, parentProc);
                if (procHandle == IntPtr.Zero) throw new Exception("Failed to open parent process");

                IntPtr lpSize = IntPtr.Zero;
                InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
                if (lpSize == IntPtr.Zero) throw new Exception("Failed to initialize proc thread attribute list");

                siex.lpAttributeList = Marshal.AllocHGlobal(lpSize);
                if (!InitializeProcThreadAttributeList(siex.lpAttributeList, 1, 0, ref lpSize))
                    throw new Exception("Failed to initialize proc thread attribute list second time");

                lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(lpValueProc, procHandle);
                if (!UpdateProcThreadAttribute(siex.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, lpValueProc, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero))
                    throw new Exception("Failed to update proc thread attribute for parent process");

                if (!CreateProcess(null, commandLine, IntPtr.Zero, IntPtr.Zero, false, CreationFlags.SUSPENDED | CreationFlags.EXTENDED_STARTUPINFO_PRESENT, IntPtr.Zero, null, ref siex, ref pi))
                    throw new Exception("Failed to create process");

                // Success
                launchedProcessPid = (uint)pi.dwProcessId;
                ResumeThread(pi.hThread);
                System.Diagnostics.Debug.WriteLine($"Process {pi.dwProcessId} resumed and spoof completed");
                result = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (siex.lpAttributeList != IntPtr.Zero) Marshal.FreeHGlobal(siex.lpAttributeList);
                if (lpValueProc != IntPtr.Zero) Marshal.FreeHGlobal(lpValueProc);
                if (pi.hProcess != IntPtr.Zero) CloseHandle(pi.hProcess);
                if (pi.hThread != IntPtr.Zero) CloseHandle(pi.hThread);
                if (procHandle != IntPtr.Zero) CloseHandle(procHandle);
            }

            return result;
        }




    }
}
