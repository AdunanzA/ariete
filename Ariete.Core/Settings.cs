using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariete.Core
{
    public static class Settings
    {
        public const string BaseURI = "http://www.fastweb.it/myfastpage/goto/momi/?id=cfg-ngrg";
        public const string MyIpUrl = "http://evolution.adunanza.net/dhtchat/getmyip.php";
#if DebugNoFw
        public const string MyIpUrlFastweb = "http://25.10.194.40/emule_testport/getmyfastwebip.php"; // Per debug fuori fastweb
        public const string AdutestUrl = "http://25.10.194.40/emule_testport/adutest.php"; // Per debug fuori fastweb
#else
        public const string AdutestUrl = "http://adutest.adunanza.net/emule_testport/adutest.php";
        public const string MyIpUrlFastweb = "http://adutest.adunanza.net/emule_testport/getmyfastwebip.php";
#endif
    }
}
