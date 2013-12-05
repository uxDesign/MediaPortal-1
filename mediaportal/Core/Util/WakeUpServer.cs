using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Utils;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using System.Net;

namespace MediaPortal.Util
{
  public class WakeUpServer
  {
    public void HandleWakeUpServer(string HostName)
    {
      String macAddress;
      byte[] hwAddress;

      WakeOnLanManager wakeOnLanManager = new WakeOnLanManager();

      IPAddress ipAddress = null;

      if (wakeOnLanManager.Ping(HostName, 100))
      {
        Log.Debug("WakeUpServer: The {0} server already started!", HostName);
        return;
      }

      // Check if we already have a valid IP address stored,
      // otherwise try to resolve the IP address
      if (!IPAddress.TryParse(HostName, out ipAddress))
      {
        // Get IP address of the TV server
        try
        {
          IPAddress[] ips;

          ips = Dns.GetHostAddresses(HostName);

          Log.Debug("WakeUpServer: WOL - GetHostAddresses({0}) returns:", HostName);

          foreach (IPAddress ip in ips)
          {
            Log.Debug("    {0}", ip);
          }

          // Use first valid IP address
          ipAddress = ips[0];
        }
        catch (Exception ex)
        {
          Log.Error("WakeUpServer: WOL - Failed GetHostAddress - {0}", ex.Message);
        }
      }

      // Check for valid IP address
      if (ipAddress != null)
      {
        // Update the MAC address if possible
        hwAddress = wakeOnLanManager.GetHardwareAddress(ipAddress);

        if (wakeOnLanManager.IsValidEthernetAddress(hwAddress))
        {
          Log.Debug("WakeUpServer: WOL - Valid auto MAC address: {0:x}:{1:x}:{2:x}:{3:x}:{4:x}:{5:x}"
                    , hwAddress[0], hwAddress[1], hwAddress[2], hwAddress[3], hwAddress[4], hwAddress[5]);

          // Store MAC address
          macAddress = BitConverter.ToString(hwAddress).Replace("-", ":");

          Log.Debug("WakeUpServer: WOL - Store MAC address: {0}", macAddress);

          using (
            MediaPortal.Profile.Settings xmlwriter =
              new MediaPortal.Profile.MPSettings())
          {
            xmlwriter.SetValue("macAddress", HostName, macAddress);
          }
        }
      }

      // Use stored MAC address
      using (Settings xmlreader = new MPSettings())
      {
        macAddress = xmlreader.GetValueAsString("macAddress", HostName, null);
      }

      Log.Debug("WakeUpServer: WOL - Use stored MAC address: {0}", macAddress);

      try
      {
        hwAddress = wakeOnLanManager.GetHwAddrBytes(macAddress);

        // Finally, start up the server
        Log.Info("WakeUpServer: WOL - Start the {0} server", HostName);

        if (wakeOnLanManager.WakeupSystem(hwAddress, HostName, 10))
        {
          Log.Info("WakeUpServer: WOL - The {0} server started successfully!", HostName);
        }
        else
        {
          Log.Error("WakeUpServer: WOL - Failed to start the {0} server", HostName);
        }
      }
      catch (Exception ex)
      {
        Log.Error("WakeUpServer: WOL - Failed to start the server - {0}", ex.Message);
      }
    }
  }
}
