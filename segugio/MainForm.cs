using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{
    public partial class MainForm : Form
    {

        Gateway gw;
        string processToOpenPath = "";
        HashSet<uint> visitedProcesses;

        private ProcessTreeView _treeViewManager;

        //flag per determinare se è stata disegnata la prima lista processi
        public volatile Boolean firstProcessTreeCreationIsMade = false;

        //flag per determinare se sto aggiornando la lista processi
        public volatile Boolean processTreeIsUpdating = false;

        //signaling
        public IProgress<UpdateSignal> progress;

        public MainForm(Gateway gw)
        {
            this.gw = gw;
            InitializeComponent();
            _treeViewManager = new ProcessTreeView(this.treeView1);

            visitedProcesses = new HashSet<uint>();


            progress = new Progress<UpdateSignal>(updateSignal =>
            {
                switch (updateSignal.Action)
                {
                    case UpdateSignal.UpdateAction.Idle: //Idle when first process Monitor loop has completed 
                        programStatusBox.Text = "Idle";
                        scannedProcessesBox.Text = gw.plm.trackingProcessesList.Count().ToString();
                        //update parent process combobox
                        updateParentProcessesComboList();

                        //update commandline combobox
                        updateCommandLineComboList();

                        //update log
                        writeMessageLog("Ready!");
                        break;

                    case UpdateSignal.UpdateAction.ProcessScanStarted: //initialize on process scan
                        programStatusBox.Text = "Running";
                        scannedProcessesBox.Text = gw.plm.trackingProcessesList.Count().ToString();
                        createProcessTreeView();
                        break;

                    case UpdateSignal.UpdateAction.CreatedProcesses:
                        scannedProcessesBox.Text = gw.plm.trackingProcessesList.Count().ToString();
                        UpdateTreeViewNodes(updateSignal.NewProcessesList);
                        break;

                    case UpdateSignal.UpdateAction.TerminatedProcesses:
                        // Gestisci l'aggiornamento di un processo esistente
                        // Ad esempio, potresti voler aggiornare il nodo esistente nel TreeView
                        updateProcessTreeView();
                        break;
                    case UpdateSignal.UpdateAction.UpdatedProcess:
                        // Gestisci l'aggiornamento di un processo esistente
                        // Ad esempio, potresti voler aggiornare il nodo esistente nel TreeView
                        yarasMatchedBox.Text = gw.plm.getAllYarasDetected();
                        updateProcessTreeView();
                        break;
                    case UpdateSignal.UpdateAction.NewLogMessage:
                        writeMessageLog(updateSignal.LogMessage);
                        break;


                }

            });

            this.FormClosing += MainForm_FormClosing;

        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            gw.Stop();
        }

        





        private void button_Click_launchFileStartScan(object sender, EventArgs e)
        {
            //if no scan is occurring
            if (!gw.ps.scanHasStarted)
            {

                button_chooseFile.Enabled = false;
                button_Launch.Enabled = false;
                button_refresh.Enabled = false;

                comboCommandLine.Enabled = false;
                comboProcessList.Enabled = false;

                


                //pass to process launch thread arguments
                gw.TriggerProcessLaunchThread(processToOpenPath, "");
            }
           

        }


        public void updateCommandLineComboList()
        {

            //disable object
            this.comboCommandLine.Enabled = false;

            ////delete all combolist process
            this.comboCommandLine.Items.Clear();

            String[] commandLines = gw.settings.GetSetting("DefaultCommandlines").Split('|');
            foreach (string commandLine in commandLines)
            {
                comboCommandLine.Items.Add(commandLine);
            }

            //enable object
            this.comboCommandLine.Enabled = true;

        }




        //ComboBox to populate the list of possible processes to use as parent of the process to launch
        //(via pidSpoofing)
        public void updateParentProcessesComboList()
        {
            //disable object
            this.comboProcessList.Enabled = false;

            ////delete all combolist process
            this.comboProcessList.Items.Clear();

            //get processList
            Dictionary<uint, String> onlyRunningProcessList = gw.plm.UpdateAndGetOnlyRunningProcessList();
            // System.Diagnostics.Debug.WriteLine($"onlyRunningProcessLists... {onlyRunningProcessList.Count()}");


            // Associate the dictionary with the ComboBox
            this.comboProcessList.DataSource = new BindingSource(onlyRunningProcessList, null);
            this.comboProcessList.DisplayMember = "Value";
            this.comboProcessList.ValueMember = "Key";

            //find the preferred process index
            String preferredParentProcess = gw.settings.GetSetting("PreferredParentProcess");
            int index = gw.ut.FindIndexWithText(this.comboProcessList, preferredParentProcess);


            if (index != -1)
            {
                

                this.comboProcessList.SelectedIndex = index;
            }
            else
            {
                //failover to explorer.exe
                index = gw.ut.FindIndexWithText(this.comboProcessList, "explorer.exe");
                this.comboProcessList.SelectedIndex = index;
            }


            //enable object
            this.comboProcessList.Enabled = true;

        }

        public void writeMessageLog(string message)
        {
            DateTime now = DateTime.Now;

            // Convert to string including milliseconds
            string formattedDate = now.ToString("HH:mm:ss.fff");
            this.logViewer.AppendText($"{formattedDate}  {message} {Environment.NewLine}");
            this.logViewer.ScrollToCaret();

        }

        private void button_Click_chooseFile(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    
                    processToOpenPath = openFileDialog.FileName;
                    fileChooser.Text = processToOpenPath;
                }

                if(processToOpenPath.Length>0)
                {
                    //enable launch button
                    button_Launch.Enabled = true;
                }
            }
        }


       


        public bool HasParentInDictionary(uint processId,
            ConcurrentDictionary<uint, ProcessInfoTracked> processDict,
            HashSet<uint> visited)
        {
            // Avoid infinite loops by checking if we have already visited this process
            if (visited.Contains(processId))
            return false;

            visited.Add(processId);

            // Find the current process in the dictionary
            if (processDict.TryGetValue(processId, out ProcessInfoTracked currentProcess))
            {
                // Check if the parent PID is present in the dictionary
                if (processDict.ContainsKey(currentProcess.ParentPID))
                    return true; // Il genitore è nel dizionario

                // Otherwise, recursively check if the parent has a parent in the dictionary
                return HasParentInDictionary(currentProcess.ParentPID, processDict, visited);
            }

            return false; // The process with this ID is not in the dictionary
        }



        private void UpdateTreeViewNodes(ConcurrentDictionary<uint, ProcessInfoTracked> processiNuoviDaAggiungere)
        {
            //aggiornamento flag lista processi iniziato
            processTreeIsUpdating = true;

            //mi creo una lista di tutti i nodi disponibili 
            ConcurrentDictionary<uint, ProcessTreeNode> dictExistingNodes = new ConcurrentDictionary<uint, ProcessTreeNode>();
            foreach (ProcessTreeNode rootNode in treeView1.Nodes)
            {
                dictExistingNodes.TryAdd(rootNode.PID, rootNode); // Aggiunge il nodo radice
                AddChildNodes(rootNode, dictExistingNodes); // Aggiunge tutti i nodi figli ricorsivamente
            }


            int countAddedProcessToDictionary = 0;

            //Preliminary check to add any root nodes
            HashSet<uint> visitedProcesses = new HashSet<uint>();

            //for all running processes, check they are root
            foreach (var runningProcess in gw.plm.trackingProcessesList.Values)
            {
                //if the process has no parents
                if (!(HasParentInDictionary(runningProcess.processID,
                    gw.plm.trackingProcessesList,
                    visitedProcesses)))
                {
                    //if the process is not present in the current list of existing nodes
                    if (!(dictExistingNodes.ContainsKey(runningProcess.processID)))
                    {
                        //then add it as root node to the list of existing nodes
                        ProcessTreeNode newRootNode = new ProcessTreeNode(runningProcess);
                        runningProcess.treenode = newRootNode;

                        //add it to the treeview
                        treeView1.Nodes.Add(runningProcess.treenode);
                        countAddedProcessToDictionary++;

                        //update the node list
                        dictExistingNodes.TryAdd(runningProcess.processID, runningProcess.treenode);
                    }
                }
            }


            //now add any root process nodes
            while (countAddedProcessToDictionary < processiNuoviDaAggiungere.Count)
            {
                foreach (var existingNode in dictExistingNodes)
                {
                    foreach (var process in processiNuoviDaAggiungere.Values)
                    {
                        //if the existing node is a parent node of some process
                        if (existingNode.Value.PID.Equals(process.ParentPID))
                        {
                            //and if the child node (process) is not present inside dictNodiEsistenti
                            if (!dictExistingNodes.ContainsKey(process.processID))
                            {
                                ProcessTreeNode newNode = new ProcessTreeNode(process);


                                //get the node from the process list
                                if (gw.plm.trackingProcessesList.TryGetValue(process.processID, out ProcessInfoTracked currentProcess))
                                {
                                    currentProcess.treenode = newNode;
                                    //then insert the node
                                    existingNode.Value.Nodes.Add(currentProcess.treenode);

                                    //update
                                    dictExistingNodes.TryAdd(currentProcess.processID, currentProcess.treenode);

                                    //increment
                                    countAddedProcessToDictionary++;

                                }
                            }
                        }
                    }
                }
            }

            // expand all processes
            this.treeView1.ExpandAll();


            //Process list flag update completed
            processTreeIsUpdating = false;

        }


        private void AddChildNodes(ProcessTreeNode parentNode, ConcurrentDictionary<uint, ProcessTreeNode> allNodes)
        {
            foreach (ProcessTreeNode childNode in parentNode.Nodes)
            {
                allNodes.TryAdd(childNode.PID, childNode); // Aggiunge il nodo figlio corrente
                AddChildNodes(childNode, allNodes); // Ricorsione per aggiungere altri sottonodi
            }
        }


        public void createProcessTreeView()
        {


            this.treeView1.Nodes.Clear();

            //start iterating RootPids
            foreach (var currentProcess in gw.plm.trackingProcessesList.Values)
            {
                //if the current process is the parent of the launched process
                if (currentProcess.processID.Equals(gw.pl.launchedProcessParentProcessPid))
                {

                    ProcessTreeNode newNode = new ProcessTreeNode(currentProcess);

                    //add to list of currently tracked processes
                    currentProcess.treenode = newNode;

                    //add to tree
                    this.treeView1.Nodes.Add(newNode);

                    // Avoid passing the entire list to prevent redundant searches and potential circular references
                    AddChildProcessesRecursively(currentProcess.treenode, currentProcess.processID);

                }

            }

            // expand all processes
            this.treeView1.ExpandAll();
            //empty visitedProcesses
            visitedProcesses.Clear();

            if (this.treeView1.Nodes.Count>0)
            {
                this.treeView1.SelectedNode = this.treeView1.Nodes[0];

            }

            //update flag created process list
            firstProcessTreeCreationIsMade = true;

        }






        private void AddChildProcessesRecursively(TreeNode parentNode, uint currentProcess)
        {

            // Avoid infinite recursion
            if (visitedProcesses.Contains(currentProcess)) return;


            visitedProcesses.Add(currentProcess);

            Dictionary<uint, ProcessInfoTracked> childProcesses = gw.plm.getChildProcessesFromPID(currentProcess);

            foreach (var childProc in childProcesses.Values)
            {
                //create new treenode
                ProcessTreeNode newNode = new ProcessTreeNode(childProc);

                //add to list of currently tracked processes
                gw.plm.AddTreeNodeToProcess(childProc.processID, newNode);

                //add to tree
                parentNode.Nodes.Add(childProc.treenode);


                AddChildProcessesRecursively(childProc.treenode, childProc.processID);
            }
        }



        public void updateProcessTreeView()
        {
            //process list flag update started
            processTreeIsUpdating = true;

            //now update the information for every node
            foreach (var currentProcess in gw.plm.trackingProcessesList.Values)
            {
                if( currentProcess.treenode != null)
                {
                    currentProcess.treenode.updateRunningStatus(currentProcess);

                    Rectangle nodeRect = currentProcess.treenode.Bounds;
                    treeView1.Invalidate(nodeRect);
                } 

            }

            //process list flag update finished
            processTreeIsUpdating = false;

        }

        private void button_refresh_Click(object sender, EventArgs e)
        {
            updateParentProcessesComboList();
        }

        private void comboProcessList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected item
            if (this.comboProcessList.SelectedIndex != -1)
            {
                var selectedPair = (KeyValuePair<uint, string>)this.comboProcessList.SelectedItem;

                //set info to ProcessLaunch
                gw.pl.launchedProcessParentProcessPid = selectedPair.Key; ;
                gw.pl.launchedProcessParentProcessName = selectedPair.Value;

            }
        }

        private void ComboBox_Format(object sender, ListControlConvertEventArgs e)
        {
            // Get the key-value pair
            KeyValuePair<uint, string> kvp = (KeyValuePair<uint, string>)e.ListItem;
            // Set the displayed text as "Key - Value"
            e.Value = $"{kvp.Value} - ({kvp.Key})";
        }

        private void comboCommandLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected item
            if (this.comboCommandLine.SelectedIndex != -1)
            {

                //set info to ProcessLaunch
                gw.pl.customCommandLine = comboCommandLine.SelectedItem.ToString();

            }
        }

        private void comboCommandLine_TextChanged(object sender, EventArgs e)
        {
            // Get the current text of the ComboBox
            gw.pl.customCommandLine = comboCommandLine.Text;
        }

    }

}
