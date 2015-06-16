using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetFwTypeLib;
using Microsoft.Win32;
using Ariete.Core;
using NLog;

namespace Ariete.WinFormUI
{
    public class Firewall
    {
        // Log purpose
        public static Logger logger;

        protected INetFwProfile fwProfile;
        public Firewall()
        {
            logger = LogManager.GetLogger("ArieteFirewall");
        }
        public void FirewallOpenPort(int port, string protocol, string appName)
        {
            ///////////// Firewall Authorize Application ////////////
            String imageFilename = Utils.EmuleAdunanzAExeGetPath();
            setProfile();
            INetFwAuthorizedApplications apps = fwProfile.AuthorizedApplications;
            INetFwAuthorizedApplication app = (INetFwAuthorizedApplication)GetInstance("INetAuthApp");
            app.Name = appName;
            app.ProcessImageFileName = imageFilename;
            apps.Add(app);

            //////////////// Open Needed Ports /////////////////
            INetFwOpenPorts openports = fwProfile.GloballyOpenPorts;
            INetFwOpenPort openport = (INetFwOpenPort)GetInstance("INetOpenPort");
            openport.Port = port;
            try
            {
                if (protocol.ToLower() == "udp")
                {
                    openport.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP;
                }
                else if (protocol.ToLower() == "tcp")
                {
                    openport.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                }
                else openport.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;

                openport.Name = appName + " " + protocol;
                openports.Add(openport);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            finally
            {
                if (apps != null) apps = null;
                if (openport != null) openport = null;
                logger.Info("Firewall: aggiunta regola per {0} su porta {1}: {2}", appName, port, protocol);
            }

        } // openFirewall

        public void FirewallClosePort(int port, string proto)
        {
            INetFwOpenPorts ports = fwProfile.GloballyOpenPorts;
            INetFwAuthorizedApplications apps = fwProfile.AuthorizedApplications;

            try
            {
                String imageFilename = Utils.EmuleAdunanzAExeGetPath();
                setProfile();
                apps.Remove(imageFilename);

                if (proto.ToLower() == "udp")
                {
                    ports.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP);
                }
                else if (proto.ToLower() == "tcp")
                {
                    ports.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
                }
                else ports.Remove(port, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            finally
            {
                logger.Info("Firewall: eliminata regola per porta {0} {1}", port, proto);
                if (apps != null) apps = null;
                if (ports != null) ports = null;
            }
        }

        protected void setProfile()
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
                logger.Error(ex.Message);
            }
            finally
            {
                logger.Info("Firewall: aggiunto profilo ");
                if (fwMgr != null) fwMgr = null;
                if (fwPolicy != null) fwPolicy = null;
            }
        }

        protected Object GetInstance(String typeName)
        {
            if (typeName == "INetFwMgr")
            {
                Type type = Type.GetTypeFromCLSID(
                new Guid("{304CE942-6E39-40D8-943A-B913C40C9CD4}"));
                return Activator.CreateInstance(type);
            }
            else if (typeName == "INetAuthApp")
            {
                Type type = Type.GetTypeFromCLSID(
                new Guid("{EC9846B3-2762-4A6B-A214-6ACB603462D2}"));
                return Activator.CreateInstance(type);
            }
            else if (typeName == "INetOpenPort")
            {
                Type type = Type.GetTypeFromCLSID(
                new Guid("{0CA545C6-37AD-4A6C-BF92-9F7610067EF5}"));
                return Activator.CreateInstance(type);
            }
            else return null;
        }
    }

}
