using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{
    public class ConfigExtraction
    {

        public Dictionary<string, string> dictConfigExtractors = new Dictionary<string, string>();

        MainForm form;

        string pyhtonExecutablePath;
        string configExtractorsDirectory;
        int numberOfConfigExtractors = 0;
        int noOfScriptWithoutNameCounter = 0; //counter for rule without a name

        Gateway gw;

        public ConfigExtraction(Gateway gw)
        {
            this.gw = gw;
            this.form = gw.mainForm;

            //define python.exe path
            pyhtonExecutablePath = gw.settings.GetSetting("PythonExecutablePath");

            //define config extractors dir
            configExtractorsDirectory = gw.settings.GetSetting("ConfigExtractorsDirectory");

            //check paths
            gw.ut.CheckPaths("file",pyhtonExecutablePath);
            gw.ut.CheckPaths("directory",configExtractorsDirectory);

            //load pyhton scripts
            LoadConfigExtractorScripts();
        }



        public Boolean existsConfigExtractorByRule(string matchedYaraRule)
        {
            if (this.dictConfigExtractors.TryGetValue(matchedYaraRule, out string pythonScriptPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string getConfigExtractorPathByRule(string matchedYaraRule)
        {
            if (this.dictConfigExtractors.TryGetValue(matchedYaraRule, out string pythonScriptPath))
            {
                return pythonScriptPath;
            }
            else
            {
                return null;
            }
            
        }

        private void LoadConfigExtractorScripts()
        {
           

                foreach (string pyFile in Directory.GetFiles(configExtractorsDirectory, "*.py"))
                {

                    String pyScriptName = ExtractScriptName(pyFile);

                    //add to dict (NO DUPLICATES)
                    if (!dictConfigExtractors.ContainsKey(pyScriptName))
                    {
                        dictConfigExtractors.Add(pyScriptName, pyFile);
                    }

                }

                numberOfConfigExtractors = dictConfigExtractors.Count;
                string message = $"succesfully loaded {numberOfConfigExtractors} py scripts";
                gw.mainForm.progress.Report(new UpdateSignal
                {
                    Action = UpdateSignal.UpdateAction.NewLogMessage,
                    LogMessage = message
                });
           


            
        }



        //method for launching a python script once the process has been tegged malicious to 
        //extract the configuration
        public (bool, string) pyExecConfScript(string scriptPath, string dumpFileName)
        {
            Boolean configFound = false;
            System.Diagnostics.Debug.WriteLine($"launching PYTHON SCRIPT at: {scriptPath}");
            string result = "";
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = pyhtonExecutablePath; 
                start.Arguments = string.Format("\"{0}\" \"{1}\"", scriptPath, dumpFileName);
                start.UseShellExecute = false; 
                start.CreateNoWindow = true; 
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true; 

                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd(); 


                        System.Diagnostics.Debug.WriteLine($"output script {result}");


                    }
                    //using (StreamReader reader = process.StandardError)
                    //{
                    //    result = reader.ReadToEnd();
                    //    //usa i segnali per comunicare le modifiche alla GUI

                    //    System.Diagnostics.Debug.WriteLine($"output script error: {result}");

                       
                    //}
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error running Python script: " + ex.Message);

                return (configFound, ex.Message);
            }

            if (result.Length > 0)
            {
                //update FLag
                configFound = true;

                // Removes all non-printable characters except space, tab, newline, and carriage return.
                result =  Regex.Replace(result, "[^\\x20-\\x7E\\t\\n\\r]", string.Empty);

                return (configFound, result);

            }
            else
            {
                //default false, empty
                return (configFound, result);
            }

        }


        private string ExtractScriptName(string filePath)
        {
            //regex for finding __script_name__ 
            string pattern = @"^\s*__script_name__\s*=\s*""([^""]+)""";
            try
            {
                string fileContent = File.ReadAllText(filePath);
                //use regex to repeat search at every line
                Match match = Regex.Match(fileContent, pattern, RegexOptions.Multiline);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                else
                {
                    noOfScriptWithoutNameCounter++;
                    return ($"NoScriptName{noOfScriptWithoutNameCounter}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }

        }

    }
}




