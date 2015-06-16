using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ariete.Core
{
    class EmuleAdunanzaConfig 
    {
        public Path ConfigDir { get; set; }
        public Path Exe { get; private set; }
        public Path Config { get; private set; }
        public bool Exist { get; set; }
        private int myVar;
        public string ipExt { get; set; }
        public string ipInt { get; set; }
        public string ipFastweb { get; set; }


        public EmuleAdunanzaConfig()
        {

        }
        public EmuleAdunanzaConfig(Path configdir)
        {
            ConfigDir = configdir;
        }
    }
}
