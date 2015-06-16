using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Net.Sockets;
using System.IO;
using IniParser;
using IniParser.Model;
using NLog;
using ScrapySharp.Core;
using ScrapySharp.Html.Parsing;
using ScrapySharp.Extensions;
using HtmlAgilityPack;
using System.Diagnostics;
using Mono.Nat;
using System.Threading;
using System.Reflection;
using System.Net.NetworkInformation;


namespace Ariete.Core
{
    public static class Utils
    {
        public static volatile bool StopTcpThread;
        public static volatile bool StopUdpThread;

        private static Logger logger = LogManager.GetLogger("ArieteCore");

        public static string GetOperatingSystem()
        {
            return Environment.OSVersion.ToString();
        }

        public static int GetUdpPortToUseMFP(string ipv4)
        {
            return (GetTcpPortToUseMFP(ipv4) + 10);
        }

        public static int GetTcpPortToUseMFP(string ipv4)
        {
            ipv4 = GetIpAddress();
            string[] ottenti = ipv4.Split('.');
            switch (ottenti.Last())
            {
                case "128":
                    return 4662;
                case "129":
                    return 4663;
                case "130":
                    return 4664;
                case "131":
                    return 4665;
                case "132":
                    return 4666;
                case "133":
                    return 4667;
                case "134":
                    return 4668;
                default:
                    throw new Exception("Indirizzo IP non valido per l'uso della procedura automatica sulla myfastpage");
            }
        }

        public static MyWebClient GetHttp(string url, MyWebClient wc)
        {
            var response = wc.DownloadString(new Uri(url));
            logger.Info(Environment.NewLine + "Scaricata prima pagina: {0}", Settings.BaseURI);

            logger.Info("ResponseHeaders:" + Environment.NewLine + "{0}", wc.ResponseHeaders);
            var cc = wc._mContainer.GetCookies(new Uri(url));
            foreach (var cookie in cc)
            {
                logger.Info("Cookie: {0}", cookie);
            }
            logger.Info("Redirect Location: {0}", wc._responseUri);

            foreach (var param in wc.responseParams)
            {
                logger.Info("Fragment: {0} = {1}", param.Key, param.Value);
            }
            return wc;
        }

        public static void PostHttp(MyWebClient wc, string baseUrl, string paramConcat)
        {
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var response = wc.UploadString(baseUrl, paramConcat);
            }
            catch (WebException ex)
            {
                logger.Error("Impossibile eseguire la post: " + ex.Message);
                throw new Exception("Impossibile eseguire la post: " + ex.Message);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            logger.Info("ResponseHeaders:" + Environment.NewLine + "{0}", wc.ResponseHeaders);
            var cc = wc._mContainer.GetCookies(new Uri("http://www.fastweb.it"));
            foreach (var cookie in cc)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                logger.Info("Cookie: {0}", cookie);
            }
            Console.ForegroundColor = ConsoleColor.White;
            logger.Info("Redirect Location: {0}", wc._responseUri);

            foreach (var param in wc.responseParams)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                logger.Info("Fragment: {0} = {1}", param.Key, param.Value);
            }

        }

        public static Dictionary<string, string> GetComputersFromHtml(MyWebClient wc)
        {
#if !Release
            //Read from file for now
            StreamReader sr = new StreamReader("response.html");
            var response = File.ReadAllText("response.html");
            // End read from file
#endif
            HtmlNode html;
            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            html = doc.DocumentNode;

            var inputs = html.CssSelect("input[type=hidden]#ipCpe");
            wc.responseParams["ipCpe"] = inputs.First().Attributes["value"].Value;

            var optiontags = html.CssSelect("option");
            var query = from x in optiontags
                        where x.Attributes["value"].Value.StartsWith("192.168") && x.Attributes["value"].Value != x.Attributes["label"].Value
                        select new { ip = x.Attributes["value"].Value.ToString(), macAddress = x.Attributes["label"].Value };
            Dictionary<string, string> computers = new Dictionary<string, string>();
            foreach (var pc in query)
            {
                computers.Add(pc.ip, pc.macAddress);
            }
            return computers;

        }

        public static bool IsFastweb()
        {
            // Controllo se l'ip esterno e' un Fastweb pubblico di nuova generazione
            string ipext = GetIpAddressExt();
            if (ipext.StartsWith("93.3") ||
                ipext.StartsWith("93.4") ||
#if DebugNoFw
 ipext.StartsWith("93") || //aggiunto per debug dalla mia linea da rimuovere
#endif
 ipext.StartsWith("93.5") ||
                ipext.StartsWith("93.6") ||
                ipext.StartsWith("2.22") ||
                ipext.StartsWith("2.23") ||
                ipext.StartsWith("2.24") ||
                ipext.StartsWith("2.25")
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsFastwebPublicIP()
        {
            // Controllo se l'ip esterno e' un Fastweb pubblico di nuova generazione
            string ipext = GetIpAddressExt();
            string ipFw = GetIpAddressFastweb();
            if (IsFastweb() && (ipext == ipFw))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetIpAddressExt()
        {
            try
            {
                WebClient wc2 = new WebClient();
                return wc2.DownloadString(Settings.MyIpUrl);
            }
            catch (WebException ex)
            {
                logger.Error("Rilevati problemi durante il rilevamento dell'indirizzo IP Esterno " + ex.Message);
                throw new Exception("Rilevati problemi durante il rilevamento dell'indirizzo IP Esterno " + ex.Message);
            }

        }

        public static string GetIpAddressFastweb()
        {
            if (!IsFastweb())
                return "Linea Fastweb non rilevata";
            try
            {
                WebClient wc2 = new WebClient();
                return wc2.DownloadString(Settings.MyIpUrlFastweb);
            }
            catch (WebException ex)
            {
                logger.Error("Rilevati problemi durante il rilevamento dell'indirizzo IP Fastweb " + ex.Message);
                throw new Exception("Rilevati problemi durante il rilevamento dell'indirizzo IP Fastweb " + ex.Message);
            }
        }

        public static string GetIpAddress()
        {
            // Codice quasi equivalente usando le lambda ma non restituisce 1 singolo valore
            //var iface = interfaces.Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
            //            ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
            //            ni.OperationalStatus == OperationalStatus.Up &&
            //            !ni.Name.Contains("VMware") && !ni.Name.Contains("Hamachi") && !ni.Name.Contains("Virtual") && !ni.Name.Contains("Team") && !ni.Name.Contains("Blue") &&
            //            ni.GetIPProperties().UnicastAddresses.SingleOrDefault().Address.AddressFamily == AddressFamily.InterNetwork &&
            //            ni.GetIPProperties().UnicastAddresses.SingleOrDefault().Address.ToString().StartsWith("192.168") |
            //            ni.GetIPProperties().UnicastAddresses.SingleOrDefault().Address.ToString().StartsWith("10")
            //            );
            //var ipv4 = (iface.FirstOrDefault().GetIPProperties().UnicastAddresses.FirstOrDefault().Address.ToString());

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                        ni.OperationalStatus == OperationalStatus.Up &&
                        (!ni.Name.Contains("VMware") | !ni.Name.Contains("Hamachi") | !ni.Name.Contains("VirtualBox") | !ni.Name.Contains("TeamViewer") | !ni.Name.Contains("Bluetooth"))
                    )
                {   
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork &
                            (ip.Address.ToString().StartsWith("192.168") |
                            ip.Address.ToString().StartsWith("10"))
                            )
                        {
                            // Ritorno il primo ip trovato pregando sia quello giusto grazie a tutti i filtri precedenti
                            return ip.Address.ToString();
                        }

                    }
                }
            }
            return "IPv4 utile non trovato";
        }

        public static bool CanUseMyFastPage()
        {
            int lastOct = int.Parse(GetIpAddress().Split('.').Last());
            if (IsFastwebPublicIP() && (lastOct >= 128 && lastOct <= 254))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        #region aMule AdunanzA

        public static string AmuleAdunanzaAduboxGetUdpPort()
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();
            IniData data = iniparser.ReadFile(AmuleAdunanzaAduboxConfigFileGetPath());
            return data["eMule"]["UDPPort"];
        }

        public static string AmuleAdunanzaAduBoxGetTcpPort()
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();
            IniData data = iniparser.ReadFile(AmuleAdunanzaAduboxConfigFileGetPath());
            return data["eMule"]["Port"];
        }

        public static string AmuleAdunanzaAduBoxConfigDirPath()
        {
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".aMuleAdunanzA", "adunanza.conf")) &&
                File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".aMuleAdunanzA", "amule.conf")))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".aMuleAdunanzA");
            }
            else
            {
                logger.Error("aMuleAdunanzA Configuration on AduBox not found!");
                return null;
            }
        }

        public static void AmuleAdunanzaSetTcpPort(int port)
        {
            if ((port > 65554) && (port == 0))
            {
                throw new Exception("la Porta TCP da configurare non e' valida");
            }
            var conf = AmuleAdunanzaAduboxConfigFileGetPath();
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();
            IniData data = iniparser.ReadFile(conf);
            data["eMule"]["Port"] = port.ToString();

            iniparser.SaveFile(conf, data);
        }

        public static void AmuleAdunanzaSetUdpPort(int port)
        {
            if ((port > 65554) && (port == 0))
            {
                throw new Exception("la Porta UDP da configurare non e' valida");
            }
            var conf = AmuleAdunanzaAduboxConfigFileGetPath();
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();
            IniData data = iniparser.ReadFile(conf);
            data["eMule"]["UDPPort"] = port.ToString();

            iniparser.SaveFile(conf, data);
        }

        public static bool AmuleAdunanzaAduboxExist()
        {
            if (File.Exists(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".aMuleAdunanzA", "adunanza.conf")) &&
                File.Exists(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".aMuleAdunanzA", "amule.conf")) &&
                File.Exists(Path.Combine("/usr", "bin", "amule")))
            {
                return true;
            }
            else return false;
        }

        public static string AmuleAdunanzaAduboxConfigFileGetPath()
        {
            return Path.Combine(AmuleAdunanzaAduBoxConfigDirPath(), "amule.conf");
        }

        public static string AmuleAdunanzABinGetPath()
        {
            if (File.Exists(Path.Combine("/usr", "bin", "amule")))
            {
                return Path.Combine("/usr", "bin", "amule");
            }
            else
            {
                logger.Fatal("aMuleAdunanzA non trovato!");
                return null;
            }
        }

        public static string AmuleAdunanzaAduboxConfigGetKey(string key)
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.ReadFile(AmuleAdunanzaAduboxConfigFileGetPath());

            return data["eMule"][key];
        }

        public static void AmuleAdunanzaStart(string binPath = null)
        {
            if (binPath == null)
            {
                binPath = AmuleAdunanzABinGetPath();
            }
            Process aMuleAdu = new Process();
            var procs = Process.GetProcessesByName("amuled");

            if (procs.Count() < 1)
            {
                aMuleAdu.StartInfo.FileName = binPath;
                aMuleAdu.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                if (!aMuleAdu.Start())
                {
                    logger.Error("Impossibile eseguire aMuleAdunanzA");
                }
                else
                {
                    logger.Info("aMule AdunanzA avviato con successo.");
                }
            }
            else
            {
                logger.Info("aMuleAdunanzA e' gia' in esecuzione.");
            }
        }

        public static void AmuleAdunanzaAduboxKill()
        {
            if (AmuleAdunanzaAduboxExist())
            {
                Process p = new Process();
                try
                {
                    var procs = Process.GetProcessesByName("amuled");
                    p = procs[0];
                    p.Kill();
                }
                catch (Exception)
                {
                    logger.Error("Impossibile uccidere aMule AdunanzA, sembra gia' morto.");
                }
            }
        }

        public static void AmuleAdunanzaChangePortsMFP(int tcpport = 0, int udpport = 0)
        {
            if (!AmuleAdunanzaAduboxExist()) return;
            logger.Info("Modifica delle porte di eMule AdunanzA in accordo con la MyFastPage");
            var ip = GetIpAddress();

            if (tcpport == 0 || udpport == 0)
            {
                tcpport = GetTcpPortToUseMFP(ip);
                udpport = GetUdpPortToUseMFP(ip);
            }
            try
            {
                AmuleAdunanzaSetTcpPort(tcpport);
                AmuleAdunanzaSetUdpPort(udpport);
            }
            catch (Exception)
            {
                logger.Error("Impossibile cambiare le porte di aMule AdunanzA. Qualcosa e' andato storto.");
                throw new Exception("Impossibile cambiare le porte di aMule AdunanzA. Qualcosa e' andato storto.");
            }

            logger.Info("Nuova porta TCP: " + tcpport);
            logger.Info("Nuova porta UDP: " + udpport);
        }

        public static string AduTestDoAdubox()
        {
            if (!IsFastweb())
                throw new Exception("Ariete non può fare l'AduTest su una linea non Fastweb");

            WebClient wc2 = new WebClient();
            var res = wc2.DownloadString(Settings.AdutestUrl + "?tcp=" + AmuleAdunanzaAduBoxGetTcpPort() + "&udp=" + AmuleAdunanzaAduboxGetUdpPort());
            logger.Debug(res);
            return res;
        }

        public static bool AduTestTcpAduboxGetResult(string res)
        {
            if (!IsFastweb())
                return false;

            logger.Debug(res);
            if (res.Contains("TCP: <strong>FALLITO"))
            {
                logger.Info("AduTest sulla porta TCP " + AmuleAdunanzaAduBoxGetTcpPort() + " Fallito.");
                return false;
            }
            else
            {
                logger.Info("AduTest sulla porta TCP " + AmuleAdunanzaAduboxGetUdpPort() + " Passato con successo.");
                return true;
            }
        }

        public static bool AduTestUdpAduboxGetResult(string res)
        {
            if (!IsFastweb())
                return false;

            if (res.Contains("UDP: <strong>FALLITO"))
            {
                logger.Info("AduTest sulla porta UDP " + AmuleAdunanzaAduBoxGetTcpPort() + " Fallito.");
                return false;
            }
            else
            {
                logger.Info("AduTest sulla porta UDP " + AmuleAdunanzaAduboxGetUdpPort() + " Passato con successo.");
                return true;
            }
        }
        #endregion

        #region eMule AdunanzA

        //OpenFileDialog openFileDialog1 = new OpenFileDialog();

        //            openFileDialog1.InitialDirectory = "c:\\";
        //            openFileDialog1.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
        //            openFileDialog1.FilterIndex = 2;
        //            openFileDialog1.RestoreDirectory = true;

        //            if (openFileDialog1.ShowDialog() == DialogResult.OK)
        //            {
        //                try
        //                {
        //                    if (openFileDialog1.CheckFileExists)
        //                    {
        //                        Path.GetDirectoryName(openFileDialog1.FileName);
        //                    }
        //                    else {
        //                        logger.Fatal("eMule AdunanzA non sembra essere installato su questo computer.");
        //                        logger.Fatal("Ariete non puo' quindi fare nulla per te e si ritira.");
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
        //                }
        //            }
        //            return;
        public static bool EmuleAdunanzAExist()
        {
            try
            {
                if (EmuleAdunanzAConfigExist() & EmuleAdunanzAExeExist())
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }


        }

        private static bool EmuleAdunanzAExeExist()
        {
            if (EmuleAdunanzAExeGetPath() == null)
            {
                return false;
            }
            return true;
        }

        public static bool EmuleAdunanzAConfigExist()
        {
            if (EmuleAdunanzAConfigFileGetPath() == null)
            {
                return false;
            }
            return true;
        }

        public static string EmuleAdunanzaConfigGetKey(string key)
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.ReadFile(EmuleAdunanzAConfigFileGetPath());

            return data["eMule"][key];
        }

        public static void EmuleAdunanzaConfigSetKey(string key, string value)
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.ReadFile(EmuleAdunanzAConfigFileGetPath());
            data["eMule"][key] = value;

            iniparser.SaveFile(EmuleAdunanzAConfigFileGetPath(), data);
        }

        public static string EmuleAdunanzaGetTcpPort()
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.ReadFile(EmuleAdunanzAConfigFileGetPath());

            return data["eMule"]["Port"];
        }

        public static void EmuleAdunanzaSetTcpPort(int port)
        {
            if ((port > 65554) && (port == 0))
            {
                throw new Exception("la Porta TCP da configurare non e' valida");
            }

            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.ReadFile(EmuleAdunanzAConfigFileGetPath());
            data["eMule"]["Port"] = port.ToString();

            iniparser.SaveFile(EmuleAdunanzAConfigFileGetPath(), data);
        }

        public static string EmuleAdunanzaGetUdpPort()
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.ReadFile(EmuleAdunanzAConfigFileGetPath());

            return data["eMule"]["UDPPort"];
        }

        public static void EmuleAdunanzASetUdpPort(int port)
        {
            if ((port > 65554) && (port == 0))
            {
                throw new Exception("la Porta UDP da configurare non e' valida");
            }

            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.ReadFile(EmuleAdunanzAConfigFileGetPath());
            data["eMule"]["UDPPort"] = port.ToString();

            iniparser.SaveFile(EmuleAdunanzAConfigFileGetPath(), data);
        }

        public static string EmuleAdunanzAConfigFileGetPath()
        {
            return Path.Combine(EmuleAdunanzAConfigDirGetPath(), "preferences.ini");
        }

        public static string EmuleAdunanzAExeGetPath()
        {
            // OS e' XP
            if (Environment.OSVersion.Version.Major < 6)
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    if (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILESX86"), "eMule AdunanzA", "eMule_AdnzA.exe")))
                    {
                        return (Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILESX86"), "eMule AdunanzA", "eMule_AdnzA.exe"));
                    }
                    else
                    {
                        logger.Error("eMuleAdunanzA non trovato nelle cartelle standard!");
                        return null;
                    }
                }
                // Sono a 32bit
                else
                {
                    return (Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES"), "eMule AdunanzA", "eMule_AdnzA.exe"));
                }
            }
            // potrebbe essere vuoto sui 32bit
            else if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "eMule AdunanzA", "eMule_AdnzA.exe")))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "eMule AdunanzA", "eMule_AdnzA.exe");
            }
            else
            {
                logger.Error("eMuleAdunanzA exe non trovato!");
                return null;
            }
        }

        // Questa funzione si interrompe a causa dei permessi (entrando nel catch) e quindi non rileva le directory dopo
        // TODO va cambiata per gestire il problema
        public static string EmuleAdunanzAConfigDirSearch(string file)
        {
            var files = new List<string>();
            var fiList = new List<FileInfo>();

            foreach (DriveInfo d in DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed))
            {

                try
                {
                    files.AddRange(Directory.EnumerateFiles(d.RootDirectory.FullName, file, SearchOption.AllDirectories));
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.Warn(ex.Message);
                }
            }
            foreach (var f in files)
            {
                var fi = new FileInfo(f);
                fiList.Add(fi);
            }


            return fiList.OrderByDescending(t => t.LastWriteTime).FirstOrDefault().FullName;
        }


        public static string EmuleAdunanzAConfigDirGetPath()
        {
            // OS e' XP
            if (Environment.OSVersion.Version.Major < 6)
            {
                // 64bit OS
                if (Environment.Is64BitOperatingSystem)
                {
                    if (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILESX86"), "eMule AdunanzA", "Config",
                        "preferences.ini")))
                    {
                        return (Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILESX86"), "eMule AdunanzA", "Config"));
                    }
                    else if (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "eMule AdunanzA", "Config",
                        "preferences.ini")))
                    {
                        return (Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "eMule AdunanzA", "Config"));
                    }
                    else
                    {
                        logger.Error("eMuleAdunanzA configdir xp non trovato!");
                        return (null);
                    }
                }
                // XP 32bit
                else
                {
                    if (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES"), "eMule AdunanzA", "Config",
                        "preferences.ini")))
                    {
                        return (Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES"), "eMule AdunanzA", "Config"));
                    }
                    else if (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "eMule AdunanzA", "Config",
                        "preferences.ini")))
                    {
                        return (Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "eMule AdunanzA", "Config"));
                    }
                    else
                    {
                        logger.Error("eMuleAdunanzA configdir non trovato!");
                        return null;
                    }
                }
            }

            // OS da Vista in su
            else if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "eMule AdunanzA", "Config", "preferences.ini")))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "eMule AdunanzA", "Config");
            }
            else if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "eMule AdunanzA", "Config", "preferences.ini")))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "eMule AdunanzA", "Config");
            }
            else
            {
                logger.Error("eMuleAdunanzA configdir non trovato!");
                return null;
            }
        }
        #endregion

        #region AduTest
        public static string AduTestDo(int tcp = 0, int udp = 0)
        {
            if (!IsFastweb())
                throw new Exception("Ariete non può fare l'AduTest su una linea non Fastweb");

            if (tcp == 0 && udp == 0)
            {
                tcp = int.Parse(EmuleAdunanzaGetTcpPort());
                udp = int.Parse(EmuleAdunanzaGetUdpPort());
            }

            try
            {
                Thread th2 = new Thread(() => Utils.StartListenerTcp(tcp));
                th2.Name = "ListenerTcpThread";
                th2.Start();
                Thread th = new Thread(() => Utils.StartListenerUdp(udp));
                th.Name = "ListenrUdpThread";
                th.Start();

                WebClient wc2 = new WebClient();
                var res = wc2.DownloadString(Settings.AdutestUrl + "?tcp=" + tcp + "&udp=" + udp);

                Utils.StopTcpThread = true;
                Utils.StopUdpThread = true;
                //th.Interrupt();
                //th2.Interrupt();
                return res;
            }
            catch (WebException ex)
            {
                logger.Error("Rilevati problemi durante la connessione al server dell'AduTest " + ex.Message);
                throw new Exception("Rilevati problemi durante la connessione al server dell'AduTest: " + ex.Message);
            }
        }
        public static string ExtractBetween(this string str, string startTag, string endTag, bool inclusive)
        {
            string rtn = null;

            int s = str.IndexOf(startTag);
            if (s >= 0)
            {
                if (!inclusive)
                    s += startTag.Length;

                int e = str.IndexOf(endTag, s);
                if (e > s)
                {
                    if (inclusive)
                        e += startTag.Length;

                    rtn = str.Substring(s, e - s);
                }
            }

            return rtn;
        }
        public static bool AduTestTcpGetResult(string res)
        {
            if (!IsFastweb())
                return false;

            string tcpport = ExtractBetween(res, "della porta TCP", "...", false);

            logger.Debug(res);
            if (res.Contains("TCP: <strong>FALLITO"))
            {
                logger.Error("AduTest sulla porta TCP " + tcpport + " Fallito.");
                return false;
            }
            else
            {
                logger.Info("AduTest sulla porta TCP " + tcpport + " Passato con successo.");
                return true;
            }
        }

        public static bool AduTestUdpGetResult(string res)
        {
            if (!IsFastweb())
                return false;
            string udpport = ExtractBetween(res, "della porta UDP", "...", false);

            if (res.Contains("UDP: <strong>FALLITO"))
            {
                logger.Error("AduTest sulla porta UDP " + udpport + " Fallito.");
                return false;
            }
            else
            {
                logger.Info("AduTest sulla porta UDP " + udpport + " Passato con successo.");
                return true;
            }

        }

        public static void StartListenerTcp(int port = 9000)
        {
            try
            {
                StopTcpThread = false;
                int recv;
                byte[] data = new byte[1024];
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, (int)port);

                Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                newsock.Bind(ipep);
                newsock.Listen(10);

                logger.Info("Waiting for message on TCP Port " + port.ToString() + " ...");
                Socket client = newsock.Accept();
                IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;
                string hello = "Ciao AduTest!";
                byte[] pkthello = new byte[200];
                pkthello = Encoding.ASCII.GetBytes(hello);

                while (!StopTcpThread)
                {
                    data = new byte[1024];
                    recv = client.Receive(data);
                    if (recv == 0)
                        break;

                    logger.Info("Connessione TCP con {0} ", clientep.Address);
                    logger.Info("Data: " + Encoding.ASCII.GetString(data, 0, recv));
                    client.Send(pkthello, pkthello.Length, SocketFlags.None);
                }
                client.Close();
                logger.Info("Disconnessione TCP con {0} ", clientep.Address);
                newsock.Close();
            }
            catch (Exception ex)
            {
                logger.Error("Eccezione non gestita: " + ex.Message);
            }
        }


        public static void StartListenerUdp(int port = 9001)
        {
            try
            {
                StopUdpThread = false;
                int recv;
                byte[] data = new byte[1024];
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);

                Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                newsock.Bind(ipep);
                logger.Info("Waiting for Message on UDP Port " + port.ToString() + " ...");

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);

                // Pacchetto di risposta che si aspetta l'Adutest
                // PONG chr(0xa4) && $tmp[1]==chr(0x58)) 
                byte[] pong = new byte[2] { 0xa4, 0x58 };
                byte[] ping = new byte[4] { 0xa4, 0x50, 0xb6, 0x87 };

                while (!StopUdpThread)
                {
                    data = new byte[1024];
                    recv = newsock.ReceiveFrom(data, ref Remote);
                    if (recv == 0)
                        break;
                    logger.Info("Message UDP received from {0}:", Remote.ToString());
                    logger.Info("Data: " + Encoding.Unicode.GetString(data, 0, recv));

                    if (data.Take(4).SequenceEqual(ping))
                    {
                        newsock.SendTo(pong, pong.Length, SocketFlags.None, Remote);
                        Thread.Sleep(100);
                        newsock.SendTo(pong, pong.Length, SocketFlags.None, Remote);
                        logger.Info("Sent 3 UDP Pong Message to {0}:", Remote.ToString());
                        StopUdpThread = true;
                    }
                }
                newsock.Close();
            }
            catch (Exception ex)
            {
                logger.Error("Eccezione non gestita: " + ex.Message);
            }

        }

        #endregion

        #region MyFastwebPAge

        public static bool MFPManual(MyWebClient wc)
        {
            // Se siamo nella pagina giusta procedo alla costruzione della POST finale
            if (wc.responseParams.ContainsKey("identifycode"))
            {
                StringBuilder sb = new StringBuilder(
                "current=2" +
                "&step=3" +
                "&ctrl=" +
                "&portmap_action=manual_setup" +
                "&channel=" + wc.responseParams["channel"] +
                "&account=" + wc.responseParams["account"] +
                "&username=" + wc.responseParams["username"] +
                "&service=" + wc.responseParams["service"] +
                "&actionid=" +
                "&status=" + wc.responseParams["status"] +
                "&segmento=" + wc.responseParams["segmento"] +
                "&selcode=" + wc.responseParams["selcode"] +
                "&origin=" + wc.responseParams["origin"] +
                "&checksum=" + wc.responseParams["checksum"] +
                "&identifycode=" + wc.responseParams["identifycode"] +
                "&previous=1" + "&aVarsParam=HTTP/1.1");
                logger.Info("http://fastmomi.fastweb.it/app/services/cfg-ngrg/RES-Bought.php?" + sb.ToString());
                PostHttp(wc, "http://fastmomi.fastweb.it/app/services/cfg-ngrg/RES-Bought.php?", sb.ToString());

                // per ora non scriviamo la risposta
                //logger.Info(response);
                //File.WriteAllText("response.html", response);

                Dictionary<string, string> dic = GetComputersFromHtml(wc);
                string mymac = dic[GetIpAddress()];

                wc.responseParams["pmaction"] = "UPDATE";
                //wc.responseParams["ipCpe"]; //gia assegnato
                wc.responseParams["ID"] = ""; //chissa' che cosa e'
                wc.responseParams["ExternalPort"] = EmuleAdunanzaGetTcpPort();
                wc.responseParams["InternalPort"] = EmuleAdunanzaGetTcpPort();
                wc.responseParams["Protocol"] = "TCP";
                wc.responseParams["InternalClient"] = GetIpAddress();
                wc.responseParams["Description"] = "AdunanzA TCP";
                wc.responseParams["Chaddr"] = mymac;

                StringBuilder sb2 = new StringBuilder(
                "pmaction=" + wc.responseParams["pmaction"] +
                "&ipCpe=" + wc.responseParams["ipCpe"] +
                "&ID=" +
                "&ExternalPort=" + wc.responseParams["ExternalPort"] +
                "&InternalPort=" + wc.responseParams["InternalPort"] +
                "&Protocol=" + wc.responseParams["Protocol"] +
                "&InternalClient=" + wc.responseParams["InternalClient"] +
                "&Description=" + wc.responseParams["Description"] +
                "&Chaddr=" + wc.responseParams["Chaddr"]
                );
                //"pmaction=UPDATE&ipCpe=10.45.43.222&ID=&ExternalPort=7002&InternalPort=7002&Protocol=TCP&InternalClient=192.168.1.129&Description=&Chaddr=4c:ed:de:e8:7e:73";
                var response = wc.UploadString("http://fastmomi.fastweb.it/app/services/cfg-ngrg/Ajax_writePortMapping.php", "POST", sb2.ToString());

                //logger.Info(response);

                if (wc._responseUri.ToString().Contains("esito") && wc._responseUri.ToString().Contains("OK"))
                {
                    logger.Info("Procedura apertura porte Manuale avvenuta con successo!");
                    return true;
                }
                else
                {
                    logger.Error("Procedura apertura porte Manuale Fallita.");
                    return false;
                }
            }
            else
            {
                logger.Info("Procedura apertura porte automatica Fallita.");
                return false;
            }
        }

        public static bool MFPConfigurazioneRapida(MyWebClient wc)
        {
            // Se siamo nella pagina giusta procedo alla costruzione della POST finale
            if (wc.responseParams.ContainsKey("identifycode"))
            {
                StringBuilder sb = new StringBuilder(
                "current=2" +
                "&step=E" +
                "&ctrl=" +
                "&portmap_action=automatic_setup" +
                "&channel=" + wc.responseParams["channel"] +
                "&account=" + wc.responseParams["account"] +
                "&username=" + wc.responseParams["username"] +
                "&service=" + wc.responseParams["service"] +
                "&actionid=" +
                "&status=" + wc.responseParams["status"] +
                "&segmento=" + wc.responseParams["segmento"] +
                "&selcode=" + wc.responseParams["selcode"] +
                "&origin=" + wc.responseParams["origin"] +
                "&checksum=" + wc.responseParams["checksum"] +
                "&identifycode=" + wc.responseParams["identifycode"] +
                "&previous=1" + "&aVarsParam=HTTP/1.1");
                logger.Info("http://fastmomi.fastweb.it/app/services/cfg-ngrg/RES-Bought.php?" + sb.ToString());

                Utils.PostHttp(wc, "http://fastmomi.fastweb.it/app/services/cfg-ngrg/RES-Bought.php?", sb.ToString());
                if (wc._responseUri.ToString().Contains("esito") && wc._responseUri.ToString().Contains("OK"))
                {
                    logger.Info("Procedura apertura porte automatica avvenuta con successo!");
                    return true;
                }
                else
                {
                    logger.Info("Procedura apertura porte automatica Fallita.");
                    return false;
                }
            }
            else
            {
                logger.Info("Procedura apertura porte automatica Fallita.");
                return false;
            }
        }

        //TODO se si usa upnp non si puo' usare le porte di default per la procedura rapida
        public static void EmuleAdunanzAChangePortsMFP(int tcpport = 0, int udpport = 0)
        {
            if (!EmuleAdunanzAExist()) return;
            var ip = GetIpAddress();

            // Uso le porte di default secondo il portmapping Fastweb se non passo i parametri alla funzione
            if (tcpport == 0 || udpport == 0)
            {
                logger.Info("Modifica delle porte di eMule AdunanzA in accordo con la MyFastPage in corso...");
                tcpport = GetTcpPortToUseMFP(ip);
                udpport = GetUdpPortToUseMFP(ip);
            }

            try
            {
                logger.Info("Modifica delle porte di eMule AdunanzA in corso...");
                EmuleAdunanzaSetTcpPort(tcpport);
                EmuleAdunanzASetUdpPort(udpport);
            }
            catch (Exception)
            {
                logger.Error("Impossibile cambiare le porte di eMule AdunanzA. Qualcosa e' andato storto.");
                throw new Exception("Impossibile cambiare le porte di eMule AdunanzA. Qualcosa e' andato storto.");
            }

            logger.Info("Nuova porta TCP: " + tcpport);
            logger.Info("Nuova porta UDP: " + udpport);
        }
        #endregion

        public static void EmuleAdunanzaKill()
        {
            if (EmuleAdunanzAExist())
            {
                Process emuleAdu = new Process();
                try
                {
                    var procs = Process.GetProcessesByName("eMule_AdnzA");
                    emuleAdu = procs[0];
                    logger.Info("Seek & Destroy eMule AdunanzA!");
                    emuleAdu.Kill();
                }
                catch (Exception)
                {
                    logger.Error("Impossibile uccidere eMule AdunanzA, sembra gia' morto.");
                }
            }
        }

        public static void EmuleAdunanzAStart(string exePath = null)
        {
            if (exePath == null)
            {
                exePath = EmuleAdunanzAExeGetPath();
            }
            Process eMuleAdu = new Process();
            var procs = Process.GetProcessesByName("eMule_AdnzA");

            if (procs.Count() < 1)
            {
                eMuleAdu.StartInfo.FileName = exePath;
                eMuleAdu.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                if (!eMuleAdu.Start())
                {
                    logger.Error("Impossibile eseguire eMuleAdunanzA");
                }
                else
                {
                    logger.Info("eMule AdunanzA avviato con successo.");
                }
            }
            else
            {
                logger.Info("eMuleAdunanzA e' gia' in esecuzione.");
            }
        }

        # region WinFirewall
        public static bool FirewallNetshIsRule(string appName = null)
        {
            if (appName == null)
            {
                appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            }

            Process p = new Process();
            try
            {
                // Siamo su XP
                if (Environment.OSVersion.Version.Major < 6)
                {
                    p.StartInfo.Arguments = "firewall show allowedprogram name=\"" + appName + "\"";
                }
                else p.StartInfo.Arguments = "advfirewall firewall show rule name=\"" + appName + "\"";

                p.StartInfo.FileName = "netsh";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string s = p.StandardOutput.ReadToEnd();
                logger.Info("Controllo esistenza regola Firewall per " + appName);
                logger.Info("Parametri: " + p.StartInfo.Arguments);
                logger.Debug("Risultato: " + s);
                if (!s.Contains("Ok.") || !s.Contains(appName))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Durante l'enumerazione regole firewall: " + ex);
                return false;
            }
        }
        /// <summary>
        /// Cancella tutte le regole sul firewall per uno specifico eseguibile
        /// </summary>
        /// <param name="appName" required=true>Richiesto su vista ed oltre</param>
        /// <param name="exePath">Richiesto su XP</param>
        public static void FirewallNetshDeleteRule(string appName = null, string exePath = null)
        {
            if (appName == null)
            {
                appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            }

            if (exePath == null)
            {
                exePath = Environment.GetCommandLineArgs()[0];
            }
            Process p = new Process();

            try
            {
                p.StartInfo.FileName = "netsh";
                // Siamo su XP
                if (Environment.OSVersion.Version.Major < 6)
                {
                    p.StartInfo.Arguments = "firewall delete allowedprogram program=\"" + exePath + " \" ";
                }
                else p.StartInfo.Arguments = "advfirewall firewall delete rule name=\"" + appName + "\"";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string s = p.StandardOutput.ReadToEnd();
                logger.Info("Eliminazione regola Firewall per " + appName);
                logger.Info("Parametri: " + p.StartInfo.Arguments);
                logger.Info("Risultato: " + s);
            }
            catch (Exception ex)
            {
                logger.Error("Durante eliminazione regola firewall: " + ex);
            }
        }

        /// <summary>
        /// Apre il Firewall per uno specifico Eseguibile
        /// </summary>
        /// <param name="appName">Nome della regola sul Firewall </param>
        /// <param name="exePath">Path all'eseguibile, di default l'app stessa</param>
        /// <param name="action">allow or deny rule</param>
        /// <param name="direction">Direzione di apertura del Firewall: ingresso od in uscita in/out </param>
        public static void FirewallNetshExe(string appName = null, string exePath = null, string action = "allow", string direction = "in")
        {
            if (exePath == null)
            {
                exePath = Environment.GetCommandLineArgs()[0];
            }
            if (appName == null)
            {
                appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            }
            Process p = new Process();
            p.StartInfo.FileName = "netsh";
            // Siamo su XP
            if (Environment.OSVersion.Version.Major < 6)
            {
                p.StartInfo.Arguments = "firewall add allowedprogram program=\"" + exePath + " \" " + "name=\" " + appName + " \" " + "ENABLE ALL";
            }
            else p.StartInfo.Arguments = "advfirewall firewall add rule name=\"" + appName + "\"  dir=" + direction
                + " action=" + action.ToLower() + " program=\"" + exePath + "\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            string s = p.StandardOutput.ReadToEnd();
            logger.Info("Impostazione regola Firewall per " + appName);
            logger.Info("Parametri: " + p.StartInfo.Arguments);
            logger.Info("Risultato: " + s);
        }

        #endregion
        # region UPNP Stuff
        #endregion
    }
}
