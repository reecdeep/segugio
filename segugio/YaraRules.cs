
using libYaraWrapper.libyara;
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

    
    public class YaraRules
    {

        private CompiledRules rules;
        private Scanner scanner;
        private YaraContext ctx;

        Gateway gw;

        //
        HashSet<Yara> LoadedYaraList = new HashSet<Yara>();


        string yaraRulesDirectory;


        int numberOfLoadedYaraRules = 0;
        int noRuleNameCounter = 0;

        public YaraRules(Gateway gw)
        {
            this.gw = gw;
            ctx = new YaraContext();
            scanner = new Scanner();

           

            //define yara rule directory
            yaraRulesDirectory  = gw.settings.GetSetting("YaraRulesDirectory"); // Percorso della cartella con i file .yar

            gw.ut.CheckPaths("directory", yaraRulesDirectory);

            //load yaraRules
            //LoadYaraRules();
            rules = CompileYaraRules(yaraRulesDirectory);

        }








        public (bool, HashSet<Yara>) RunYaraScan(int processId)
        {
            HashSet<Yara> yaraResults = new HashSet<Yara>();
            Boolean isMalicious = false;

            if (rules != null)
            {
                try
                {
             
                    List<ScanResult> scanResults = scanner.ScanProcess(processId, rules);

                    if (scanResults.Count > 0)
                    {
                        isMalicious = true;
                    }

                    
                    foreach (var result in scanResults)
                    {
                        result.MatchingRule.Metas.TryGetValue("description", out object description);

                        Yara matchedyara = getYaraRule(result.MatchingRule.Identifier.ToString());

                        yaraResults.Add(matchedyara);
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("exxxception" + ex.Message);
                }

                

                
            }

            return (isMalicious, yaraResults);
        }

       
        private CompiledRules CompileYaraRules(string rulesPath)
        {
            var ruleFiles = Directory.GetFiles(rulesPath, "*.yar", SearchOption.AllDirectories).ToArray();

            using (var compiler = new Compiler())
            {
                foreach (var file in ruleFiles)
                {
                    String yaraRuleName = ExtractYaraRuleName(file);
                    String yaraRuleDescription = ExtractYaraRuleDescription(file);


                  
                    //add 
                    Yara newYara = new Yara(file, yaraRuleName, yaraRuleDescription);
                    LoadedYaraList.Add(newYara); 



                    //compile all rules
                    compiler.AddRuleFile(file);
                }

                numberOfLoadedYaraRules = LoadedYaraList.Count;
                string message = $"succesfully loaded {numberOfLoadedYaraRules} yara rules";
                gw.mainForm.progress.Report(new UpdateSignal
                {
                    Action = UpdateSignal.UpdateAction.NewLogMessage,
                    LogMessage = message
                });


                System.Diagnostics.Debug.WriteLine("Rules Compiled");
                return compiler.Compile();
            }
        }


       
        public Yara getYaraRule(string identifier)
        {
            Yara output = null;
            foreach(var rule in LoadedYaraList)
            {
                if(rule.Name.Equals(identifier))
                {
                    output = rule; 
                }
            }


            return output;
        }



        private string ExtractYaraRuleName(string filePath)
        {

            try
            {
                string fileContent = File.ReadAllText(filePath);
                // regex for finding "name" or "nome" in tag "meta"
                string pattern = @"meta:\s*[\r\n]+[\s\S]*?(name|nome)\s*=\s*""([^""]+)""";
                System.Text.RegularExpressions.Match match = Regex.Match(fileContent, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return match.Groups[2].Value;
                }
                else
                {
                    noRuleNameCounter++;
                    return ($"NoRuleName{noRuleNameCounter}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }


        }

        private string ExtractYaraRuleDescription(string filePath)
        {
            try
            {
                string fileContent = File.ReadAllText(filePath);
                // regex for finding "description" or "descrizione" in tag "meta"
                string pattern = @"meta:\s*[\r\n]+[\s\S]*?(description|descrizione)\s*=\s*""([^""]+)""";
                System.Text.RegularExpressions.Match match = Regex.Match(fileContent, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    // Restituisce il valore associato alla chiave trovata
                    return match.Groups[2].Value;
                }
                else
                {
                    return "Description not found.";
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
