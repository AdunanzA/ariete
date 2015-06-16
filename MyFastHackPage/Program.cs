using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
//using HtmlAgilityPack;
using System.Timers;
using System.Threading;
using System.Web;
using System.Net.Sockets;
using System.IO;
using IniParser;
using ScrapySharp.Core;
using ScrapySharp.Html.Parsing;
using ScrapySharp.Extensions;
using HtmlAgilityPack;

namespace MyFastPageHack
{

    class MyWebClient : WebClient
    {
        public CookieContainer _mContainer = new CookieContainer();
        public Uri _responseUri;
        public Dictionary<string, string> responseParams = new Dictionary<string, string>();

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = _mContainer;
            }
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);
            if (response is HttpWebResponse)
            {
                _mContainer.Add((response as HttpWebResponse).Cookies);
                _responseUri = response.ResponseUri;

                var query = HttpUtility.ParseQueryString(response.ResponseUri.Query);
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var param in query)
                {
                    try
                    {
                        if ((param != null) && (query.GetValues(param.ToString()) != null))
                        {
                            responseParams.Add(param.ToString(), query.Get(param.ToString()));
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Key gia' esistente nel dizionario");
                    }
                }
            }
            return response;
        }

        public void ClearCookies()
        {
            _mContainer = new CookieContainer();
        }
    }

    class Program
    {
        // Settings
        string BaseURI = "http://www.fastweb.it/myfastpage/goto/momi/?id=cfg-ngrg";
        string MyIpUrl = "http://evolution.adunanza.net/dhtchat/getmyip.php";
        string response;
        CookieContainer cookieContainer = new CookieContainer();
        MyWebClient wc;
        public Program()
        {
            wc = new MyWebClient();


        }
        static void Main(string[] args)
        {
            Program p = new Program();

            Console.Title = "MyFastHackPage";
            Console.Clear();

            try
            {
                Console.WindowHeight = 50;
                Console.WindowWidth = 120;

            }
            catch (Exception)
            {
                Console.WindowHeight = Console.LargestWindowHeight;
                Console.WindowWidth = Console.LargestWindowWidth;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("###############################");
            Console.WriteLine("#  MyFastHackPage by AduTeam  #");
            Console.WriteLine("###############################" + Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Gray;

            if (args.Length > 0)
            {
                p.BaseURI = args[0].ToString();
            }
            Console.WriteLine("Premere un tasto per continuare");
            Console.ReadKey();
            //p.GetForwardRulesHtml();
            //p.GetComputersFromHtml();
            Console.WriteLine("Current O.S.: {0}", Environment.OSVersion);
            
            if (p.GetOperatingSystem().Contains("Microsoft"))
            {
                Console.WriteLine("Rilevamento eMule AdunanzA ...");
                Console.WriteLine("Premere un tasto per continuare.");
                Console.ReadKey();

                if (p.EmuleAdunanzAExist())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Versione: " + p.GetEmuleAdunanzAConfig("AppVersion"));

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Configurazione trovata in: " + p.GetEmuleAdunanzAConfigPath());

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("La porta TCP usata da eMule AdunanzA e': " + p.GetEmuleAdunanzATcpPort());
                    Console.WriteLine("La porta UDP usata da eMule AdunanzA e': " + p.GetEmuleAdunanzAConfiguredUdpPort());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("eMule AdunanzA non sembra essere installato su questo computer.");
                }
                Console.WriteLine("Premere un tasto per continuare");
                Console.ReadKey();

                if (!p.IsFastwebIP())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Questo programma puo' funzionare SOLO su Rete Fastweb e con determinate linee di nuova generazione.");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Il tuo indirizzo IP esterno e': " + p.GetIpAddressExt());
                    Console.WriteLine("Il tuo indirizzo IP interno e': " + p.GetIpAddress() + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Il tuo indirizzo IP non ci risulta essere pubblico.");
                    Console.WriteLine("Se credi ci sia un errore contattaci sul nostro Forum all'indirizzo:");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("forum.adunanza.net" + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Il tuo indirizzo IP esterno e': " + p.GetIpAddressExt());
                    Console.WriteLine("Il tuo indirizzo IP interno e': " + p.GetIpAddress() + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Il tuo indirizzo IP non ci risulta essere pubblico.");
                    Console.WriteLine("Se credi ci sia un errore contattaci sul nostro Forum all'indirizzo:");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("forum.adunanza.net" + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                Console.WriteLine("Premere un tasto per continuare.");
                Console.ReadKey();

            }
            if (p.GetOperatingSystem().Contains("Android"))
            {
                Console.WriteLine("Rilevamento aMule AdunanzA in corso..." + Environment.NewLine);
                if (p.AmuleAdunanzAExist())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Versione: " + p.GetAmuleAdunanzAConfig("AppVersion"));

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Configurazione trovata in: " + p.GetAmuleAdunanzAConfigPath());

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("La porta TCP usata da aMule AdunanzA e': " + p.GetAmuleAdunanzAConfiguredTcpPort());
                    Console.WriteLine("La porta UDP usata da aMule AdunanzA e': " + p.GetAmuleAdunanzAConfiguredUdpPort());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("aMule AdunanzA non sembra essere installato su questo computer.");
                }
            }

            Console.WriteLine("Premere un tasto per chiudere il programma.");
            Console.ReadKey();
            Environment.Exit(1);
            
            try
            {
                Console.WriteLine("Apertura porte in corso..." + Environment.NewLine);

                p.GetHTTP("http://www.fastweb.it/myfastpage/?visore-portale=link-myfastpage");
                p.GetHTTP("http://www.fastweb.it/myfastpage/accesso/?DirectLink=%2Fmyfastpage%2F%3Fvisore-portale%3Dlink-myfastpage");
                p.GetHTTP("http://www.fastweb.it/myfastpage/abbonamento/#mConfig");
                p.GetHTTP("http://www.fastweb.it/myfastpage/goto/?id=CFG-NGRG&u=%2Fmyfastpage%2Fgoto%2Fmomi%2F%3Fid%3Dcfg-ngrg");

                if (p.wc.responseParams.ContainsKey("checksum"))
                {
                    string str = "http://fastmomi.fastweb.it/consolle.php?inside=1&account=" + p.wc.responseParams["account"]
                        + "&service=cfg-ngrg&channel=MYFP&checksum=" + p.wc.responseParams["checksum"];
                    p.GetHTTP(str);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Qualcosa e' andato storto...");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Scegliere la procedura premendo il numero corrispondente:");
                Console.WriteLine("1. Procedura Automatica");
                Console.WriteLine("2. Procedura Manuale");
                string keyPressed = Console.ReadLine();
                switch (keyPressed)
                {
                    case "1":
                        p.AutomaticSetup();
                        break;
                    case "2":
                        p.ManualSetup();
                        break;
                    default:
                        break;
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Premere un tasto per terminare il programma");
                Console.ReadKey();
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message); ;
            }
        }

        private string GetAmuleAdunanzAConfig(string p)
        {
            throw new NotImplementedException();
        }

        private string GetAmuleAdunanzAConfiguredUdpPort()
        {
            throw new NotImplementedException();
        }

        private string GetAmuleAdunanzAConfiguredTcpPort()
        {
            throw new NotImplementedException();
        }

        private string GetAmuleAdunanzAConfigPath()
        {
            throw new NotImplementedException();
        }

        private bool AmuleAdunanzAExist()
        {
            throw new NotImplementedException();
        }

        private string GetOperatingSystem()
        {
            return Environment.OSVersion.ToString();
        }

        private void ManualSetup()
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
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("http://fastmomi.fastweb.it/app/services/cfg-ngrg/RES-Bought.php?" + sb.ToString());

                PostHttp(wc, "http://fastmomi.fastweb.it/app/services/cfg-ngrg/RES-Bought.php?", sb.ToString());
                Console.ReadKey();
                // per ora non scriviamo la risposta
                //Console.WriteLine(response);
                //File.WriteAllText("response.html", response);

                Dictionary<string, string> dic = GetComputersFromHtml();
                string mymac = dic[GetIpAddress()];

                wc.responseParams["pmaction"] = "UPDATE";
                //wc.responseParams["ipCpe"]; //gia assegnato
                wc.responseParams["ID"] = ""; //chissa' che cosa e'
                wc.responseParams["ExternalPort"] = GetEmuleAdunanzATcpPort();
                wc.responseParams["InternalPort"] = GetEmuleAdunanzATcpPort();
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
                response = wc.UploadString("http://fastmomi.fastweb.it/app/services/cfg-ngrg/Ajax_writePortMapping.php", "POST", sb2.ToString());

                Console.WriteLine(response);
                Console.WriteLine("Premere un tasto per la POST di apertura porte");
                Console.ReadKey();

                // SIAMO QUI
                if (wc._responseUri.ToString().Contains("esito") && wc._responseUri.ToString().Contains("OK"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Procedura apertura porte automatica avvenuta con successo!");

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Il tuo indirizzo IP esterno e' :" + GetIpAddressExt());
                    Console.WriteLine("Il tuo indirizzo IP interno e' :" + GetIpAddress() + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("Usa le seguenti porte con eMule AdunanzA inserendole in Opzioni -> Connessione");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("TCP: " + GetTcpPortToUse(GetIpAddress()));
                    Console.WriteLine("UDP: " + GetUdpPortToUse(GetIpAddress()));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Procedura apertura porte automatica Fallita.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Procedura apertura porte automatica Fallita.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private Dictionary<string, string> GetComputersFromHtml()
        {
            //Read from file for now
            StreamReader sr = new StreamReader("response.html");
            response = File.ReadAllText("response.html");

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

        private void GetForwardRulesHtml()
        {
            //Read from file for now
            StreamReader sr = new StreamReader("response.html");
            response = File.ReadAllText("response.html");

            HtmlNode html;
            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            html = doc.DocumentNode;
            //*[@id="pmitem_MacAddress_1"] //*[@id="FORM1"]/table/tbody/tr/td/div[2]/table[2]/tbody
            //var inputs = html.CssSelect("input[type=hidden] #pmitem");
            // parse della tabella
            // xpath ff: //*[@id="FORM1"]/table/tbody/tr/td/div[2]/table[2]
            // //table/tbody/tr/td/div[2]/table[2]
            var asd = html.CssSelect("table[width=860px]");
            var q = from table in doc.DocumentNode.SelectNodes("//table/tbody").Cast<HtmlNode>()
                    from row in table.SelectNodes("tr").Cast<HtmlNode>()
                    from cell in row.SelectNodes("th|td").Cast<HtmlNode>()
                    select new { Table = table.Id, CellText = cell.InnerText };

            foreach (var cell in q)
            {
                Console.WriteLine("{0}: {1}", cell.Table, cell.CellText);
            }
        }
        private void AutomaticSetup()
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
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("http://fastmomi.fastweb.it/app/services/cfg-ngrg/RES-Bought.php?" + sb.ToString());
                /*http://fastmomi.fastweb.it/app/services/cfg-ngrg/RES-Bought.php?current=2&step=3&ctrl=&portmap_action=manual_setup&channel=MYFP&account=3871035&username=antonello.mereu0001&service=cfg-ngrg&actionid=&status=Bought&segmento=RES&selcode=DR.4006.0101MI&origin=cfg-ngrg&checksum=16e4ec232fb8905ca043f93058a6aca1&identifycode=93.50.105.220&previous=1&aVarsParam=*/
                PostHttp(wc, "http://fastmomi.fastweb.it/app/services/cfg-ngrg/RES-Bought.php?", sb.ToString());
                if (wc._responseUri.ToString().Contains("esito") && wc._responseUri.ToString().Contains("OK"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Procedura apertura porte automatica avvenuta con successo!");

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Il tuo indirizzo IP esterno e' :" + GetIpAddressExt());
                    Console.WriteLine("Il tuo indirizzo IP interno e' :" + GetIpAddress() + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("Usa le seguenti porte con eMule AdunanzA inserendole in Opzioni -> Connessione");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("TCP: " + GetTcpPortToUse(GetIpAddress()));
                    Console.WriteLine("UDP: " + GetUdpPortToUse(GetIpAddress()));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Procedura apertura porte automatica Fallita.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Procedura apertura porte automatica Fallita.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private string GetEmuleAdunanzAConfig(string key)
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.LoadFile(GetEmuleAdunanzAConfigPath());

            return data["eMule"][key];
        }

        private string GetEmuleAdunanzATcpPort()
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.LoadFile(GetEmuleAdunanzAConfigPath());
            
            return data["eMule"]["Port"];
        }

        private void SetEmuleAdunanzATcpPort(int port)
        {
            if ((port > 65554) && (port == 0))
            {
                throw new Exception("la Porta TCP da configurare non e' valida");
            }    
            
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.LoadFile(GetEmuleAdunanzAConfigPath());
            data["eMule"]["Port"] = port.ToString();
        }

        private string GetEmuleAdunanzAConfiguredUdpPort()
        {
            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.LoadFile(GetEmuleAdunanzAConfigPath());

            return data["eMule"]["UDPPort"];
        }

        private void SetEmuleAdunanzAUdpPort(int port)
        {
            if ((port > 65554) && (port == 0))
            {
                throw new Exception("la Porta UDP da configurare non e' valida");
            }

            //Create an instance of a ini file parser
            IniParser.FileIniDataParser iniparser = new FileIniDataParser();

            IniData data = iniparser.LoadFile(GetEmuleAdunanzAConfigPath());
            data["eMule"]["UDPPort"] = port.ToString();
        }

        private bool EmuleAdunanzAExist()
        {
            if (File.Exists(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                    Path.DirectorySeparatorChar + Path.Combine("eMule AdunanzA", "Config") +
                    Path.DirectorySeparatorChar + "preferences.ini"
                    )
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private string GetEmuleAdunanzAConfigPath()
        {
            if (File.Exists(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                    Path.DirectorySeparatorChar + Path.Combine("eMule AdunanzA", "Config") +
                    Path.DirectorySeparatorChar + "preferences.ini"
                    )
                )
            {
                return (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                    Path.DirectorySeparatorChar + Path.Combine("eMule AdunanzA", "Config") +
                    Path.DirectorySeparatorChar + "preferences.ini");
            }
            else
            {
                return null;
            }
        }


        private string GetIpAddressExt()
        {
            WebClient wc2 = new WebClient();
            return wc2.DownloadString(MyIpUrl);
        }

        private string GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string ipv4 = (from ip in host.AddressList
                           where ip.AddressFamily == AddressFamily.InterNetwork &&
                                 ip.ToString().StartsWith("192")
                           select ip.ToString()).First();
            return ipv4;
        }
        private int GetUdpPortToUse(string ipv4)
        {
            return (GetTcpPortToUse(ipv4) + 10);
        }

        private int GetTcpPortToUse(string ipv4)
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
                default:
                    return 0;
            }
        }

        private MyWebClient PostHttp(MyWebClient wc, string baseUrl, string paramConcat)
        {
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            response = wc.UploadString(baseUrl, paramConcat);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Environment.NewLine + "Scaricata prima pagina: {0}", BaseURI);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("ResponseHeaders:" + Environment.NewLine + "{0}", wc.ResponseHeaders);
            var cc = wc._mContainer.GetCookies(new Uri("http://www.fastweb.it"));
            foreach (var cookie in cc)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Cookie: {0}", cookie);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Redirect Location: {0}", wc._responseUri);

            foreach (var param in wc.responseParams)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Fragment: {0} = {1}", param.Key, param.Value);
            }
            return wc;
        }

        private bool IsFastwebIP()
        {
            // Controllo se l'ip esterno e' un Fastweb pubblico di nuova generazione
            string ip = GetIpAddressExt();
            if (ip.StartsWith("93.4") ||
#if DEBUG
 ip.StartsWith("93") || //aggiunto per debug da rimuovere
#endif
                ip.StartsWith("93.5") ||
                ip.StartsWith("93.6") ||
                ip.StartsWith("2.22") ||
                ip.StartsWith("2.23") ||
                ip.StartsWith("2.24") ||
                ip.StartsWith("2.25"))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private MyWebClient GetHTTP(string url)
        {
            response = wc.DownloadString(new Uri(url));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Environment.NewLine + "Scaricata prima pagina: {0}", BaseURI);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("ResponseHeaders:" + Environment.NewLine + "{0}", wc.ResponseHeaders);
            var cc = wc._mContainer.GetCookies(new Uri("http://www.fastweb.it"));
            foreach (var cookie in cc)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Cookie: {0}", cookie);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Redirect Location: {0}", wc._responseUri);

            foreach (var param in wc.responseParams)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Fragment: {0} = {1}", param.Key, param.Value);
            }
            return wc;
        }

        private static string ParseFastwebUrl(string url)
        {
            if (url.Contains("../../"))
            {
                url = url.Replace("../../", @"http://www.fastweb.it/myfastpage/");
            }
            return url;
        }
    }
}