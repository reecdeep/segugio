using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{

    public class ProcessTreeNode : TreeNode
    {
        public Icon ProcessIcon { get; set; }
        public string ProcessName { get; set; }
        public uint PID { get; set; }
        public uint parentPID { get; set; }
        public string Status { get; set; } //process status, "Running" or "Terminated"
        public string CommandLine { get; set; }
        public int numberOfMatchedYaraRules { get; set; }
        public int numberOfMatchedConfigExtracted { get; set; }

        public HashSet<Yara> matchedYaraRules;
        public Dictionary<Yara, String> configurations;

        public ProcessTreeNode(ProcessInfoTracked trackedProcess)
        {
            ProcessName = trackedProcess.ProcessName;
            PID = trackedProcess.processID;
            CommandLine = trackedProcess.CommandLine;
            Text = $"{ProcessName} [{PID}] - {Status}"; // Default text
            parentPID = trackedProcess.ParentPID;
            ProcessIcon = trackedProcess.processIcon;
            updateRunningStatus(trackedProcess);

        }

        public void updateRunningStatus(ProcessInfoTracked trackedProcess)
        {
            if (trackedProcess.isRunning)
            {
                Status = "Running";
            }
            else
            {
                Status = "Not Running";
            }

            if(trackedProcess.matchedYaraRules.Count>0)
            {
                //number of matched yara Rules
                numberOfMatchedYaraRules = trackedProcess.matchedYaraRules.Count;
            }
            else
            {
                numberOfMatchedYaraRules = 0;
            }

            
            matchedYaraRules = trackedProcess.matchedYaraRules;

            //number of extracted Configus
            numberOfMatchedConfigExtracted = trackedProcess.dictMalwareConfiguration.Count;
            configurations = trackedProcess.dictMalwareConfiguration;
        }

    }

    public class ProcessTreeView
    {
        private ContextMenuStrip treeContextMenu;
        String div = "--------------------------------------------------\r\n";

        TreeView tv;
        public ProcessTreeView(TreeView treeView)
        {
            this.tv = treeView;
            ConfigureTreeView(treeView);

            //contextual menu
            CreateContextMenu(); 
        }

        private void ConfigureTreeView(TreeView treeView)
        {
            treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
            treeView.DrawNode += OnDrawNode;
            treeView.ItemHeight = 64;
            treeView.NodeMouseClick += TreeView1_NodeMouseClick;
        }

        private void CreateContextMenu()
        {
            treeContextMenu = new ContextMenuStrip();
            treeContextMenu.Items.Add("Show CommandLine", null, showCommandLine);
            treeContextMenu.Items.Add("Show Detection Details", null, showYaraDetails);
            treeContextMenu.Items.Add("Show Extracted Config", null, showExtractedConfig);
        }

        private void TreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tv.SelectedNode = e.Node;
                treeContextMenu.Show(tv, e.Location);
            }
        }

        private void showCommandLine(object sender, EventArgs e)
        {
            if (tv.SelectedNode != null)
            {
                // Creazione del form 
                FormContextMenu commandLineForm = new FormContextMenu();
                commandLineForm.Text = "Show CommandLine";

                ProcessTreeNode selectedNode = (ProcessTreeNode) tv.SelectedNode;
                String content = $"Process: {selectedNode.ProcessName} [{selectedNode.PID}]\r\n";
                content = content + $"CommandLine: {selectedNode.CommandLine}";
                commandLineForm.contenutoBox.Text = content;  


                commandLineForm.closeButton.Click += (s, args) => { commandLineForm.Close(); };
               

                // show the form
                commandLineForm.ShowDialog();
            }
        }

        private void showYaraDetails(object sender, EventArgs e)
        {
            if (tv.SelectedNode != null)
            {
                ProcessTreeNode selectedNode = (ProcessTreeNode)tv.SelectedNode;
                if (selectedNode.numberOfMatchedYaraRules>0)
                {
                    // create form 
                    FormContextMenu detectionDetailsForm = new FormContextMenu();
                    detectionDetailsForm.Text = "Show CommandLine";

                    
                    String content = $"Process: {selectedNode.ProcessName} [{selectedNode.PID}]\r\n";
                    content = content + $"Matched yara rule(s): {selectedNode.numberOfMatchedYaraRules}\r\n";
                    content = content + div;
                    int countingYara = 0;

                    foreach (Yara yaraMatched in selectedNode.matchedYaraRules)
                    {
                        content = content + $"Name: {yaraMatched.Name}\r\n";
                        content = content + $"Description: {yaraMatched.Description}\r\n";
                        content = content + $"Rule path: {yaraMatched.FilePath}\r\n";
                        countingYara++;

                        if(countingYara< selectedNode.numberOfMatchedYaraRules)
                        {
                            //insert div
                            content = content + div;
                        }
                    }

                    detectionDetailsForm.contenutoBox.Text = content;  
                    detectionDetailsForm.closeButton.Click += (s, args) => { detectionDetailsForm.Close(); };


                    //show the form
                    detectionDetailsForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show($"No Yara matched this process!");
                }

            }
        }

        private void showExtractedConfig(object sender, EventArgs e)
        {
            ProcessTreeNode selectedNode = (ProcessTreeNode)tv.SelectedNode;
            if (selectedNode.numberOfMatchedConfigExtracted > 0)
            {
                // create form 
                FormContextMenu configurationDetailsForm = new FormContextMenu();
                configurationDetailsForm.Text = "Show Extracted Config";


                String content = $"Process: {selectedNode.ProcessName} [{selectedNode.PID}]\r\n";
                content = content + $"Matched yara rule(s): {selectedNode.numberOfMatchedYaraRules}\r\n";
                content = content + div;
                int countingYara = 0;

                foreach (var yaraMatched in selectedNode.configurations)
                {
                    content = content + $"Name: {yaraMatched.Key.Name}\r\n";
                    content = content + $"Description: {yaraMatched.Key.Description}\r\n";
                    content = content + $"Config extracted: \r\n {yaraMatched.Value}\r\n";
                    countingYara++;

                    if (countingYara < selectedNode.numberOfMatchedYaraRules)
                    {
                        //insert div
                        content = content + div;
                    }
                }

                configurationDetailsForm.contenutoBox.Text = content;  // Preimposta il testo del nodo selezionato nella TextBox
                configurationDetailsForm.closeButton.Click += (s, args) => { configurationDetailsForm.Close(); };


                // Mostra il form in modalità dialogo
                configurationDetailsForm.ShowDialog();
            }
            else
            {
                MessageBox.Show($"No configuration extracted for this process!");
            }
        }

        private void OnDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            ProcessTreeNode processNode = e.Node as ProcessTreeNode; // Assicurati che il nodo sia del tipo corretto

            if (processNode != null)
            {

                // Calcola l'area di selezione
                Rectangle nodeBounds = new Rectangle(e.Bounds.X, e.Bounds.Y, tv.Width, 64);


                // Evidenzia l'area di selezione
                if ((e.State & TreeNodeStates.Selected) != 0)
                {
                    e.Node.BackColor = Color.White;
                    e.Node.ForeColor = Color.White;
                    e.Graphics.FillRectangle(Brushes.LightBlue, nodeBounds);
                }
                else
                {
                    e.Node.BackColor = Color.White;
                    e.Node.BackColor = Color.White;
                    e.Graphics.FillRectangle(Brushes.White, nodeBounds);
                }

                // Definisci le metriche base per il disegno
                Font font = e.Node.TreeView.Font;
                int lineHeight = (int)font.GetHeight() + 2; // Altezza di una riga di testo + un piccolo spazio
                int iconSize = 64; 
                Point drawPoint = new Point(e.Bounds.Left, e.Bounds.Top);

                // Disegna l'icona del processo (se disponibile)
                if (processNode.ProcessIcon != null)
                {
                    e.Graphics.DrawIcon(processNode.ProcessIcon, new Rectangle(drawPoint, new Size(iconSize, iconSize)));
                }

                //draw informations
                drawPoint.X += iconSize + 4; //shift right for Icon size

                // first row: process name - process status 
                String firstLine = $"{processNode.ProcessName} [{processNode.PID}] - {processNode.Status}";
                e.Graphics.DrawString(firstLine, font, Brushes.Black, drawPoint);
                drawPoint.Y += lineHeight;  // shift downwards for next line

                // second row: Command line
                e.Graphics.DrawString(processNode.CommandLine, font, Brushes.Gray, new PointF(drawPoint.X, drawPoint.Y));
                drawPoint.Y += lineHeight; // shift downwards for next line

                // third row: YARA RULE 
                Brush square1Brush;
                String matchedYaras;
                if (processNode.numberOfMatchedYaraRules>0)
                {
                    matchedYaras = "matched yara(s): ";
                    square1Brush  = Brushes.Red;
                    foreach(var yara in processNode.matchedYaraRules)
                    {
                        matchedYaras = matchedYaras + yara.Name + ", ";
                    }
                }
                else
                {
                    square1Brush = Brushes.Blue;
                    matchedYaras = "no matched yaras";
                }

                // first square - config found
                e.Graphics.FillRectangle(square1Brush, drawPoint.X, drawPoint.Y, 10, 10); 
                e.Graphics.DrawString(matchedYaras, font, Brushes.Black, new PointF(drawPoint.X+15, drawPoint.Y));
                drawPoint.Y += lineHeight; // shift downwards for next line

                // fourth row: config extraction 
                Brush square2Brush;
                String extractedConfigs;
                if (processNode.numberOfMatchedConfigExtracted > 0)
                {
                    extractedConfigs = "extracted config(s): ";
                    square2Brush = Brushes.Red;
                    foreach (var config in processNode.configurations)
                    {
                        extractedConfigs = extractedConfigs + config.Key.Name + ", ";
                    }
                }
                else
                {
                    square2Brush = Brushes.Blue;
                    extractedConfigs = "no config extracted";
                }

                // second square - config found
                e.Graphics.FillRectangle(square2Brush, drawPoint.X, drawPoint.Y, 10, 10); 
                e.Graphics.DrawString(extractedConfigs, font, Brushes.Black, new PointF(drawPoint.X + 15, drawPoint.Y));


                //compute dimensions of elements
                SizeF firstLineSize = e.Graphics.MeasureString(firstLine, font);
                SizeF commandLineSize = e.Graphics.MeasureString(processNode.CommandLine, font);
                SizeF yaraSize = e.Graphics.MeasureString(extractedConfigs, font);
                SizeF configSize = e.Graphics.MeasureString(extractedConfigs, font);


                

                



            }
        }

      


        

    }
}




