using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetFwTypeLib;
using System.Windows.Forms;
using NLog;

namespace Ariete.WinFormUI
{
    public class clsFirewall
    {
        private INetFwProfile fwProfile = null;
        // Log purpose
        public static Logger logger;

        public clsFirewall()
        {
            logger = LogManager.GetLogger("ArieteClsFirewall");
        }
        /// <summary>
        /// Apre tutte le porte sia TCP che UDP per un determinato Exe
        /// </summary>
        /// <param name="appName">Nome che comparira' nella regola del Firewall</param>
        /// <param name="exePath">Percorso alleseguibile da sbloccare sul Firewall</param>
        protected internal void OpenFirewallExe(string appName = "", string exePath = "")
        {
            INetFwAuthorizedApplications authApps = null;
            INetFwAuthorizedApplication authApp = null;
            INetFwOpenPorts openPorts = null;
            INetFwOpenPort openPort = null;
            
            if (appName == "")
            {
                appName = Application.ProductName;
            }
            if (exePath == "")
            {
                exePath = Application.ExecutablePath;
            }
            try
            {
                if (isAppFound(Application.ProductName) == false)
                {
                    SetProfile();
                    authApps = fwProfile.AuthorizedApplications;
                    authApp = GetInstance("INetAuthApp") as INetFwAuthorizedApplication;
                    authApp.Name = appName;
                    authApp.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;
                    authApp.IpVersion = NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY;
                    authApp.ProcessImageFileName = exePath;
                    authApps.Add(authApp);

                    logger.Info("Aggiunta regola generale per " + appName +": " + exePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (authApps != null) authApps = null;
                if (authApp != null) authApp = null;
                if (openPorts != null) openPorts = null;
                if (openPort != null) openPort = null;
            }

        }

        protected internal void OpenFirewallPort(int port, string protocol, string appName)
        {
            INetFwAuthorizedApplications authApps = null;
            INetFwAuthorizedApplication authApp = null;
            INetFwOpenPorts openPorts = null;
            INetFwOpenPort openPort = null;
            try
            {
                if (isPortFound(port) == false)
                {
                    SetProfile();
                    openPorts = fwProfile.GloballyOpenPorts;
                    openPort = GetInstance("INetOpenPort") as INetFwOpenPort;
                    openPort.Port = port;
                    if (protocol.ToLower() == "udp")
                    {
                        openPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP;
                    }
                    else if (protocol.ToLower() == "tcp")
                    {
                        openPort.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;

                    }

                    openPort.Name = appName + " " + protocol.ToUpper();
                    openPorts.Add(openPort);
                    logger.Info("Firewall: Aggiunta regola per " + appName + ": " + protocol.ToUpper());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (authApps != null) authApps = null;
                if (authApp != null) authApp = null;
                if (openPorts != null) openPorts = null;
                if (openPort != null) openPort = null;
            }
        }

        protected internal void CloseFirewall(int port, string protocol, string appName)
        {
            INetFwAuthorizedApplications apps = null;
            INetFwOpenPorts ports = null;
            try
            {

                if (isAppFound(Application.ProductName + " Server") == true)
                {
                    SetProfile();
                    apps = fwProfile.AuthorizedApplications;
                    apps.Remove(Application.ExecutablePath);
                }

                if (isPortFound(port) == true)
                {
                    SetProfile();
                    ports = fwProfile.GloballyOpenPorts;
                    if (protocol.ToLower() == "udp")
                    {
                        ports.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP);
                    }
                    else if (protocol.ToLower() == "tcp")
                    {
                        ports.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);

                    }
                    else ports.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (apps != null) apps = null;
                if (ports != null) ports = null;
            }
        }

        protected internal bool isAppFound(string appName)
        {
            bool boolResult = false;
            Type progID = null;
            INetFwMgr firewall = null;
            INetFwAuthorizedApplications apps = null;
            INetFwAuthorizedApplication app = null;
            try
            {
                progID = Type.GetTypeFromProgID("HNetCfg.FwMgr");
                firewall = Activator.CreateInstance(progID) as INetFwMgr;
                if (firewall.LocalPolicy.CurrentProfile.FirewallEnabled)
                {
                    apps = firewall.LocalPolicy.CurrentProfile.AuthorizedApplications;
                    IEnumerator appEnumerate = apps.GetEnumerator();
                    while ((appEnumerate.MoveNext()))
                    {
                        app = appEnumerate.Current as INetFwAuthorizedApplication;
                        if (app.Name == appName)
                        {
                            boolResult = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (progID != null) progID = null;
                if (firewall != null) firewall = null;
                if (apps != null) apps = null;
                if (app != null) app = null;
            }
            return boolResult;
        }

        protected internal bool isPortFound(int portNumber)
        {
            bool boolResult = false;
            INetFwOpenPorts ports = null;
            Type progID = null;
            INetFwMgr firewall = null;
            INetFwOpenPort currentPort = null;
            try
            {
                // TODO: aprire in pubblico e privato 
                // ref: http://stackoverflow.com/questions/15409790/adding-an-application-firewall-rule-to-both-private-and-public-networks-via-win7
                progID = Type.GetTypeFromProgID("HNetCfg.FwMgr");
                firewall = Activator.CreateInstance(progID) as INetFwMgr;
                ports = firewall.LocalPolicy.CurrentProfile.GloballyOpenPorts;
                IEnumerator portEnumerate = ports.GetEnumerator();
                while ((portEnumerate.MoveNext()))
                {
                    currentPort = portEnumerate.Current as INetFwOpenPort;
                    if (currentPort.Port == portNumber)
                    {
                        boolResult = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (ports != null) ports = null;
                if (progID != null) progID = null;
                if (firewall != null) firewall = null;
                if (currentPort != null) currentPort = null;
            }
            return boolResult;
        }

        protected internal void SetProfile()
        {
            INetFwMgr fwMgr = null;
            INetFwPolicy fwPolicy = null;
            try
            {
                fwMgr = GetInstance("INetFwMgr") as INetFwMgr;
                fwPolicy = fwMgr.LocalPolicy;
                fwProfile = fwPolicy.CurrentProfile;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (fwMgr != null) fwMgr = null;
                if (fwPolicy != null) fwPolicy = null;
            }
        }

        protected internal object GetInstance(string typeName)
        {
            Type tpResult = null;
            switch (typeName)
            {
                case "INetFwMgr":
                    tpResult = Type.GetTypeFromCLSID(new Guid("{304CE942-6E39-40D8-943A-B913C40C9CD4}"));
                    return Activator.CreateInstance(tpResult);
                case "INetAuthApp":
                    tpResult = Type.GetTypeFromCLSID(new Guid("{EC9846B3-2762-4A6B-A214-6ACB603462D2}"));
                    return Activator.CreateInstance(tpResult);
                case "INetOpenPort":
                    tpResult = Type.GetTypeFromCLSID(new Guid("{0CA545C6-37AD-4A6C-BF92-9F7610067EF5}"));
                    return Activator.CreateInstance(tpResult);
                default:
                    return null;
            }
        }

    }
}
