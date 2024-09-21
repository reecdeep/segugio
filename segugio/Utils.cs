using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace segugio
{
    //file di utility multi-classe
    public class Utils
    {
        Gateway gw;
        public Utils(Gateway gw)
        {
            this.gw = gw;
        }


        public bool IsStringNumeric(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }


        public void CheckPaths(String type, String path)
        {
         
            if(type.Equals("file"))
            {
                if (!File.Exists(path))
                {
                    MessageBox.Show($"The specified file {path} doesn't exists.",
                       "Error while loading file", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Environment.Exit(0);
                }
            }

            if(type.Equals("directory"))
            {
                if (!Directory.Exists(path))
                {
                    MessageBox.Show($"The specified directory {path} doesn't exists.",
                       "Error while loading directory", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Environment.Exit(0);
                }
            }

        }


        //method for finding the index of the preferred program for being the parent pid of new created process
        public int FindIndexWithText(ComboBox comboBox, string preferredValue)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                var kvp = (KeyValuePair<uint, string>)comboBox.Items[i];
                if (kvp.Value == preferredValue)
                {
                    return i;
                }
            }
            return -1; 
        }





    }


    
}
