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
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Implementations.Helper;
using TvDatabase;
using TvLibrary.Epg;
using UPnP.Infrastructure.CP.Description;
using System.Xml.XPath;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles CableCARD digital cable tuners.
  /// </summary>
  public class TvCardNaCable : TvCardATSC
  {
    #region variables

    private IBaseFilter _pbdaFilter = null;
    private IBDA_ConditionalAccess _bdaCa = null;
    private DeviceDescriptor _descriptor = null;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardNaCable"/> class.
    /// </summary>
    /// <param name="device">The device.</param>
    public TvCardNaCable(DsDevice device)
      : base(device)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardNaCable"/> class.
    /// </summary>
    /// <param name="descriptor">The device description. Essentially an XML document.</param>
    public TvCardNaCable(DeviceDescriptor descriptor)
      : base(null)
    {
      _descriptor = descriptor;
      _name = descriptor.FriendlyName;

      // unique device name is as good as a device path for a unique identifier
      _devicePath = descriptor.DeviceUDN;

      GetPreloadBitAndCardId();
      GetSupportsPauseGraph();

      String urlBase = String.Empty;
      XPathNavigator navigator = descriptor.RootDescriptor.DeviceDescription.CreateNavigator();
      if (navigator.MoveToChild("URLBase", "urn:schemas-upnp-org:device-1-0"))
      {
        urlBase = navigator.Value;
        Log.Log.Debug("debug: found URL base {0}", urlBase);
      }
    }

    #region graphbuilding

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
    {
      try
      {
        if (_graphState != GraphState.Idle)
        {
          Log.Log.Error("NA Cable: graph already built");
          throw new TvException("Graph already built");
        }
        Log.Log.WriteFile("NA Cable: build graph");
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);
        AddNetworkProviderFilter(typeof (ATSCNetworkProvider).GUID);
        AddTsWriterFilterToGraph();
        if (!useInternalNetworkProvider)
        {
          AddMpeg2DemuxerToGraph();
        }
        IBaseFilter lastFilter;
        AddAndConnectBDABoardFilters(_device, out lastFilter);
        _bdaCa = _filterTuner as IBDA_ConditionalAccess;
        if (_bdaCa == null)
        {
          Log.Log.Debug("NA Cable: tuner filter does not implement required interface");
          throw new TvExceptionGraphBuildingFailed("The tuner filter does not implement the required interface.");
        }
        AddPbdaFilter(ref lastFilter);
        CompleteGraph(ref lastFilter);
        bool connected = ConnectTsWriter(_filterTuner);
        Log.Log.Debug("debug: connect OOB pin result = {0}", connected);
        CheckCableCardInfo();
        string graphName = _device.Name + " - NA Cable Graph.grf";
        FilterGraphTools.SaveGraphFile(_graphBuilder, graphName);
        GetTunerSignalStatistics();
        _graphState = GraphState.Created;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw new TvExceptionGraphBuildingFailed("Graph building failed", ex);
      }
    }

    /// <summary>
    /// Determine whether the tuner filter needs to connect to a capture filter,
    /// or whether it can be directly connected to an inf tee.
    /// </summary>
    /// <returns><c>true</c> if the tuner filter must be connected to a capture filter, otherwise <c>false</c></returns>
    protected override bool UseCaptureFilter()
    {
      // Really critical! Do *not* attempt to use a capture filter as it can screw up the operation of the graph.
      return false;
    }

    private void AddPbdaFilter(ref IBaseFilter lastFilter)
    {
      Log.Log.Debug("NA Cable: add PBDA filter");

      // PBDA PTFilter
      // {89C2E132-C29B-11DB-96FA-005056C00008}
      Guid pbdaPtFilter = new Guid(0x89c2e132, 0xc29b, 0x11db, 0x96, 0xfa, 0x00, 0x50, 0x56, 0xc0, 0x00, 0x08);

      _pbdaFilter = FilterGraphTools.AddFilterFromClsid(_graphBuilder, pbdaPtFilter, "PBDA PTFilter");
      if (_pbdaFilter == null)
      {
        Log.Log.Debug("NA Cable: failed to add PBDA filter, filter is null - you must use Windows 7!");
        throw new TvExceptionGraphBuildingFailed("Failed to add PBDA filter to graph.");
      }

      int hr = _capBuilder.RenderStream(null, null, lastFilter, null, _pbdaFilter);
      if (hr != 0)
      {
        Log.Log.Debug("NA Cable: failed to connect PBDA filter into the graph, hr = 0x{0:x}", hr);
        throw new TvExceptionGraphBuildingFailed("Failed to connect PBDA filter into graph.");
      }
      lastFilter = _pbdaFilter;
    }

    private void CheckCableCardInfo()
    {
      if (_bdaCa == null)
      {
        return;
      }
      RunGraph(-1);
      Log.Log.Debug("NA Cable: check CableCARD info");
      int hr;

      // Smart card information.
      String cardName;
      String cardManufacturer;
      bool isDaylightSavings;
      byte ratingRegion;
      int timeOffset;
      String lang;
      EALocationCodeType locationCode;
      try
      {
        hr = _bdaCa.get_SmartCardInfo(out cardName, out cardManufacturer, out isDaylightSavings, out ratingRegion, out timeOffset, out lang, out locationCode);
        if (hr == 0)
        {
          Log.Log.Debug("NA Cable: got smart card info");
          Log.Log.Debug("  card name         = {0}", cardName);
          Log.Log.Debug("  card manufacturer = {0}", cardManufacturer);
          Log.Log.Debug("  is DS time        = {0}", isDaylightSavings);
          Log.Log.Debug("  rating region     = {0}", ratingRegion);
          Log.Log.Debug("  time offset       = {0}", timeOffset);
          Log.Log.Debug("  language          = {0}", lang);
          Log.Log.Debug("  location code...");
          Log.Log.Debug("    scheme               = {0}", locationCode.LocationCodeScheme);
          Log.Log.Debug("    state code           = {0}", locationCode.StateCode);
          Log.Log.Debug("    country sub division = {0}", locationCode.CountySubdivision);
          Log.Log.Debug("    country code         = {0}", locationCode.CountyCode);
        }
        else
        {
          Log.Log.Debug("NA Cable: failed to read smart card info, hr = 0x{0:x}", hr);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("NA Cable: caught exception when attempting to read smart card info\r\n{0}", ex.ToString());
      }

      // Smart card applications.
      SmartCardStatusType status;
      SmartCardAssociationType association;
      string error;
      bool isOutOfBandTunerLocked;
      try
      {
        hr = _bdaCa.get_SmartCardStatus(out status, out association, out error, out isOutOfBandTunerLocked);
        if (hr == 0)
        {
          Log.Log.Debug("NA Cable: got smart card status");
          Log.Log.Debug("  status      = {0}", status);
          Log.Log.Debug("  association = {0}", association);
          Log.Log.Debug("  OOB locked  = {0}", isOutOfBandTunerLocked);
          Log.Log.Debug("  error       = {0}", error);
        }
        else
        {
          Log.Log.Debug("NA Cable: failed to read smart card status, hr = 0x{0:x}", hr);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("NA Cable: caught exception when attempting to read smart card status\r\n{0}", ex.ToString());
      }

      // Smart card applications.
      int scAppCount = 32;
      int maxAppCount = 32;
      SmartCardApplication[] applicationDetails = new SmartCardApplication[33];
      try
      {
        hr = _bdaCa.get_SmartCardApplications(ref scAppCount, maxAppCount, applicationDetails);
        if (hr == 0)
        {
          Log.Log.Debug("NA Cable: got smart card application info, application count = {0}", scAppCount);
          for (int i = 0; i < scAppCount; i++)
          {
            Log.Log.Debug("  application #{0}", i + 1);
            Log.Log.Debug("    type    = {0}", applicationDetails[i].ApplicationType);
            Log.Log.Debug("    version = {0}", applicationDetails[i].ApplicationVersion);
            Log.Log.Debug("    name    = {0}", applicationDetails[i].pbstrApplicationName);
            Log.Log.Debug("    URL     = {0}", applicationDetails[i].pbstrApplicationURL);
          }
        }
        else
        {
          Log.Log.Debug("NA Cable: failed to read smart card application info, hr = 0x{0:x}", hr);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("NA Cable: caught exception when attempting to read smart card application info\r\n{0}", ex.ToString());
      }
    }

    #endregion

    #region tuning & recording

    protected override ITvSubChannel SubmitTuneRequest(int subChannelId, IChannel channel, ITuneRequest tuneRequest,
                                              bool performTune)
    {
      Log.Log.Info("NA Cable: tune channel \"{0}\", subChannel {1} ", channel.Name, subChannelId);
      RunGraph(-1);
      bool newSubChannel = false;
      if (_mapSubChannels.ContainsKey(subChannelId) == false)
      {
        Log.Log.Info("  new subchannel");
        newSubChannel = true;
        subChannelId = GetNewSubChannel(channel);
      }
      else
      {
        Log.Log.Info("  existing subchannel {0}", subChannelId);
      }

      _mapSubChannels[subChannelId].CurrentChannel = channel;
      try
      {         
        _mapSubChannels[subChannelId].OnBeforeTune();        
        if (_interfaceEpgGrabber != null)
        {
          _interfaceEpgGrabber.Reset();
        }
        if (performTune)
        {
          Log.Log.WriteFile("NA Cable: tuning...");
          int hr = _bdaCa.TuneByChannel((ushort)(channel as ATSCChannel).PhysicalChannel);
          if (hr != 0)
          {
            Log.Log.WriteFile("  tuning failed, hr = 0x{0:x}", hr);
            throw new TvException("Unable to tune to channel");
          }
        }
        else
        {
          Log.Log.WriteFile("NA Cable: already tuned");
        }
        _lastSignalUpdate = DateTime.MinValue;
        _mapSubChannels[subChannelId].OnAfterTune();
      }
      catch (Exception)
      {        
        if (newSubChannel)
        {
          Log.Log.WriteFile("NA Cable: tuning failed, removing subchannel");
          if (_mapSubChannels.ContainsKey(subChannelId))
          {
            _mapSubChannels.Remove(subChannelId);
          }
        }
        throw;
      }

      return _mapSubChannels[subChannelId];
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="channel"></param>
    /// <returns>
    /// true if card can tune to the channel otherwise false
    /// </returns>
    public override bool CanTune(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        return false;
      }
      if (atscChannel.ModulationType != ModulationType.ModNotSet)
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Scans the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public override ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("NA Cable: scan");
      try
      {
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
        }
        // It is enough to check that the OOB tuner is locked.
        SmartCardStatusType status;
        SmartCardAssociationType association;
        string error;
        bool isOutOfBandTunerLocked = false;
        int hr = _bdaCa.get_SmartCardStatus(out status, out association, out error, out isOutOfBandTunerLocked);
        if (hr == 0)
        {
          Log.Log.Debug("NA Cable: got smart card status");
          Log.Log.Debug("  status      = {0}", status);
          Log.Log.Debug("  association = {0}", association);
          Log.Log.Debug("  OOB locked  = {0}", isOutOfBandTunerLocked);
          Log.Log.Debug("  error       = {0}", error);
        }
        else
        {
          Log.Log.Debug("NA Cable: failed to read smart card status, hr = 0x{0:x}", hr);
        }

        if (hr != 0 || !isOutOfBandTunerLocked)
        {
          throw new TvExceptionNoSignal();
        }

        // Create a subchannel.
        if (_mapSubChannels.ContainsKey(subChannelId) == false)
        {
          Log.Log.Info("NA Cable: creating new subchannel");
          subChannelId = GetNewSubChannel(channel);
        }
        else
        {
          Log.Log.Info("NA Cable: using existing subchannel");
        }
        RunGraph(-1);
        return _mapSubChannels[subChannelId];
      }
      catch (Exception ex)
      {
        Log.Log.Debug("NA Cable: caught exception when attempting to scan\r\n{0}", ex.ToString());
      }
      return null;
    }

    protected override bool BeforeTune(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        Log.Log.WriteFile("NA Cable: channel is not a ATSC channel!!! {0}", channel.GetType().ToString());
        return false;
      }
      if (_graphState == GraphState.Idle)
      {
        BuildGraph();
      }
      return true;
    }

    #endregion

    /// <summary>
    /// destroys the graph and cleans up any resources
    /// </summary>
    protected void Decompose()
    {
      base.Decompose();
      if (_pbdaFilter != null)
      {
        Release.ComObject(_pbdaFilter);
        _pbdaFilter = null;
      }
      _bdaCa = null;
    }
  }
}