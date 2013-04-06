#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Microsoft.Win32;
using TvDatabase;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.Dri;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Pbda;
using TvLibrary.Implementations.RadioWebStream;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;

namespace TvLibrary.Implementations
{
  /// <summary>
  /// This class continually monitors device events.
  /// </summary>
  public class DeviceDetector : IDisposable
  {
    // Used for detecting and communicating with UPnP devices.
    private CPData _upnpControlPointData = null;
    private UPnPNetworkTracker _upnpAgent = null;
    private UPnPControlPoint _upnpControlPoint = null;

    // Are we actively detecting devices?
    private volatile bool _detecting = false;
    // The thread responsible for actually detecting devices.
    private Thread _detectThread = null;
    // The listener that we notify when device events occur.
    private IDeviceEventListener _deviceEventListener = null;

    // Network providers
    private IBaseFilter _atscNp = null;
    private IBaseFilter _dvbcNp = null;
    private IBaseFilter _dvbsNp = null;
    private IBaseFilter _dvbtNp = null;
    private IBaseFilter _mpNp = null;

    private IFilterGraph2 _graphBuilder = null;
    private DsROTEntry _rotEntry = null;

    /// <summary>
    /// Constructor.
    /// Start a thread that periodically checks device states.
    /// </summary>
    /// <param name="listener">A listener that wishes to be notified about device events.</param>
    public DeviceDetector(IDeviceEventListener listener)
    {
      _deviceEventListener = listener;

      // Logic here to delay starting to detect devices.
      // Ideally this should also apply after resuming from standby.
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("delayCardDetect", "0");
      int delayDetect = Convert.ToInt32(setting.Value);
      if (delayDetect >= 1)
      {
        Log.Log.WriteFile("Delaying device detection for {0} second(s)", delayDetect);
        Thread.Sleep(delayDetect * 1000);
      }

      _detecting = true;

      // Start detecting UPnP devices.
      // IMPORTANT: you should start the control point before the network tracker.
      _upnpControlPointData = new CPData();
      _upnpAgent = new UPnPNetworkTracker(_upnpControlPointData);
      _upnpAgent.RootDeviceAdded += UpnpRootDeviceAdded;
      _upnpAgent.RootDeviceRemoved += UpnpRootDeviceRemoved;
      _upnpControlPoint = new UPnPControlPoint(_upnpAgent);
      _upnpControlPoint.Start();
      _upnpAgent.Start();

      // Start detecting BDA/WDM devices.
      _detectThread = new Thread(new ThreadStart(DetectDevices));
      _detectThread.Priority = ThreadPriority.Lowest;
      _detectThread.IsBackground = true;
      _detectThread.Start();
    }

    #region IDisposable member

    /// <summary>
    /// Clean up, dispose, release.
    /// </summary>
    public void Dispose()
    {
      _detecting = false;
      _upnpAgent.Close();
      _upnpControlPoint.Close();

      if (_detectThread != null && _detectThread.IsAlive)
      {
        if (!_detectThread.Join(30000))
        {
          _detectThread.Abort();
        }
        _detectThread = null;
      }

      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      if (_graphBuilder != null)
      {
        FilterGraphTools.RemoveAllFilters(_graphBuilder);
        Release.ComObject("device detection graph builder", _graphBuilder);
        _graphBuilder = null;
      }
      if (_atscNp != null)
      {
        Release.ComObject("device detection ATSC network provider", _atscNp);
        _atscNp = null;
      }
      if (_dvbcNp != null)
      {
        Release.ComObject("device detection DVB-C network provider", _dvbcNp);
        _dvbcNp = null;
      }
      if (_dvbsNp != null)
      {
        Release.ComObject("device detection DVB-S network provider", _dvbsNp);
        _dvbsNp = null;
      }
      if (_dvbtNp != null)
      {
        Release.ComObject("device detection DVB-T network provider", _dvbtNp);
        _dvbtNp = null;
      }
      if (_mpNp != null)
      {
        Release.ComObject("device detection MediaPortal network provider", _mpNp);
        _mpNp = null;
      }
    }

    #endregion

    private static bool ConnectFilter(IFilterGraph2 graphBuilder, IBaseFilter networkFilter, IBaseFilter tunerFilter)
    {
      IPin pinOut = DsFindPin.ByDirection(networkFilter, PinDirection.Output, 0);
      IPin pinIn = DsFindPin.ByDirection(tunerFilter, PinDirection.Input, 0);
      int hr = graphBuilder.Connect(pinOut, pinIn);
      return (hr == 0);
    }

    /// <summary>
    /// Check whether any BDA or WDM devices have been connected to or
    /// disconnected from the system.
    /// </summary>
    private void DetectDevices()
    {
      Log.Log.WriteFile("Detecting BDA/WDM devices...");

      try
      {
        InitBdaDetectionGraph();
      }
      catch (Exception ex)
      {
        Log.Log.Error("Failed to initialise the BDA device detection graph!\r\n{0}", ex);
        return;
      }

      HashSet<string> previouslyKnownDevices = new HashSet<string>();
      HashSet<string> knownDevices = new HashSet<string>();

      // Always one RWS "tuner".
      RadioWebStreamCard rws = new RadioWebStreamCard();
      _deviceEventListener.OnDeviceAdded(rws);

      while (_detecting)
      {
        previouslyKnownDevices = knownDevices;
        knownDevices = new HashSet<string>();
        knownDevices.Add(rws.DevicePath);

        // Detect TechniSat SkyStar/AirStar/CableStar 2 & IP streaming devices.
        DetectSupportedLegacyAmFilterDevices(ref previouslyKnownDevices, ref knownDevices);

        // Detect capture devices. Currently only the Hauppauge HD PVR & Colossus.
        DetectSupportedAmKsCrossbarDevices(ref previouslyKnownDevices, ref knownDevices);

        // Detect analog tuners.
        DetectSupportedAmKsTvTunerDevices(ref previouslyKnownDevices, ref knownDevices);

        // Detect digital BDA tuners.
        DetectSupportedBdaSourceDevices(ref previouslyKnownDevices, ref knownDevices);

        // Remove the devices that are no longer connected.
        foreach (string previouslyKnownDevice in previouslyKnownDevices)
        {
          if (!knownDevices.Contains(previouslyKnownDevice))
          {
            Log.Log.WriteFile("Device {0} removed", previouslyKnownDevice);
            _deviceEventListener.OnDeviceRemoved(previouslyKnownDevice);
          }
        }

        // Check for new devices every 60 seconds.
        Thread.Sleep(60000);
      }
    }

    private void InitBdaDetectionGraph()
    {
      Log.Log.Debug("Initialise BDA device detection graph");
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graphBuilder);

      Guid mpNetworkProviderClsId = new Guid("{D7D42E5C-EB36-4aad-933B-B4C419429C98}");
      if (FilterGraphTools.IsThisComObjectInstalled(mpNetworkProviderClsId))
      {
        _mpNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, mpNetworkProviderClsId, "MediaPortal Network Provider");
        return;
      }

      ITuningSpace tuningSpace = null;
      ILocator locator = null;

      // ATSC
      _atscNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(ATSCNetworkProvider).GUID, "ATSC Network Provider");
      tuningSpace = (ITuningSpace)new ATSCTuningSpace();
      tuningSpace.put_UniqueName("ATSC TuningSpace");
      tuningSpace.put_FriendlyName("ATSC TuningSpace");
      ((IATSCTuningSpace)tuningSpace).put_MaxChannel(10000);
      ((IATSCTuningSpace)tuningSpace).put_MaxMinorChannel(10000);
      ((IATSCTuningSpace)tuningSpace).put_MinChannel(0);
      ((IATSCTuningSpace)tuningSpace).put_MinMinorChannel(0);
      ((IATSCTuningSpace)tuningSpace).put_MinPhysicalChannel(0);
      ((IATSCTuningSpace)tuningSpace).put_InputType(TunerInputType.Antenna);
      locator = (IATSCLocator)new ATSCLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);
      locator.put_CarrierFrequency(-1);
      ((IATSCLocator)locator).put_PhysicalChannel(-1);
      ((IATSCLocator)locator).put_TSID(-1);
      tuningSpace.put_DefaultLocator(locator);
      ((ITuner)_atscNp).put_TuningSpace(tuningSpace);

      // DVB-C
      _dvbcNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(DVBCNetworkProvider).GUID, "DVB-C Network Provider");
      tuningSpace = (ITuningSpace)new DVBTuningSpace();
      tuningSpace.put_UniqueName("DVB-C TuningSpace");
      tuningSpace.put_FriendlyName("DVB-C TuningSpace");
      tuningSpace.put__NetworkType(typeof(DVBCNetworkProvider).GUID);
      ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Cable);
      locator = (ILocator)new DVBCLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);
      tuningSpace.put_DefaultLocator(locator);
      ((ITuner)_dvbcNp).put_TuningSpace(tuningSpace);

      // DVB-S
      _dvbsNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(DVBSNetworkProvider).GUID, "DVB-S Network Provider");
      tuningSpace = (ITuningSpace)new DVBSTuningSpace();
      tuningSpace.put_UniqueName("DVB-S TuningSpace");
      tuningSpace.put_FriendlyName("DVB-S TuningSpace");
      tuningSpace.put__NetworkType(typeof(DVBSNetworkProvider).GUID);
      ((IDVBSTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Satellite);
      locator = (ILocator)new DVBTLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);
      tuningSpace.put_DefaultLocator(locator);
      ((ITuner)_dvbsNp).put_TuningSpace(tuningSpace);

      // DVB-T
      _dvbtNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(DVBTNetworkProvider).GUID, "DVB-T Network Provider");
      tuningSpace = (ITuningSpace)new DVBTuningSpace();
      tuningSpace.put_UniqueName("DVB-T TuningSpace");
      tuningSpace.put_FriendlyName("DVB-T TuningSpace");
      tuningSpace.put__NetworkType(typeof(DVBTNetworkProvider).GUID);
      ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Terrestrial);
      locator = (ILocator)new DVBTLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);
      tuningSpace.put_DefaultLocator(locator);
      ((ITuner)_dvbtNp).put_TuningSpace(tuningSpace);
    }

    private void DetectSupportedLegacyAmFilterDevices(ref HashSet<string> previouslyKnownDevices, ref HashSet<string> knownDevices)
    {
      Log.Log.Debug("Detect legacy AM filter devices");
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("iptvCardCount", "1");
      int iptvTunerCount = Convert.ToInt32(setting.Value);

      DsDevice[] connectedDevices = DsDevice.GetDevicesOfCat(FilterCategory.LegacyAmFilterCategory);
      foreach (DsDevice connectedDevice in connectedDevices)
      {
        string name = connectedDevice.Name;
        string devicePath = connectedDevice.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          continue;
        }

        if (name.Equals("B2C2 MPEG-2 Source"))
        {
          knownDevices.Add(devicePath);
          if (!previouslyKnownDevices.Contains(devicePath))
          {
            Log.Log.WriteFile("Detected new TechniSat *Star 2 tuner root device");
            TvCardDvbSS2 tuner = new TvCardDvbSS2(connectedDevice);
            _deviceEventListener.OnDeviceAdded(tuner);
          }
        }
        else if (name.Equals("Elecard NWSource-Plus"))
        {
          for (int i = 1; i <= iptvTunerCount; i++)
          {
            TvCardDVBIP iptvTuner = new TvCardDVBIPElecard(connectedDevice, i);
            knownDevices.Add(iptvTuner.DevicePath);
            if (!previouslyKnownDevices.Contains(iptvTuner.DevicePath))
            {
              Log.Log.WriteFile("Detected new Elecard IPTV tuner {0} {1}", iptvTuner.Name, iptvTuner.DevicePath);
              _deviceEventListener.OnDeviceAdded(iptvTuner);
            }
            else
            {
              iptvTuner.Dispose();
            }
          }
        }
        else if (name.Equals("MediaPortal IPTV Source Filter"))
        {
          for (int i = 1; i <= iptvTunerCount; i++)
          {
            TvCardDVBIP iptvTuner = new TvCardDVBIPBuiltIn(connectedDevice, i);
            knownDevices.Add(iptvTuner.DevicePath);
            if (!previouslyKnownDevices.Contains(iptvTuner.DevicePath))
            {
              Log.Log.WriteFile("Detected new MediaPortal IPTV tuner {0} {1}", iptvTuner.Name, iptvTuner.DevicePath);
              _deviceEventListener.OnDeviceAdded(iptvTuner);
            }
            else
            {
              iptvTuner.Dispose();
            }
          }
        }
      }
    }

    private void DetectSupportedAmKsCrossbarDevices(ref HashSet<string> previouslyKnownDevices, ref HashSet<string> knownDevices)
    {
      Log.Log.WriteFile("Detect AM KS crossbar devices");
      DsDevice[] connectedDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
      foreach (DsDevice connectedDevice in connectedDevices)
      {
        string name = connectedDevice.Name;
        string devicePath = connectedDevice.DevicePath;
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(devicePath) &&
          (name.Equals("Hauppauge HD PVR Crossbar") || name.Contains("Hauppauge Colossus Crossbar"))
        )
        {
          knownDevices.Add(devicePath);
          if (!previouslyKnownDevices.Contains(devicePath))
          {
            Log.Log.WriteFile("Detected new Hauppauge capture device {0} {1)", name, devicePath);
            TvCardHDPVR captureDevice = new TvCardHDPVR(connectedDevice);
            _deviceEventListener.OnDeviceAdded(captureDevice);
          }
        }
      }
    }

    private void DetectSupportedAmKsTvTunerDevices(ref HashSet<string> previouslyKnownDevices, ref HashSet<string> knownDevices)
    {
      Log.Log.Debug("Detect AM KS TV tuner devices");
      DsDevice[] connectedDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSTVTuner);
      foreach (DsDevice connectedDevice in connectedDevices)
      {
        string name = connectedDevice.Name;
        string devicePath = connectedDevice.DevicePath;
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(devicePath))
        {
          knownDevices.Add(devicePath);
          if (!previouslyKnownDevices.Contains(devicePath))
          {
            Log.Log.WriteFile("Detected new analog tuner device {0} {1}", name, devicePath);
            TvCardAnalog analogTuner = new TvCardAnalog(connectedDevice);
            _deviceEventListener.OnDeviceAdded(analogTuner);
          }
        }
      }
    }

    private void DetectSupportedBdaSourceDevices(ref HashSet<string> previouslyKnownDevices, ref HashSet<string> knownDevices)
    {
      Log.Log.WriteFile("Detect BDA source devices");

      // MS generic, MCE 2005 roll-up 2 or better
      bool isMsGenericNpAvailable = FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID);

      DsDevice[] connectedDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      foreach (DsDevice connectedDevice in connectedDevices)
      {
        string name = connectedDevice.Name;
        string devicePath = connectedDevice.DevicePath;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(devicePath))
        {
          continue;
        }
        if (previouslyKnownDevices.Contains(devicePath))
        {
          knownDevices.Add(devicePath);
          continue;
        }

        // North American CableCARD tuners [PBDA].
        if (name.StartsWith("HDHomeRun Prime") || name.StartsWith("Ceton InfiniTV"))
        {
          Log.Log.WriteFile("Detected new PBDA CableCARD tuner device {0} {1}", name, devicePath);
          TunerPbdaCableCard cableCardTuner = new TunerPbdaCableCard(connectedDevice);
          knownDevices.Add(devicePath);
          _deviceEventListener.OnDeviceAdded(cableCardTuner);
          continue;
        }

        IBaseFilter tmpDeviceFilter;
        try
        {
          _graphBuilder.AddSourceFilterForMoniker(connectedDevice.Mon, null, name, out tmpDeviceFilter);
        }
        catch (Exception ex)
        {
          Log.Log.Error("Failed to add filter to detect device type for {0}!\r\n{1}", name, ex);
          continue;
        }

        try
        {
          // Silicondust regular (non-CableCARD) HDHomeRun. Workaround for tuner type
          // detection issue. The MS generic provider would always detect DVB-T.
          bool isCablePreferred = false;
          if (name.StartsWith("Silicondust HDHomeRun Tuner"))
          {
            isCablePreferred = GetHdHomeRunSourceType(name).Equals("Digital Cable");
          }

          Log.Log.WriteFile("Detected new digital BDA tuner device {0} {1}", name, devicePath);

          // Try the MediaPortal network provider first.
          ITVCard deviceToAdd = null;
          if (_mpNp != null)
          {
            Log.Log.WriteFile("  check type with MP NP");
            IDvbNetworkProvider interfaceNetworkProvider = (IDvbNetworkProvider)_mpNp;
            string hash = GetHash(devicePath);
            interfaceNetworkProvider.ConfigureLogging(GetFileName(devicePath), hash, LogLevelOption.Debug);
            if (ConnectFilter(_graphBuilder, _mpNp, tmpDeviceFilter))
            {
              TuningType tuningTypes;
              interfaceNetworkProvider.GetAvailableTuningTypes(out tuningTypes);
              Log.Log.WriteFile("  tuning types = {0}, hash = {1}" + tuningTypes, hash);
              if (tuningTypes.HasFlag(TuningType.DvbT) && !isCablePreferred)
              {
                deviceToAdd = new TvCardDVBT(connectedDevice);
              }
              else if (tuningTypes.HasFlag(TuningType.DvbS) && !isCablePreferred)
              {
                deviceToAdd = new TvCardDVBS(connectedDevice);
              }
              else if (tuningTypes.HasFlag(TuningType.DvbC))
              {
                deviceToAdd = new TvCardDVBC(connectedDevice);
              }
              else if (tuningTypes.HasFlag(TuningType.Atsc))
              {
                deviceToAdd = new TvCardATSC(connectedDevice);
              }
              else
              {
                Log.Log.WriteFile("  connected to MP NP but type not recognised");
              }
            }
            else
            {
              Log.Log.WriteFile("  failed to connect to MP NP");
            }
          }
          // Try the Microsoft network provider next if the MP NP
          // failed and the MS generic NP is available.
          if (deviceToAdd == null && isMsGenericNpAvailable)
          {
            // Note: the MS NP must be added/removed to/from the graph for each
            // device that is checked. If you don't do this, the networkTypes
            // list gets longer and longer and longer.
            Log.Log.WriteFile("  check type with MS NP");
            IBaseFilter genericNp = null;
            try
            {
              genericNp = FilterGraphTools.AddFilterFromClsid(_graphBuilder, typeof(NetworkProvider).GUID, "Microsoft Network Provider");
            }
            catch
            {
              genericNp = null;
            }
            if (genericNp == null)
            {
              Log.Log.WriteFile(" failed to add MS NP to graph");
            }
            else
            {
              if (ConnectFilter(_graphBuilder, genericNp, tmpDeviceFilter))
              {
                int networkTypesMax = 5;
                int networkTypeCount;
                Guid[] networkTypes = new Guid[networkTypesMax];
                int hr = (genericNp as ITunerCap).get_SupportedNetworkTypes(networkTypesMax, out networkTypeCount, networkTypes);
                Log.Log.WriteFile("  network type count = {0}", networkTypeCount);
                for (int n = 0; n < networkTypeCount; n++)
                {
                  Log.Log.WriteFile("  network type {0} = {1}", n, networkTypes[n]);
                  if (networkTypes[n] == typeof(DVBTNetworkProvider).GUID && !isCablePreferred)
                  {
                    deviceToAdd = new TvCardDVBT(connectedDevice);
                  }
                  else if (networkTypes[n] == typeof(DVBSNetworkProvider).GUID && !isCablePreferred)
                  {
                    deviceToAdd = new TvCardDVBS(connectedDevice);
                  }
                  else if (networkTypes[n] == typeof(DVBCNetworkProvider).GUID)
                  {
                    deviceToAdd = new TvCardDVBC(connectedDevice);
                  }
                  else if (networkTypes[n] == typeof(ATSCNetworkProvider).GUID)
                  {
                    deviceToAdd = new TvCardATSC(connectedDevice);
                  }
                  if (deviceToAdd != null)
                  {
                    break;
                  }
                  else if (n == (networkTypeCount - 1))
                  {
                    Log.Log.WriteFile(" connected to MS NP but type not recognised");
                  }
                }
              }
              else
              {
                Log.Log.WriteFile("  failed to connect to MS NP");
              }

              Release.ComObject("device detection generic network provider", genericNp);
              genericNp = null;
            }
          }
          // Last shot is the old style Microsoft network providers.
          if (deviceToAdd == null)
          {
            Log.Log.WriteFile("  check type with specific NPs");
            if (ConnectFilter(_graphBuilder, _dvbtNp, tmpDeviceFilter))
            {
              deviceToAdd = new TvCardDVBT(connectedDevice);
            }
            else if (ConnectFilter(_graphBuilder, _dvbcNp, tmpDeviceFilter))
            {
              deviceToAdd = new TvCardDVBC(connectedDevice);
            }
            else if (ConnectFilter(_graphBuilder, _dvbsNp, tmpDeviceFilter))
            {
              deviceToAdd = new TvCardDVBS(connectedDevice);
            }
            else if (ConnectFilter(_graphBuilder, _atscNp, tmpDeviceFilter))
            {
              deviceToAdd = new TvCardATSC(connectedDevice);
            }
            else
            {
              Log.Log.WriteFile("  failed to connect to specific NP");
            }
          }

          if (deviceToAdd != null)
          {
            Log.Log.WriteFile("  tuner type = {0}", deviceToAdd.CardType);
            knownDevices.Add(devicePath);
            _deviceEventListener.OnDeviceAdded(deviceToAdd);
          }
        }
        finally
        {
          _graphBuilder.RemoveFilter(tmpDeviceFilter);
          Release.ComObject("device detection device filter", tmpDeviceFilter);
        }
      }
    }

    #region MP network provider only (contact MisterD)

    /// <summary>
    /// Generates the file and pathname of the log file
    /// </summary>
    /// <param name="devicePath">Device Path of the card</param>
    /// <returns>Complete filename of the configuration file</returns>
    public static string GetFileName(string devicePath)
    {
      string hash = GetHash(devicePath);
      string pathName = PathManager.GetDataPath;
      string fileName = string.Format(@"{0}\Log\NetworkProvider-{1}.log", pathName, hash);
      Log.Log.WriteFile("NetworkProvider logfilename: " + fileName);
      Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      return fileName;
    }

    public static string GetHash(string value)
    {
      byte[] data = Encoding.ASCII.GetBytes(value);
      byte[] hashData = new SHA1Managed().ComputeHash(data);

      string hash = string.Empty;

      foreach (byte b in hashData)
      {
        hash += b.ToString("X2");
      }
      return hash;
    }

    #endregion

    #region UPnP delegates

    private void UpnpRootDeviceAdded(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor.State != RootDescriptorState.Ready)
      {
        return;
      }
      DeviceDescriptor deviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
      if (deviceDescriptor.FriendlyName.StartsWith("HDHomeRun Prime Tuner") ||
        deviceDescriptor.FriendlyName.StartsWith("Ceton InfiniTV")
      )
      {
        Log.Log.WriteFile("Detected new OCUR/DRI device {0}, sub-device count = {1}", deviceDescriptor.FriendlyName, deviceDescriptor.ChildDevices.Count);
        foreach (DeviceDescriptor d in deviceDescriptor.ChildDevices)
        {
          if (d.FriendlyName.Contains(" OCTA "))
          {
            Log.Log.WriteFile("  skipping Ceton tuning adaptor device, not supported");
          }
          else
          {
            Log.Log.WriteFile("  add {0} {1}", d.FriendlyName, d.DeviceUDN);
            _deviceEventListener.OnDeviceAdded(new TunerDri(d, _upnpControlPoint));
          }
        }
      }
    }

    private void UpnpRootDeviceRemoved(RootDescriptor rootDescriptor)
    {
      DeviceDescriptor deviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
      if (deviceDescriptor.FriendlyName.StartsWith("HDHomeRun Prime Tuner") ||
        deviceDescriptor.FriendlyName.StartsWith("Ceton InfiniTV")
      )
      {
        Log.Log.WriteFile("Device {0} removed, sub-device count = {1}", deviceDescriptor.FriendlyName, deviceDescriptor.ChildDevices.Count);
        foreach (DeviceDescriptor d in deviceDescriptor.ChildDevices)
        {
          if (!d.FriendlyName.Contains(" OCTA "))
          {
            Log.Log.WriteFile("  remove {0} {1}", d.FriendlyName, d.DeviceUDN);
            _deviceEventListener.OnDeviceRemoved(d.DeviceUDN);
          }
        }
      }
    }

    #endregion

    #region hardware specific functions

    private string GetHdHomeRunSourceType(string tunerName)
    {
      try
      {
        // The tuner settings (configured by "HDHomeRun Setup" - part of the
        // Silicondust HDHomeRun driver and software package) are stored in the
        // registry. Example:
        // tuner device name = "Silicondust HDHomeRun Tuner 1210551E-0"
        // registry key = "HKEY_LOCAL_MACHINE\SOFTWARE\Silicondust\HDHomeRun\Tuners\1210551E-0"
        // possible source type values =
        //    "Digital Cable" [DVB-C, clear QAM]
        //    "Digital Antenna" [DVB-T, ATSC]
        //    "CableCARD" [North American encrypted cable television]
        string serialNumber = tunerName.Replace("Silicondust HDHomeRun Tuner ", "");
        using (
          RegistryKey registryKey =
            Registry.LocalMachine.OpenSubKey(string.Format(@"SOFTWARE\Silicondust\HDHomeRun\Tuners\{0}", serialNumber)))
        {
          if (registryKey != null)
          {
            return registryKey.GetValue("Source").ToString();
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Error("Failed to check HDHomeRun preferred mode.\r\n{0}", ex);
      }
      return "Digital Antenna";
    }

    #endregion
  }
}