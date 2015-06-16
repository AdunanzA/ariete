using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ariete.Core
{
    abstract class SoftwareConfig
    {
        public abstract Path ConfigDir { get; set; }
        public abstract Path Exe { get; private set; }
        public abstract Path Config { get; private set; }
        public abstract bool Exist { get; private set; }
        public virtual string ipExt { get; set; }
        public virtual string ipInt { get; set; }
        public virtual string ipMan { get; set; }

        public SoftwareConfig()
        {

        }
        public SoftwareConfig(Path configdir)
        {
            ConfigDir = configdir;
        }
    }
}
