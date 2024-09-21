using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{
    public class ProcessListManager
    {
        //
        public int numberRootProcesses = 0;
        public int numberTerminatedProcesses = 0;
        public int numberCreatedProcesses = 0;

        //contains all existing and existed processes
        public ConcurrentDictionary<uint,ProcessInfo> mainProcessList;
       
        //process being part of SCAN analysis
        public ConcurrentDictionary<uint, ProcessInfoTracked> trackingProcessesList;

        

        Gateway gw;

        String messageToLog { get; set; } //handle to new message to log

        public volatile Boolean startingPidHasNotified = false;

        public ProcessListManager(Gateway gw)
        {
            this.gw = gw;

            mainProcessList = new ConcurrentDictionary<uint, ProcessInfo>();
            trackingProcessesList = new ConcurrentDictionary<uint, ProcessInfoTracked>();
            

            
        }

        //method for updating the list of only running processes
        public Dictionary<uint, String> UpdateAndGetOnlyRunningProcessList()
        {
            //contains only running processes
            Dictionary<uint, String> onlyRunningProcessList = new Dictionary<uint, String>(); ;

            //clear list
            onlyRunningProcessList.Clear();

            //for each listed process
            foreach (var process in gw.plm.mainProcessList.Values)
            {
                //if process is running and is not the current process and has a valid handle
                if (process.isRunning == process.IsProcessRunning(process.processID)
                    &&  !(process.ProcessName.Equals(gw.applicationName))  ) 
                {
                    //add to only running process
                    onlyRunningProcessList.Add(process.processID, process.ProcessName);
                }

            }

            return onlyRunningProcessList;
        }


        //This method is used to keep track of all existing and past processes
        public void AddRunningProcess(uint pid, ProcessInfo process)
        {

            if (!(mainProcessList.ContainsKey(pid)) )
            {

                try
                {
                    Process proc = Process.GetProcessById((int)pid, process.machineName);
                    process.startingProcessTime = proc.StartTime;
                }
                catch (Exception ex)
                {
                    process.startingProcessTime = null;
                }

                mainProcessList.TryAdd(pid, process);

            }


        }




        //method for finding changes in traced processes
        public void updateTrackedProcessProperties()
        {
            
            ConcurrentDictionary<uint, ProcessInfoTracked> newProcesses = new ConcurrentDictionary<uint, ProcessInfoTracked>();

            foreach (var process in gw.plm.trackingProcessesList.Values)
            {
                //if process is running no more
                if (process.isRunning != process.IsProcessRunning(process.processID))
                {
                    //
                    process.isRunning = process.IsProcessRunning(process.processID);

                    //send update to GUI - logview
                    messageToLog = ($"process terminated: {process.ProcessName} ({process.processID})");
                    gw.mainForm.progress.Report(new UpdateSignal
                    {
                        Action = UpdateSignal.UpdateAction.NewLogMessage,
                        LogMessage = messageToLog
                    });

                    //send update to GUI - treeview
                    gw.mainForm.progress.Report(new UpdateSignal
                    {
                        Action = UpdateSignal.UpdateAction.TerminatedProcesses,
                        LogMessage = messageToLog
                    });


                }

                //if process hasn't related treenode, it's a new process
                if (process.treenode == null)
                {
                    //invia l'aggiornamento alla GUI per aggiornare il LogView
                    messageToLog = ($"process added to tracking: {process.ProcessName} ({process.processID})");
                    gw.mainForm.progress.Report(new UpdateSignal
                    {
                        Action = UpdateSignal.UpdateAction.NewLogMessage,
                        LogMessage = messageToLog
                    });


                    //add to temporary list
                    newProcesses.TryAdd(process.processID, process);

                     
                }
            }
            
            //if new processes are present
            if(newProcesses.Count>0)
            {
                //send update to GUI (logview)
                gw.mainForm.progress.Report(new UpdateSignal
                {
                    Action = UpdateSignal.UpdateAction.CreatedProcesses,
                    NewProcessesList = newProcesses
                });
            }

        }


        //metodo usato dalla GUI per estrarre tutte le occorrenze dei TAG
        public string getAllYarasDetected()
        {
            HashSet<Yara> filteredYaras = new HashSet<Yara>();

            foreach (var process in trackingProcessesList.Values)
            {
                foreach (var yara in process.matchedYaraRules )
                {
                    filteredYaras.Add(yara);
                }
               
            }
            StringBuilder yaraTagList = new StringBuilder();
            //trasformo filteredYaras in stringa
            foreach (Yara yara in filteredYaras)
            {
                if (yaraTagList.Length > 0)
                {
                    yaraTagList.Append(", ");
                }
                yaraTagList.Append(yara.Name);
            }

            return yaraTagList.ToString();
        }




        //questo metodo serve per aggiungere i processi che sono in corso di Tracking
        public void AddToTrackedProcess(uint pid, ProcessInfoTracked process)
        {
            //if i'm tracking this process, (Segugio) skip it
            if (pid.Equals(gw.thisCurrentProcessPid))
            {

            }
            else
            {
                //se il processo non è ancora presente nella lista
                if (!(trackingProcessesList.ContainsKey(pid)))
                {



                    //CASO DEL PID INIZIALE
                    if (startingPidHasNotified && pid.Equals(gw.pl.launchedProcessPid))
                    {
                        process.isStartingProcess = true;

                        //add pid to trackedlist
                        //aggiungi il parent alla lista dei processi creati
                        if (mainProcessList.TryGetValue(process.ParentPID, out ProcessInfo parentProcess))
                        {
                            ProcessInfoTracked trackedParentProcess = new ProcessInfoTracked(
                                parentProcess.ParentPID,
                                parentProcess.ProcessName,
                                parentProcess.processID);

                            AddToTrackedProcess(process.ParentPID, trackedParentProcess);

                        }

                        //reset startingPidHasNotified
                        this.startingPidHasNotified = false;
                    }


                    //add more properties solo per i processi che dovranno essere ispezionati
                    //in questo modo la lista dei processi è più snella
                    try
                    {
                        process.process = Process.GetProcessById((int)pid, process.machineName);
                        process.isRunning = true;
                    }
                    catch (Exception ex)
                    {
                        //caso in cui il processo non è in esecuzione
                        process.isRunning = false;
                    }

                    if (process.isRunning)
                    {
                        //check permissions
                        process.CanAttachToProcess(pid);

                        if (process.canBeAttached)
                        {
                            System.Diagnostics.Debug.WriteLine(" asking commandline for " + process.ProcessName);
                            process.GetCommandLine();

                            process.getProcessIcon();
                        }



                        //se il processo parent non esiste nella lista di processi monitorati
                        if (!mainProcessList.ContainsKey(process.ParentPID))
                        {
                            //allora è un root
                            process.isRootProcess = true;
                        }
                        else
                        {
                            //se esiste controlla che non sia terminato
                            if (process.isParentPidRunning)
                            {
                                // se è in esecuzione allora non è un root process
                                process.isRootProcess = false;

                                ////aggiungi il processo
                                //AddToTrackedProcess(process.ParentPID, new ProcessInfoTracked(
                                //    mainProcessList[process.ParentPID].ParentPID,
                                //    mainProcessList[process.ParentPID].ProcessName,
                                //    mainProcessList[process.ParentPID].processID));

                            }
                            else
                            {
                                //aggiungilo come root process
                                process.isRootProcess = true;
                            }
                        }

                        //process.isRunning = true;

                        //prepara le strutture per eventuali detection
                        process.matchedYaraRules = new HashSet<Yara>();
                        process.dictMalwareConfiguration = new Dictionary<Yara, string>();


                        trackingProcessesList.TryAdd(pid, process);
                    }
                    else
                    {
                        //se il processo non sta runnando
                        //metti un'icona vuota
                        process.getDefaultIcon();

                        //metti una commandline vuota
                        process.CommandLine = "";

                        //aggiungilo alla lista dei processi tracciati
                        trackingProcessesList.TryAdd(pid, process);


                    }



                }
                else
                {
                    //L'aggiornamento della proprietà dei processi deve avvenire dalla classe ProcessScan
                }
            }
            

        }


        //public void addAttributionToProcess(uint processID, Yara matchYara)
        //{
        //    if(TrackingProcessesList.ContainsKey(processID))
        //    {
        //        TrackingProcessesList[processID].addYaraDetection(matchYara);
        //    }
        //}

        //public void addConfigurationToProcess(uint processID, Yara matchYara, string configuration)
        //{
        //    if (TrackingProcessesList.ContainsKey(processID))
        //    {
        //        TrackingProcessesList[processID].addMalwareConfiguration(matchYara.Name, configuration);
        //    }
        //}



       


        //Questo metodo avvia la ricerca e restituisce tutti i figli di un dato processo
        //tutti i figli li mette in un dictionary TrackingProcessesList che è gestito da ProcessListManager
        public void searchAllNewProcessesesByLaunchedProcess(uint startingPID)
        {
            //aggiunge il processo appena creato 
            //ottengo l'istanza del processo creato
            if (mainProcessList.TryGetValue(startingPID, out ProcessInfo startingProcess))
            {
                AddToTrackedProcess(startingPID, new ProcessInfoTracked(
                            startingProcess.ParentPID,
                            startingProcess.ProcessName,
                            startingProcess.processID));

                findChildRecursiveOfStartingProcess(startingPID);
            }

            //adesso controlla tutti i processi creati a partire dal parent del processo creato (explorer.exe)
            //a partire dall'iniizo della scansione
            //FindChildrenRecursive(gw.pl.parentPidLaunchedProcess, gw.ps.scanningStartTime);

            FindAllNewProcessesAfter(gw.ps.scanningStartTime);

        }
        private void FindAllNewProcessesAfter(DateTime startTime)
        {
            foreach (var kvp in mainProcessList.Values)
            {

                if(kvp.startingProcessTime.HasValue && kvp.startingProcessTime.Value > startTime)
                {
                    AddToTrackedProcess(kvp.processID, new ProcessInfoTracked(
                            kvp.ParentPID,
                            kvp.ProcessName,
                            kvp.processID));
                   
                }

            }
        }




        private void FindChildrenRecursive(uint parentPid, DateTime startTime)
        {
            foreach (var kvp in mainProcessList.Values)
            {

                if (kvp.ParentPID == parentPid && kvp.startingProcessTime.HasValue && kvp.startingProcessTime.Value > startTime)
                {
                    AddToTrackedProcess(kvp.processID, new ProcessInfoTracked(
                            kvp.ParentPID,
                            kvp.ProcessName,
                            kvp.processID));
                    FindChildrenRecursive(kvp.processID, startTime);  // Ricorsivamente trova i figli
                }
                
            }
        }

        //Metodo ricorsivo privato per individuare solo i nodi figli del processo di partenza
        private void findChildRecursiveOfStartingProcess(uint parentPid)
        {
            foreach (var process in gw.plm.mainProcessList.Values)
            {
                //se l'attuale processo è figlio del PID specificato allora aggiungilo
                if (process.ParentPID.Equals(parentPid))
                {
                    // Aggiungi questo processo figlio se non è già presente
                    if (!trackingProcessesList.ContainsKey(process.processID))
                    {
                        AddToTrackedProcess(process.processID, new ProcessInfoTracked(
                            process.ParentPID,
                            process.ProcessName,
                            process.processID));

                        // Ricorsivamente trova e aggiungi i discendenti di questo processo figlio
                        findChildRecursiveOfStartingProcess(process.processID);
                    }
                }
            }
        }






        //get Pid of a process
        public uint? FindPidForProcessName(string processName)
        {
            foreach (var item in mainProcessList)
            {
                if (item.Value.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Key; // Restituisce il PID quando trova una corrispondenza
                }
            }

            return null; // Restituisce null se non trova corrispondenze
        }





        public ProcessInfo getProcessByPID(uint pid)
        {
            if(mainProcessList.TryGetValue((uint)pid, out ProcessInfo pInfo) )
            {
                return pInfo;
            }
            else
            {
                return null;
            }
        }


        //public TreeNode createNewTreeNode(ProcessInfo newProcess)
        //{
        //    TreeNode newTreenode = new TreeNode($"{newProcess.ProcessName} (PID: {newProcess.processID}) - " + (newProcess.isRunning ? "active" : "terminated"));

        //    if (mainProcessList.ContainsKey(newProcess.processID))
        //    {
        //        // Modifica solo alcune proprietà dell'oggetto ProcessInfo esistente
        //        mainProcessList[newProcess.processID].treenode = newTreenode;
        //    }


        //    return newTreenode;
        //}

        //public string updateTreeNode(ProcessInfo processInfo)
        //{
        //    string value = $"{processInfo.ProcessName} (PID: {processInfo.processID}) - " + (processInfo.isRunning ? "active" : "terminated");
        //    return value;
        //}

        //// Restituisce tutti gli oggetti ProcessInfo con un certo ParentPID da una lista currentList
        public Dictionary<uint, ProcessInfoTracked> getChildProcessesFromPID(uint parentPID)
        {
            Dictionary<uint, ProcessInfoTracked> result = new Dictionary<uint, ProcessInfoTracked>();

            foreach (var process in trackingProcessesList)
            {
                if (process.Value.ParentPID == parentPID)
                {
                    result.Add(process.Key, process.Value);
                }
            }

            return result;
        }


        // Restituisce tutti gli oggetti ProcessInfo che non hanno un pid esistente
        //public void getRootProcesses()
        //{
        //    foreach (var item in mainProcessList)
        //    {
        //        // Controlla se il ParentPID di questo processo esiste come processID in dictProcPid
        //        if (!mainProcessList.ContainsKey(item.Value.ParentPID) || item.Value.ParentPID == 0)
        //        {
        //            if (!(dictRootPIDs.ContainsKey(item.Key)))
        //            {
        //                // Se non esiste, il processo è radice
        //                dictRootPIDs.TryAdd(item.Key, item.Value);
        //            }

        //        }
        //    }

        //    //update counter
        //    this.numberRootProcesses = dictRootPIDs.Count;

        //}


        ////method for avoiding interference with processes that are spawned for
        ////scanning purposes, like yara or python
        //public List<uint> GetAllDescendantPIDs(uint parentPid)
        //{
        //    System.Diagnostics.Debug.WriteLine($"{numberRootProcesses} processes without parent ");

        //    List<uint> descendantPIDs = new List<uint>();

        //    // Cerca tutti i processi che hanno parentPid come genitore
        //    foreach (var process in dictProcPid)
        //    {
        //        if (process.Value.ParentPID == parentPid)
        //        {
        //            // Aggiunge il PID del processo figlio alla lista
        //            descendantPIDs.Add(process.Key);

        //            process.Value.isDescendantSegugio = true;

        //            // Ricorsivamente trova e aggiunge tutti i discendenti del processo figlio
        //            descendantPIDs.AddRange(GetAllDescendantPIDs(process.Key));
        //        }
        //    }

        //    return descendantPIDs;
        //}


        //public void filterDescendantsOfSegugio()
        //{
        //    List<uint> childPIDofSegugio = GetAllDescendantPIDs((uint)thisPid);

        //    System.Diagnostics.Debug.WriteLine($"processi da rimuovere :" + childPIDofSegugio.Count + 1);


        //    foreach (var child in childPIDofSegugio)
        //    {
        //        dictProcPid.TryRemove(child, out ProcessInfo removedProcess);
        //        dictCurrentProcPid.Remove(child);
        //    }

        //    //rimuove segugio.exe
        //    dictProcPid.TryRemove((uint)thisPid, out ProcessInfo removed);
        //    dictCurrentProcPid.Remove((uint)thisPid);
        //}

        //public void getTerminatedProcesses()
        //{
        //    foreach (var item in dictProcPid)
        //    {
        //        if (!item.Value.isRunning)
        //        {
        //            if (!dictNewTerminatedProcesses.ContainsKey(item.Key))
        //            {
        //                dictRootPIDs.TryAdd(item.Key, item.Value);
        //            }
        //            elsec
        //            {
        //                //update the corresponding value to key
        //                dictRootPIDs[item.Key] = item.Value;
        //            }
        //        }
        //    }
        //}



        public void AddTreeNodeToProcess(uint pid, ProcessTreeNode node)
        {
            if(this.trackingProcessesList.ContainsKey(pid) )
            {
                this.trackingProcessesList[pid].treenode = node;
            }
            
        }


    }
}
