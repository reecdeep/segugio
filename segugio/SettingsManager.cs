using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{
    public class SettingsManager
    {
        Gateway gw;
        private string filePath;
        private Dictionary<string, string> settings;
        public String settingFileName = "settings.ini";


        public SettingsManager(Gateway gw)
        {
            this.gw = gw;
            this.filePath = gw.directoryPath + "\\"+settingFileName;
            settings = new Dictionary<string, string>();
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (!File.Exists(filePath))
            {

                MessageBox.Show("File settings.ini not found. Make sure it is present within the same execution path as segugio.exe",
                    "Error while loading settings", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Environment.Exit(0);
            }

            foreach (var line in File.ReadAllLines(filePath))
            {
                // ignore empty strings or starting with #
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                {
                    continue;
                }

                var parts = line.Split(new char[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    settings.Add(parts[0].Trim(), parts[1].Trim());
                }
            }
        }

        public void SaveSettings()
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var setting in settings)
                {
                    writer.WriteLine($"{setting.Key}={setting.Value}");
                }
            }
        }

        public string GetSetting(string key)
        {
            string defaultValue = "";
            if (settings.ContainsKey(key))
            {
                return settings[key];
            }

            return defaultValue;
        }

        public bool GetBooleanSetting(string key)
        {
            if (settings.ContainsKey(key) && bool.TryParse(settings[key], out bool result))
            {
                return result;
            }

            return false; // Default value if not found or not parsable
        }

        

        public void SetSetting(string key, string value)
        {
            if (settings.ContainsKey(key))
            {
                settings[key] = value;
            }
            else
            {
                settings.Add(key, value);
            }
        }
    }
}
