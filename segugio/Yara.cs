using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace segugio
{
    public class Yara
    {
        public string FilePath { get;  set; }
        public string Name { get;  set; }
        public string Description { get;  set; }

        public Yara(string yaraPath, string yaraName, string yaraDescription)
        {
            this.FilePath = yaraPath;
            this.Name = yaraName;
            this.Description = yaraDescription;
        }


        public override bool Equals(object obj) 
        {
            
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (Yara)obj;

            
            return Name == other.Name && FilePath == other.FilePath && Description == other.Description;
        }

        public override int GetHashCode() 
        {
            unchecked 
            {
                int hash = 17;
                //23 as prime number
                hash = hash * 23 + (Name != null ? Name.GetHashCode() : 0);
                hash = hash * 23 + (FilePath != null ? FilePath.GetHashCode() : 0);
                hash = hash * 23 + (Description != null ? Description.GetHashCode() : 0);
                return hash;
            }
        }



       


    }
}
