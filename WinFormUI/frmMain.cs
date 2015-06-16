using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using Ariete.Core;
using System.Net;
using System.Web;
using Mono.Nat;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Deployment.Application;
using System.Reflection;

namespace Ariete.WinFormUI
{
    public partial class frmMain : Form
    {
        Form CMessageBox;

        // Log purpose
        public static Logger logger;
        MyWebClient wc;
        CookieContainer cookieContainer;
        bool PortMappingOkTcp = false;
        bool PortMappingOkUdp = false;
        string tcpport = "0";
        string udpport = "0";
        int volte = 0;
        string ipExt;
        string ipInt;
        string ipFastweb;
        string appVer;

        // Create an instance of the open file dialog box.
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            logger = LogManager.GetLogger("ArieteMainFormUI");
            logger.Info("MainFormUI created.");
            cookieContainer = new CookieContainer();
            wc = new MyWebClient();
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.DeviceLost += DeviceLost;

            // Pulizia e creazione regole sul firewall per Ariete
            if (Utils.FirewallNetshIsRule())
            {
                Utils.FirewallNetshDeleteRule();
            }
            Utils.FirewallNetshExe();

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                this.Text += " v" + ad.CurrentVersion.ToString();
            }
            else
                this.Text +=  " Debug";
        }

        private String BuildFormTitle()
        {
            String AppName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            String FormTitle = String.Format("{0} {1} ({2})",
                                             AppName,
                                             Application.ProductName,
                                             Application.ProductVersion);
            return FormTitle;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            this.UseWaitCursor = true;
            var t = new Thread(() =>
            {
                try { Start(); }
                catch (Exception ex) { logger.Fatal(ex.Message); }
            });
            t.Name = "Ariete";
            t.Start();
            logger.Info("-------------------------------------------------------------");

            logger.Info("Pulsante Avvia Cliccato. Tentativo numero {0}", ++volte);
            lblDesc.Text = "Questa procedura potrebbe necessitare di 20-40 secondi circa. Attendere...";
        }

        private void Start()
        {
            logger.Info("Sistema Operativo rilevato: " + Environment.OSVersion);
            if (Environment.OSVersion.ToString().Contains("Microsoft"))
            {
                logger.Debug("Rilevamento eMule AdunanzA ...");

                if (Utils.EmuleAdunanzAExist())
                {
                    // Gestire il caso di emule normale invece di Adu
                    appVer = Utils.EmuleAdunanzaConfigGetKey("AppVersion");
                    logger.Info("Versione: " + appVer);

                    logger.Info("Configurazione trovata in: " + Utils.EmuleAdunanzAConfigFileGetPath());
                    logger.Info("Programma trovato in: " + Utils.EmuleAdunanzAExeGetPath());

                    tcpport = Utils.EmuleAdunanzaGetTcpPort();
                    udpport = Utils.EmuleAdunanzaGetUdpPort();
                    logger.Info("La porta TCP usata da eMule AdunanzA e': " + tcpport);
                    logger.Info("La porta UDP usata da eMule AdunanzA e': " + udpport);
                }
                else
                {
                    ExitFail("I2", "eMule AdunanzA non sembra essere installato su questo computer." + Environment.NewLine
                        + "Puoi installare eMule AdunanzA da qui: http://www.adunanza.net/pagina/download-emule-adunanza.html e rilanciare Ariete una volta installato!");
                    return;
                }

                if (!Utils.IsFastweb())
                {
                    ipExt = Utils.GetIpAddressExt();
                    ipInt = Utils.GetIpAddress();
                    ipFastweb = Utils.GetIpAddressFastweb();

                    rtbSummary.Invoke(new Action(() => rtbSummary.Text = "IPv4: "));
                    rtbSummary.Invoke(new Action(() => rtbSummary.AppendText(ipInt)));

                    logger.Info("Il tuo indirizzo IP visto dalla rete esterna e': " + ipExt);
                    logger.Info("Il tuo indirizzo IP visto dalla tua rete interna e': " + ipInt + Environment.NewLine);

                    ExitFail("R1", "Questo programma puo' funzionare SOLO su Rete Fastweb.");

                    return;
                }
                // Se siamo Fastweb ed abbiamo IP 100 allora usciamo con l'avviso di richiedere IP pubblico.
                else if (Utils.IsFastweb() & Utils.GetIpAddressFastweb().StartsWith("100"))
                {
                    ExitFail100();
                }
                else
                {
                    // Siamo Fastweb
                    ipExt = Utils.GetIpAddressExt();
                    ipInt = Utils.GetIpAddress();
                    ipFastweb = Utils.GetIpAddressFastweb();

                    rtbSummary.Invoke(new Action(() => rtbSummary.Text = "IPv4: "));
                    rtbSummary.Invoke(new Action(() => rtbSummary.AppendText(ipInt)));

                    logger.Info("Linea Fastweb Rilevata.");
                    logger.Info("Il tuo indirizzo IP visto dalla rete esterna e': " + ipExt);
                    logger.Info("Il tuo indirizzo IP visto dalla rete Fastweb e': " + ipFastweb);
                    logger.Info("Il tuo indirizzo IP visto dalla tua rete interna e': " + ipInt + Environment.NewLine);
                    if (!Utils.IsFastwebPublicIP())
                    {
                        logger.Error("Ip pubblico non rilevato! La tua linea è limitata. Chiedi a Fastweb di cambiartela seguendo questa procedura gratuita:                                 http://forum.adunanza.net/threads/91608-GUIDA-Richiesta-indirizzo-IP-Pubblico");
                    }
                    // Disattivo UPNP e modifico la Conf come serve ad ariete
                    Utils.EmuleAdunanzaKill();
                    Utils.EmuleAdunanzaConfigSetKey("UPnPNAT", "0");

                    // Pulisco ed Apro le porte del Firewall di win per eMule AdunanzA
                    if (Utils.FirewallNetshIsRule("eMule AdunanzA"))
                    {
                        Utils.FirewallNetshDeleteRule("eMule AdunanzA", Utils.EmuleAdunanzAExeGetPath());
                    }
                    Utils.FirewallNetshExe("eMule AdunanzA", Utils.EmuleAdunanzAExeGetPath());

                    // Controllo Adutest delle porte
                    var result = Utils.AduTestDo();
                    if (Utils.AduTestTcpGetResult(result) & Utils.AduTestUdpGetResult(result))
                    {
                        ExitWin();
                    }
                    else
                    {
                        // Provo con UPNP
                        //devo usare porte differenti dal portmapping Fastweb
                        Random rn = new Random();
                        int rPort = rn.Next(4679, 65534);

                        tcpport = rPort.ToString();
                        udpport = (rPort + 1).ToString();
                        NatUtility.StartDiscovery();

                        System.Threading.Thread.Sleep(15000);

                        if (PortMappingOkTcp & PortMappingOkUdp)
                        {
                            result = Utils.AduTestDo(int.Parse(tcpport), int.Parse(udpport));
                            if (Utils.AduTestTcpGetResult(result) & Utils.AduTestUdpGetResult(result))
                            {
                                Utils.EmuleAdunanzAChangePortsMFP(int.Parse(tcpport), int.Parse(udpport));
                                logger.Info("Adutest delle porte Ora e' Superato");
                                ExitWin();
                            }
                            else
                            {
                                ExitFail("U1", "Qualcosa e' andato storto: nonostante UPNP riuscito le porte risultano ancora chiuse.");
                            }
                        }
                        // Provo la MyFastPage
                        // TODO: controllare che esista il link sulla mfp per aprire il router
                        else if (Utils.CanUseMyFastPage())
                        {
                            logger.Info("Linea Configurabile dalla MyfastPage rilevata.");
                            var res = MessageBox.Show("Ariete puo' configurare automaticamente per Te le porte sulla MyFastPage eseguendo la procedura rapida. Consenti di farlo?", "AduTips", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (res == System.Windows.Forms.DialogResult.Yes)
                            {
                                if (StartMyFastPageHack("auto"))
                                {
                                    Utils.EmuleAdunanzAChangePortsMFP();

                                    result = Utils.AduTestDo();
                                    if (Utils.AduTestTcpGetResult(result) & Utils.AduTestUdpGetResult(result))
                                    {
                                        logger.Info("Adutest delle porte Ora e' Superato");
                                        ExitWin();
                                    }
                                    else
                                    {
                                        ExitFail("F1", "Qualcosa e' andato storto: nonostante la MFP sia diventata come il Colosseo, le porte risultano ancora inaccessibili.");
                                    }
                                }
                                else
                                {
                                    ExitFail("F3", "Qualcosa e' andato storto: durante l'apertuta sulla MFP");
                                }

                            }
                            else
                            {
                                logger.Info("Procedura Rapida MFP rifiutata dall'utente.");
                                // Dovrei lanciare il manuale impostando le porte gia' settate nel mulo
                                // Ha fallito l'apertura automatica
                                //StartMyFastPageHack("manual");
                                ExitFail("F2", "Procedura Rapida MFP rifiutata dall'utente.");
                            }
                        }
                        else
                        {
                            ExitFail("C1", "Ariete non può più aiutarti. Probabilmente stai utilizzando apparati aggiutivi oltre a quello in dotazione Fastweb, oppure la tua linea è bloccata da qualche Firewall od Antivirus mal configurato. Chiedi aiuto ai nostri volontari sul forum di asistenza: http://forum.adunanza.net/threads/52878-Cerchi-Aiuto-Supporto");
                        }
                    }
                }
            }
            #region Adubox
            else if (Utils.GetOperatingSystem().Contains("Unix"))
            {
                logger.Debug("Rilevamento aMule");
                if (Utils.AmuleAdunanzaAduboxExist())
                {
                    logger.Info("Versione: " + Utils.AmuleAdunanzaAduboxConfigGetKey("AppVersion"));

                    logger.Info("Configurazione trovata in: " + Utils.AmuleAdunanzaAduboxConfigFileGetPath());
                    logger.Info("Programma trovato in: " + Utils.AmuleAdunanzABinGetPath());

                    tcpport = Utils.AmuleAdunanzaAduBoxGetTcpPort();
                    udpport = Utils.AmuleAdunanzaAduboxGetUdpPort();
                    logger.Info("La porta TCP usata da aMule AdunanzA e': " + tcpport);
                    logger.Info("La porta UDP usata da aMule AdunanzA e': " + udpport);
                }
                else
                {
                    logger.Fatal("aMule AdunanzA non sembra essere installato su questo computer.");
                    logger.Fatal("Puoi installare aMule AdunanzA da qui: http://www.adunanza.net/pagina/download-emule-adunanza.html"
                     + "e rilanciare Ariete una volta installato!");
                    return;
                }
                if (!Utils.IsFastweb())
                {
                    ////Console.ForegroundColor = ConsoleColor.Red;
                    logger.Fatal("Questo programma puo' funzionare SOLO su Rete Fastweb e con determinate linee di nuova generazione.");
                    ////Console.ForegroundColor = ConsoleColor.Magenta;
                    logger.Info("Il tuo indirizzo IP visto dalla rete esterna e': " + Utils.GetIpAddressExt());
                    logger.Info("Il tuo indirizzo IP visto dalla tua rete interna e': " + Utils.GetIpAddress() + Environment.NewLine);

                    logger.Error("Il tuo indirizzo IP non ci risulta essere pubblico.");
                    logger.Info("Se credi ci sia un errore contattaci sul nostro Forum all'indirizzo: http://forum.adunanza.net");
                    return;
                }
                else
                {
                    logger.Info("Linea Fastweb Rilevata.");
                    logger.Info("Il tuo indirizzo IP visto dalla rete esterna e': " + Utils.GetIpAddressExt());
                    logger.Info("Il tuo indirizzo IP visto dalla rete Fastweb e': " + Utils.GetIpAddressFastweb());
                    logger.Info("Il tuo indirizzo IP visto dalla tua rete interna e': " + Utils.GetIpAddress() + Environment.NewLine);
                    if (!Utils.IsFastwebPublicIP())
                    {
                        logger.Error("Ip pubblico non rilevato! La tua linea è limitata. Chiedi a Fastweb di cambiartela seguendo questa procedura:                                 http://forum.adunanza.net/threads/91608-GUIDA-Richiesta-indirizzo-IP-Pubblico");
                    }
                    // Controllo Adutest delle porte
                    // Mi devo assicurare che eMuleAdunanzA sia in funzione altrimenti lo lancio
                    Utils.AmuleAdunanzaStart();

                    // Diamo un minimo di tempo ad Emule per partire altrimenti fallisce il test porte
                    System.Threading.Thread.Sleep(5000);

                    var result = Utils.AduTestDoAdubox();
                    if (Utils.AduTestTcpAduboxGetResult(result) & Utils.AduTestUdpAduboxGetResult(result))
                    {
                        logger.Info("Adutest delle porte Superato");
                        logger.Info("Ariete ha liberato la strada. Ora si ritira.");
                    }
                    else
                    {
                        // Provo con UPNP                        
                        //devo usare porte differenti dal portmapping Fastweb
                        Random rn = new Random();
                        int rPort = rn.Next(4679, 65534);

                        tcpport = rPort.ToString();
                        udpport = (rPort + 1).ToString();

                        NatUtility.StartDiscovery();
                        System.Threading.Thread.Sleep(8000);
                        NatUtility.StopDiscovery();

                        if (PortMappingOkTcp & PortMappingOkUdp)
                        {
                            Utils.AmuleAdunanzaAduboxKill();
                            Utils.AmuleAdunanzaChangePortsMFP(int.Parse(tcpport), int.Parse(udpport));
                            Utils.AmuleAdunanzaStart();

                            result = Utils.AduTestDoAdubox();
                            if (Utils.AduTestTcpAduboxGetResult(result) & Utils.AduTestUdpAduboxGetResult(result))
                            {
                                logger.Info("Adutest delle porte Ora e' Superato");
                                logger.Info("Ariete ha liberato la strada. Ora si ritira vittorioso.");
                            }
                            else
                            {
                                logger.Fatal("Qualcosa e' andato storto: nonostante UPNP riuscito le porte sono ancora chiuse.");
                                logger.Fatal("Ariete ha fatto il possibile, ma ha fallito. Ora si ritira sconfitto.");
                            }
                        }
                        else if (Utils.CanUseMyFastPage())
                        {
                            logger.Info("Linea Configurabile dalla MyfastPage rilevata.");
                            var res = MessageBox.Show("Ariete puo' configurare automaticamente per Te le porte sulla MyFastPage eseguendo la procedura rapida. Consenti di farlo?", "AduTips", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                            if ((res == System.Windows.Forms.DialogResult.OK) && StartMyFastPageHack("auto"))
                            {
                                // potrei rifare il test porte per conferma
                                Utils.AmuleAdunanzaAduboxKill();
                                Utils.EmuleAdunanzAChangePortsMFP();
                                Utils.AmuleAdunanzaStart();

                                logger.Info("Ariete ha liberato la strada. Ora si ritira.");
                            }
                            else
                            {
                                // Dovrei lanciare il manuale impostando le porte gia' settate nel mulo
                                // Ha fallito l'apertura automatica
                                //StartMyFastPageHack("manual");
                                logger.Info("Ariete ha fatto il possibile, ma ha fallito. Ora si ritira sconfitto.");
                            }

                        }
                        else
                        {
                            // Problema di antivirus avvertire l'utente
                            logger.Info("Ariete non può più aiutarti. Probabilmente la tua linea è bloccata da qualche Firewall od antivirus non                                        configurato correttamente. Chiedi aiuto ai nostri volontari sul forum di asistenza: http://forum.adunanza.net/threads/52878-Cerchi-Aiuto-Supporto");
                            MessageBox.Show("Ariete non può più aiutarti. Probabilmente la tua linea è bloccata da qualche Firewall od antivirus configurato correttamente. Chiedi aiuto ai nostri volontari sul forum di asistenza: http://forum.adunanza.net/threads/52878-Cerchi-Aiuto-Supporto", "AduTips", MessageBoxButtons.OK, MessageBoxIcon.Question);
                        }
                    } //fine upnp/myfastpagehack
                } //fine lineafastweb
            } // fine unix
            #endregion Adubox
        }

        private void ExitFail100()
        {
            // Riattivo UPNP in caso di fallimento per facilitare l'aiuto in chat a ricavare l'UP
            if (Utils.EmuleAdunanzAConfigExist())
            {
                Utils.EmuleAdunanzaConfigSetKey("UPnPNAT", "1");
            }

            int lastOct = 0;
            if (ipInt != null)
            {
                lastOct = int.Parse(ipInt.Split('.').Last());
            }
            frmMain.ActiveForm.Invoke(new Action(() => this.UseWaitCursor = false));
            frmMain.ActiveForm.Invoke(new Action(() => btnStart.Enabled = true));
            frmMain.ActiveForm.Invoke(new Action(() => rtbSummary.AppendText(Environment.NewLine + "Errore: R2"  + "-" + lastOct.ToString())));
            logger.Fatal("Rilevato IP 100 incompatibile con eMule AdunanzA");
            logger.Info("Ariete ha fatto il possibile, ma ha fallito. Ora si ritira sconfitto.");

            CMessageBox = new CustomMessageBox("Attenzione! La tua line non e' compatibile con AdunanzA!", "Abbiamo rilevato che l'indirizzo IP (100.) della tua linea non e' compatibile con eMule AdunanzA. " + Environment.NewLine + Environment.NewLine
                + "Chiedi a Fastweb di aggiornare gratuitamente la tua linea come descritto in questa procedura: " + "http://forum.adunanza.net/threads/91608-GUIDA-Richiesta-indirizzo-IP-Pubblico" + Environment.NewLine + Environment.NewLine
                + "Dopo aver effettuato la richiesta, entro tre giorni la tua linea dovrebbe essere aggiornata gratuitamente. " + Environment.NewLine
            + "Bastera' rilanciare nuovamente Ariete per terminare la configurazione.", "http://forum.adunanza.net/threads/91608-GUIDA-Richiesta-indirizzo-IP-Pubblico");
            CMessageBox.ShowDialog();
        }
        
        private string AskPathExeToUser(String sw)
        {
            var res = MessageBox.Show(sw + "non e' stato trovato nelle cartelle standard di installazione. Vuoi fornirci tu il percorso esatto?", sw + " non trovato!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = "File Eseguibile (*.exe)|*.exe|Tutti i files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.AutoUpgradeEnabled = false;
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (File.Exists(openFileDialog1.FileName))
                    {
                        return openFileDialog1.FileName;
                    }
                }
            }
            return null;
        }

        private string AskPathConfigToUser(String sw)
        {
            Stream myStream = null;
            var res = MessageBox.Show("Il File di configurazione " + sw + "non e' stato trovato nelle cartelle standard di installazione. Vuoi fornirci tu il percorso esatto?", sw + " non trovato!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = sw + "|" + sw;
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.AutoUpgradeEnabled = false;
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Utils.EmuleAdunanzaConfigGetKey("AppVersion");
                    try
                    {
                        if ((myStream = openFileDialog1.OpenFile()) != null)
                        {
                            using (myStream)
                            {
                                // Insert code to read the stream here.
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);

                    }
                }
            }
            return null;
        }


        private void ExitWin()
        {
            frmMain.ActiveForm.Invoke(new Action(() => btnStart.Enabled = true));
            frmMain.ActiveForm.Invoke(new Action(() => this.UseWaitCursor = false));

            logger.Info("Ariete ha liberato la strada. Ora si ritira vittorioso.");
            var res = MessageBox.Show("La configurazione automatica e' avvenuta con successo!" + Environment.NewLine
                + "Ricorda che eMule AdunanzA le prime volte ci mettera' almeno 10 min a collegarsi alla rete kAdu. " + Environment.NewLine
                + "Nel mentre, risultera' firewalled ma niente paura! E' normale." + Environment.NewLine
                + "Ora, vuoi lanciare eMule AdunanzA?", "Vittoria! Ariete ha liberato la strada con successo.", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                Utils.EmuleAdunanzAStart();
                if (System.Windows.Forms.Application.MessageLoop)
                {
                    // Use this since we are a WinForms app
                    Application.Exit();
                }
                else
                {
                    // Use this since we are a console app
                    Environment.Exit(1);
                }
            }
        }

        private void ExitFail(string errorCode, string errorMsg)
        {
            // Riattivo UPNP in caso di fallimento per facilitare l'aiuto in chat a ricavare l'UP
            if (Utils.EmuleAdunanzAConfigExist())
            {
                Utils.EmuleAdunanzaConfigSetKey("UPnPNAT", "1");
            }

            int lastOct = 0;
            if (ipInt != null)
            {
                lastOct = int.Parse(ipInt.Split('.').Last());
            }
            frmMain.ActiveForm.Invoke(new Action(() => this.UseWaitCursor = false));
            frmMain.ActiveForm.Invoke(new Action(() => btnStart.Enabled = true));
            frmMain.ActiveForm.Invoke(new Action(() => rtbSummary.AppendText(Environment.NewLine + "Errore: " + errorCode + "-" + lastOct.ToString())));
            logger.Fatal(errorMsg);
            logger.Info("Ariete ha fatto il possibile, ma ha fallito. Ora si ritira sconfitto.");

            var res = MessageBox.Show(errorMsg + Environment.NewLine
            + "Riportaci in chat questo Errore: " + errorCode + "-" + lastOct.ToString() + Environment.NewLine 
            + "Ora, vuoi lanciare eMule AdunanzA?",
            "Sconfitta! Ariete ha fallito, ora si ritira :(", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0, "http://forum.adunanza.net/threads/91608-GUIDA-Richiesta-indirizzo-IP-Pubblico");
            if (res == System.Windows.Forms.DialogResult.Yes)
                Utils.EmuleAdunanzAStart();

            // todo:risolvere il problema di chiusura
            // Purtroppo l'app non si chiude bene a causa dei listener che rimangono inspiegabilmente appesi in alcune situazioni pur settando le bool a true e quindi sono costretto a killare
            //In questo modo killo ariete e mi assicuro che tutto sia ok per lasciare le porte al mulo
            Process.GetCurrentProcess().Kill();

            // Vecchio codice per la chiusura normale
            //if (System.Windows.Forms.Application.MessageLoop)
            //{
            //    // Use this since we are a WinForms app
            //    Application.Exit();
            //}
            //else
            //{
            //    // Use this since we are a console app
            //    Environment.Exit(1);
            //}
        }

        private bool StartMyFastPageHack(string mode = "auto")
        {
            logger.Info("Inizio apertura porte in corso...");
            try
            {
                Utils.GetHttp("http://www.fastweb.it/myfastpage/?visore-portale=link-myfastpage", wc);
                Utils.GetHttp("http://www.fastweb.it/myfastpage/accesso/?DirectLink=%2Fmyfastpage%2F%3Fvisore-portale%3Dlink-myfastpage", wc);
                Utils.GetHttp("http://www.fastweb.it/myfastpage/abbonamento/#mConfig", wc);
                Utils.GetHttp("http://www.fastweb.it/myfastpage/goto/?id=CFG-NGRG&u=%2Fmyfastpage%2Fgoto%2Fmomi%2F%3Fid%3Dcfg-ngrg", wc);

                if (wc.responseParams.ContainsKey("checksum"))
                {
                    string str = "http://fastmomi.fastweb.it/consolle.php?inside=1&account=" + wc.responseParams["account"]
                        + "&service=cfg-ngrg&channel=MYFP&checksum=" + wc.responseParams["checksum"];
                    Utils.GetHttp(str, wc);
                }
                else
                {
                    logger.Info("Qualcosa e' andato storto...");
                    return false;
                }
            }
            catch (WebException ex)
            {
                logger.Info(ex.Message); ;
                return false;
            }

            if (mode == "manual")
            {
                return Utils.MFPManual(wc);
            }
            else return Utils.MFPConfigurazioneRapida(wc);
        }


        public void DeviceFound(object sender, DeviceEventArgs args)
        {
            try
            {
                INatDevice device = args.Device;

                logger.Info("Trovato dispositivo con UPNP abilitato.");
                logger.Info("Tipo: {0}", device.GetType().Name);
                logger.Info("IP Esterno del dispositivo: {0}", device.GetExternalIP());

                Mapping mapTcp = new Mapping(Protocol.Tcp, Convert.ToInt32(tcpport), Convert.ToInt32(tcpport));
                logger.Info("Creazione del PortMapping sul dispositivo UPNP: Protocollo={0}, Porta Public={1}, private={2}", mapTcp.Protocol, mapTcp.PublicPort, mapTcp.PrivatePort);
                device.CreatePortMap(mapTcp);

                Mapping mapUdp = new Mapping(Protocol.Udp, Convert.ToInt32(udpport), Convert.ToInt32(udpport));
                logger.Info("Creazione del PortMapping sul dispositivo UPNP: Protocollo={0}, Porta Public={1}, private={2}", mapUdp.Protocol, mapUdp.PublicPort, mapUdp.PrivatePort);
                device.CreatePortMap(mapUdp);

                Mapping mapTcp2 = device.GetSpecificMapping(Protocol.Tcp, Convert.ToInt32(tcpport));
                PortMappingOkTcp = true;
                logger.Info("Verifica del PortMapping Protocollo={0}, Porta={1} passata con successo", mapTcp2.Protocol, mapTcp2.PublicPort);

                Mapping mapUdp2 = device.GetSpecificMapping(Protocol.Udp, Convert.ToInt32(udpport));
                PortMappingOkUdp = true;
                logger.Info("Verifica del PortMapping Protocollo={0}, Porta={1} passata con successo", mapUdp2.Protocol, mapUdp2.PublicPort);

                // Se il portfoward funziona interrompiamo il discovery
                // NOTA: rileviamo solo il primo router della lista
                NatUtility.StopDiscovery();
            }
            catch (Exception ex)
            {
                logger.Fatal("Procedura UPNP Fallita.");

                logger.Fatal(ex.Message);
                logger.Fatal(ex.StackTrace);
            }
        }

        public void DeviceLost(object sender, DeviceEventArgs args)
        {
            INatDevice device = args.Device;

            logger.Fatal("Device Lost");
            logger.Fatal("Type: {0}", device.GetType().Name);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (StartMyFastPageHack("auto"))
            {
                Utils.EmuleAdunanzaKill();
                Utils.EmuleAdunanzAChangePortsMFP();
                Utils.EmuleAdunanzAStart();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Utils.EmuleAdunanzAStart();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Utils.EmuleAdunanzAStart();
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Dictionary<string, string> dic = Utils.GetComputersFromHtml(wc);
            //string mymac = dic[Utils.GetIpAddress()];
            Utils.MFPManual(wc);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Environment.OSVersion.VersionString.Contains("Microsoft"))
            {
                tcpport = Utils.EmuleAdunanzaGetTcpPort();
                udpport = Utils.EmuleAdunanzaGetUdpPort();
            }
            else
            {
                tcpport = Utils.AmuleAdunanzaAduBoxGetTcpPort();
                udpport = Utils.AmuleAdunanzaAduboxGetUdpPort();
            }
            logger.Info("Ricerca dispositivi upnp in corso");

            NatUtility.StartDiscovery();
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (Environment.Is64BitOperatingSystem)
            {
                logger.Info("%programfilex86% 64bit% " + Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"), "eMule AdunanzA", "Config",
               "preferences.ini"));
            }
            else
            {
                logger.Info("%programfile% 32bit" + Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES"), "eMule AdunanzA", "Config",
                "preferences.ini"));
            }
            logger.Info("%appdata% su 32" + Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "eMule AdunanzA", "Config",
                "preferences.ini"));
            logger.Info("programfilex86 > xp: " + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "eMule AdunanzA", "Config",
                "preferences.ini"));
            logger.Info("conf: " + Utils.EmuleAdunanzAConfigFileGetPath());
            logger.Info("exe: " + Utils.EmuleAdunanzAExeGetPath());
            logger.Info("esiste?  " + Utils.EmuleAdunanzAExist().ToString());

        }

        private void button5_Click(object sender, EventArgs e)
        {
            rtxtbLog.SelectAll();
            rtxtbLog.Copy();
        }

        private void button6_Click(object sender, EventArgs e)
        {

            Thread th2 = new Thread(() => Utils.StartListenerTcp());
            th2.Name = "ListenerTcpThread";
            th2.Start();

            Thread th = new Thread(() => Utils.StartListenerUdp());
            th.Name = "ListenrUdpThread";
            th.Start();

            var res = Utils.AduTestDo();
            logger.Info(res);
            Utils.AduTestTcpGetResult(res);
            Utils.AduTestUdpGetResult(res);

        }

        private void button7_Click(object sender, EventArgs e)
        {
            //AskPathExeToUser("eMule AdunanzA");
            logger.Info(Utils.EmuleAdunanzAConfigDirSearch("eMule_AdnzA.exe"));
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            clsFirewall fw = new clsFirewall();
            fw.OpenFirewallExe("Ariete");
            fw.OpenFirewallPort(666, "udp", "Ariete");
            try
            {
                Utils.StopUdpThread = true;
                Thread th = new Thread(() => Utils.StartListenerUdp(666));
                th.Name = "ListenrUdpThread";
                th.Start();
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);


            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            Process.GetCurrentProcess().Kill();

            // Confirm user wants to close
            //switch (MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
            //{
            //    case DialogResult.No:
            //        e.Cancel = true;
            //        logger.Info("chiusura annullata dall'utente");
            //        break;
            //    default:
            //        Process.GetCurrentProcess().Kill(); 
            //        break;
            //}
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                Utils.StopUdpThread = true;
                Thread th = new Thread(() => Utils.StartListenerUdp(666));
                th.Name = "ListenrUdpThread";
                th.Start();
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
            }
        }
    }

}

