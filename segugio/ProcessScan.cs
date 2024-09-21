using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{
 



    public class ProcessScan
    {
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, IntPtr dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll")]
        public static extern void GetNativeSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize; // Usa IntPtr per gestire correttamente le dimensioni sia su 32 che su 64 bit
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

        private enum ThreadAccess : int
        {
            SUSPEND_RESUME = 0x0002 //THREAD_SUSPEND_RESUME
        }

  

        

        string dumpFolder { get; set; } // path for saving dumps

        string messageToLog { get; set; }

        public DateTime scanningStartTime { get; set; }
        public volatile Boolean isRunning = true;
        public volatile Boolean scanHasStarted = false;
        public volatile Boolean firstIteration = true;



        Gateway gw;


        private object lockObject = new object();
        private ConcurrentDictionary<uint, bool> suspendedProcesses = new ConcurrentDictionary<uint, bool>();

        //default value 100
        public int scanInterval = 100;

        public bool isEnabledParentScan;

        public ProcessScan(Gateway gw)
        {
            this.gw = gw;
            dumpFolder = gw.settings.GetSetting("DumpFolder"); // path for saving dumps
            isEnabledParentScan = gw.settings.GetBooleanSetting("isEnabledParentScan"); // enabled parent process scan
            gw.ut.CheckPaths("directory", dumpFolder);

            firstIteration = true;

            //get settings for ScanInterval
            String scanIntervalOptionString = gw.settings.GetSetting("ScanInterval");
            if (gw.ut.IsStringNumeric(scanIntervalOptionString))
            {
                scanInterval = int.Parse(scanIntervalOptionString);
            }
            else
            {
                MessageBox.Show($"Unable to read ScanInterval option from {gw.settings.settingFileName}). Check the value out!",
                      "Failed to read ScanInterval option", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Environment.Exit(0);
            }
        }


        public void ScanProcess()
        {
            gw.startThreadProcessScan.WaitOne();


            if(isRunning)
            {
                //update Status
                scanHasStarted = true;
                //flag per aggiornamento iniziale del processo genitore del processo lanciato
                gw.plm.startingPidHasNotified = true;

                scanningStartTime = DateTime.Now;
                //update logview
                messageToLog = ($"process scan started at: {scanningStartTime}");
                gw.mainForm.progress.Report(new UpdateSignal
                {
                    Action = UpdateSignal.UpdateAction.NewLogMessage,
                    LogMessage = messageToLog
                });
            }

            

            while (isRunning)
            {
                gw.pauseEvent.Wait();


                System.Diagnostics.Debug.WriteLine("New Thread for scanning processes");

     
                //identify processes created from the initial pid
                // find processes created from the parent process of the launched process
                //after the launched process has been executed
                gw.plm.searchAllNewProcessesesByLaunchedProcess(gw.pl.launchedProcessParentProcessPid);


                //if I am ready to perform the first iteration of creating the process list and TrackingProcessesList has at least one element
                if (firstIteration && gw.plm.trackingProcessesList.Count > 0)
                {
                    //update gui
                    gw.mainForm.progress.Report(new UpdateSignal
                    {
                        Action = UpdateSignal.UpdateAction.ProcessScanStarted
                    });

                    //switch off flag
                    firstIteration = false;
                }
                else
                {
                    //if the process list has been created, then I can modify it
                    if (gw.mainForm.firstProcessTreeCreationIsMade)
                    {
                        //if there are no changes in progress to the process list
                        if (!gw.mainForm.processTreeIsUpdating)
                        {
                            
                            gw.plm.updateTrackedProcessProperties();
                            
                        }
                        
                    }
                    
                }




                //There is a part that scans the selected processes
                ProcessCheck();

                // Process monitoring code
                if (!isRunning)
                {
                    break;
                }

                //delay 
                Thread.Sleep(scanInterval); 
            }
        }






        ////search for all processes that are child of starting process
        public void ProcessCheck()
        {
            try
            {
                Parallel.ForEach(gw.plm.trackingProcessesList.Values, currentProcess =>
                {
                    if (currentProcess.isRunning)
                    {
                        if (currentProcess.canBeAttached)
                        {
                            
                            System.Diagnostics.Debug.WriteLine("Running YARA against: " + currentProcess.ProcessName + ", " + currentProcess.processID);
                            lock (lockObject)
                            {
                                // Sospendi il processo se non è il padre del processo lanciato
                                if (isEnabledParentScan)
                                {

                                    SuspendProcess(currentProcess);
                                }
                            }

                                var (isMatching, matchedYaraRules) = gw.yaras.RunYaraScan((int)currentProcess.processID);

                                if (isMatching)
                                {
                                    foreach (var matchedYara in matchedYaraRules)
                                    {
                                        if (!currentProcess.isProcessMatchedByYara(matchedYara))
                                        {
                                            currentProcess.addYaraDetection(matchedYara);

                                            string message = ($"process {currentProcess.ProcessName} ({currentProcess.processID}), got detection by YARA rule {matchedYara.Name} !");
                                            gw.mainForm.progress.Report(new UpdateSignal
                                            {
                                                Action = UpdateSignal.UpdateAction.NewLogMessage,
                                                LogMessage = message
                                            });

                                            //invia l'aggiornamento alla GUI per aggiornare il treeview
                                            gw.mainForm.progress.Report(new UpdateSignal
                                            {
                                                Action = UpdateSignal.UpdateAction.UpdatedProcess,
                                                LogMessage = messageToLog
                                            });
                                        }

                                        if (!currentProcess.hasProcessGotConfigExtraction(matchedYara))
                                        {
                                            if (isEnabledParentScan)
                                            {
                                                tryProcessConfigExtration(currentProcess, matchedYara);
                                            }
                                        }
                                    }
                                }
                            lock (lockObject)
                            {
                                // Riprendi il processo
                                if (isEnabledParentScan)
                                {

                                    ResumeProcess(currentProcess);
                                }
                            }
                            
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Errore: " + ex.Message);
            }
        }



        //public void ProcessCheck()
        //{
        //    try
        //    {
        //        foreach (var currentProcess in gw.plm.trackingProcessesList.Values)
        //        {
        //            //se il processo è running e se non è il padre del processo lanciato
        //            if (currentProcess.isRunning )
        //            {
        //                //se ho l'handle al processo
        //                if (currentProcess.canBeAttached)
        //                { 
        //                    System.Diagnostics.Debug.WriteLine(" Running yara against:" + currentProcess.ProcessName + ", " + currentProcess.processID);

        //                    //se il processo attuale non è il padre del processo lanciato
        //                    if (currentProcess.processID != gw.pl.parentPidLaunchedProcess)
        //                    {
        //                        //sospendi il processo
        //                        SuspendProcess(currentProcess);
        //                    }

        //                    var (isMatching, matchedYaraRules) = gw.yaras.RunYaraScan((int)currentProcess.processID);
        //                    //matchedYaraRule è un HAshset<YARA>

        //                    //se trovo un match con le regole yara
        //                    if (isMatching)
        //                    {
        //                        //for each matched yara, add attribution
        //                        foreach (var matchedYara in matchedYaraRules)
        //                        {

        //                            //se NON ho già il matching di una precisa yara per quel processo
        //                            if (!currentProcess.isProcessMatchedByYara(matchedYara))
        //                            {
        //                                //allora non fare nulla perchè è stato già segnalato
        //                                //creo l'attribuzione per il processo
        //                                currentProcess.addYaraDetection(matchedYara);
        //                                //gw.plm.addAttributionToProcess(currentProcess.processID, matchedYara);

        //                                //segnalo l'attribuzione per il processo alla Gui
        //                                string message = ($"process {currentProcess.ProcessName} ({currentProcess.processID}), got detection by YARA rule {matchedYara.Name} !");
        //                                gw.mainForm.progress.Report(new UpdateSignal
        //                                {
        //                                    Action = UpdateSignal.UpdateAction.NewLogMessage,
        //                                    LogMessage = message
        //                                });

        //                                //invia l'aggiornamento alla GUI per aggiornare il treeview
        //                                gw.mainForm.progress.Report(new UpdateSignal
        //                                {
        //                                    Action = UpdateSignal.UpdateAction.UpdatedProcess,
        //                                    LogMessage = messageToLog
        //                                });

        //                            }


        //                            //if current process has NOT yet a config extraction for that yara rule
        //                            if (!currentProcess.hasProcessGotConfigExtraction(matchedYara))
        //                            {
        //                                //se il processo attuale non è il padre del processo lanciato
        //                                if (currentProcess.processID != gw.pl.parentPidLaunchedProcess)
        //                                {
        //                                    //procedi all'estrazione di una eventuale configurazione per quella precisa regola yara
        //                                    tryProcessConfigExtration(currentProcess, matchedYara);
        //                                }

        //                            }

        //                        }
        //                    }

        //                    //se il processo attuale non è il padre del processo lanciato
        //                    if (currentProcess.processID != gw.pl.parentPidLaunchedProcess)
        //                    {
        //                        //riprendi il processo
        //                        ResumeProcess(currentProcess);
        //                    }

        //                }

        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Errore: " + ex.Message);
        //    }


        //}


        //l'istanza di PRocessInfo è già dentro la lista dei processi tracciati
        private void tryProcessConfigExtration(ProcessInfoTracked currentProcess, Yara matchedYara)
        {

            //if exists a script file for the matched Yara Rule
            if (gw.mlwConfigEx.existsConfigExtractorByRule(matchedYara.Name))
            {

                //segnala 
                String message = ($"Starting dumping process {currentProcess.ProcessName} ({currentProcess.processID}) in {dumpFolder}");
                gw.mainForm.progress.Report(new UpdateSignal
                {
                    Action = UpdateSignal.UpdateAction.NewLogMessage,
                    LogMessage = message
                });


                //start dumping
                DumpProcessMemory(currentProcess);

                message = ($"Process {currentProcess.ProcessName} ({currentProcess.processID}) dumped in {dumpFolder}. Now trying to extract configuration!");
                gw.mainForm.progress.Report(new UpdateSignal
                {
                    Action = UpdateSignal.UpdateAction.NewLogMessage,
                    LogMessage = message
                });

                //allora procedi con l'estrazione
                string pythonScriptPath = gw.mlwConfigEx.getConfigExtractorPathByRule(matchedYara.Name);

                //enumerate all dumps in folder
                string[] dmpFiles = Directory.GetFiles(dumpFolder, "*.dmp", SearchOption.AllDirectories);

                // Controlla se sono stati trovati dei file
                if (dmpFiles.Length > 0)
                {
                    string configOutput = "";

                    foreach (string file in dmpFiles)
                    {
                        System.Diagnostics.Debug.WriteLine("running config extractor for " + matchedYara.Name + " on file " + file);

                        var (configFound, result) = gw.mlwConfigEx.pyExecConfScript(pythonScriptPath, file);

                        if (configFound)
                        {
                            // add all results in one string
                            if (result.Length > 0)
                            {
                                configOutput = configOutput + result + "\r\n";
                            }
                        }

                    }

                    //al termine dell'estrazione di tutti i dump di memoria relativamente ad un processo

                    if (configOutput.Length > 0)
                    {
                        //creo la configurazione estratta per il processo
                        //gw.plm.addConfigurationToProcess(currentProcess.processID, matchedYara, configOutput);
                        currentProcess.addMalwareConfiguration(matchedYara, configOutput);


                        string messageLog = ($"process {currentProcess.ProcessName} ({currentProcess.processID}), got config extraction for {matchedYara.Name} !");
                        gw.mainForm.progress.Report(new UpdateSignal
                        {
                            Action = UpdateSignal.UpdateAction.NewLogMessage,
                            LogMessage = messageLog
                        });

                        //invia l'aggiornamento alla GUI per aggiornare il treeview
                        gw.mainForm.progress.Report(new UpdateSignal
                        {
                            Action = UpdateSignal.UpdateAction.UpdatedProcess,
                            LogMessage = messageToLog
                        });

                        //System.Diagnostics.Debug.WriteLine(messageLog);
                    }
                    else
                    {
                        //vuol dire che esiste il config extractor ma che il dump non contiente informazioni succose
                    }

                }
            }
            else
            {
                string messageLog = ($"skipping process {currentProcess.ProcessName} ({currentProcess.processID}), because no config extractor for {matchedYara.Name} !");
                gw.mainForm.progress.Report(new UpdateSignal
                {
                    Action = UpdateSignal.UpdateAction.NewLogMessage,
                    LogMessage = messageLog
                });
                System.Diagnostics.Debug.WriteLine(messageLog);
            }



        }



        private void PrepareDumpFolder(ProcessInfo currentProcess)
        {
            dumpFolder = Path.Combine(gw.desktopPath, $"{currentProcess.ProcessName}_{currentProcess.processID}");
            if (!Directory.Exists(dumpFolder))
            {
                Directory.CreateDirectory(dumpFolder);
                System.Diagnostics.Debug.WriteLine("Subfolder created at: " + dumpFolder);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Subfolder already exists: " + dumpFolder);
            }
        }


        public void DumpProcessMemory(ProcessInfoTracked currentProcess)
        {


            int dumpCount = currentProcess.getNumberOfDumps();
            PrepareDumpFolder(currentProcess); //writes to dump_folder

            //segnalo l'attribuzione per il processo alla Gui
            string message = ($"Dumping {currentProcess.ProcessName} ({currentProcess.processID}) private memory in {dumpFolder}");
            gw.mainForm.progress.Report(new UpdateSignal
            {
                Action = UpdateSignal.UpdateAction.NewLogMessage,
                LogMessage = message
            });

            SYSTEM_INFO systemInfo = new SYSTEM_INFO();
            GetNativeSystemInfo(out systemInfo);

            IntPtr minimumAddress = systemInfo.minimumApplicationAddress;
            IntPtr maximumAddress = systemInfo.maximumApplicationAddress;

            IntPtr processHandle = OpenProcess(ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.QueryInformation, false, (int)currentProcess.processID);
            if (processHandle == IntPtr.Zero)
            {
                throw new Exception("Unable to open process for reading");
            }

            try
            {
                IntPtr currentAddress = minimumAddress;
                while (currentAddress.ToInt64() < maximumAddress.ToInt64())
                {
                    MEMORY_BASIC_INFORMATION mbi = new MEMORY_BASIC_INFORMATION();
                    int mbiSize = Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));

                    //int queryResult = ;
                    if (VirtualQueryEx(processHandle, currentAddress, out mbi, (uint)mbiSize) != mbiSize)
                    {
                        break;
                    }

                    if (mbi.State == (uint)0x1000)
                    {
                        byte[] buffer = new byte[(int)mbi.RegionSize];
                        bool success = ReadProcessMemory(processHandle, mbi.BaseAddress, buffer, mbi.RegionSize, out IntPtr bytesRead);

                        if (success && bytesRead.ToInt64() > 0)
                        {
                            string fileName = Path.Combine(dumpFolder, $"dump_{dumpCount}_{mbi.BaseAddress.ToString("x")}.dmp");
                            File.WriteAllBytes(fileName, buffer);
                        }
                    }

                    // Prepara per la prossima iterazione
                    long nextAddress = currentAddress.ToInt64() + (long)mbi.RegionSize;
                    if (nextAddress < 0)
                    {
                        break; // Previene overflow in caso di indirizzi a 64 bit
                    }
                    currentAddress = new IntPtr(nextAddress);


                }
            }
            finally
            {
                CloseHandle(processHandle);
            }

            currentProcess.setNumberOfDumps(dumpCount);// Aggiorna il conteggio dei dump per questo processo

            
        }





        private void ResumeProcess(ProcessInfoTracked currentProcess)
        {
            if (suspendedProcesses.TryRemove(currentProcess.processID, out bool _))
            {
                Process process = currentProcess.process;
                foreach (ProcessThread thread in process.Threads)
                {
                    IntPtr threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);

                    if (threadHandle != IntPtr.Zero)
                    {
                        ResumeThread(threadHandle);
                        CloseHandle(threadHandle);
                    }
                }

                string messageLog = ($"Process {currentProcess.ProcessName} ({currentProcess.processID}), has been resumed!");
                gw.mainForm.progress.Report(new UpdateSignal
                {
                    Action = UpdateSignal.UpdateAction.NewLogMessage,
                    LogMessage = messageLog
                });
                System.Diagnostics.Debug.WriteLine(messageLog);
            }

            
        }

        private void SuspendProcess(ProcessInfoTracked currentProcess)
        {

            Process process = currentProcess.process;

            foreach (ProcessThread thread in process.Threads)
            {
                IntPtr threadHandle = OpenThread(
                    ThreadAccess.SUSPEND_RESUME,
                    false,
                    (uint)thread.Id);

                if (threadHandle != IntPtr.Zero)
                {
                    SuspendThread(threadHandle);
                    CloseHandle(threadHandle);
                    suspendedProcesses[currentProcess.processID] = true;
                }
            }

            string messageLog = ($"Process {currentProcess.ProcessName} ({currentProcess.processID}), has been suspended!");
            gw.mainForm.progress.Report(new UpdateSignal
            {
                Action = UpdateSignal.UpdateAction.NewLogMessage,
                LogMessage = messageLog
            });
            System.Diagnostics.Debug.WriteLine(messageLog);
        }




    }
}
