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
using System.Linq;
using System.Xml.XPath;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Helper;
using TvLibrary.Interfaces;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri
{
  public class TunerDri : TvCardATSC
  {
    #region constants

    private static readonly Guid SourceFilterClsid = new Guid(0xd3dd4c59, 0xd3a7, 0x4b82, 0x97, 0x27, 0x7b, 0x92, 0x03, 0xeb, 0x67, 0xc0);
    private static readonly string DefaultUrl = "rtp://0.0.0.0:1234";

    #endregion

    #region variables

    private DeviceDescriptor _descriptor = null;
    private UPnPControlPoint _controlPoint = null;
    private DeviceConnection _deviceConnection = null;
    private StateVariableChangedDlgt _stateVariableDelegate = null;

    // services
    private TunerService _tunerService = null;
    private FdcService _fdcService = null;      // forward data channel
    private AuxService _auxService = null;
    private EncoderService _encoderService = null;
    private CasService _casService = null;
    private MuxService _muxService = null;
    private SecurityService _securityService = null;
    private DiagService _diagService = null;
    private AvTransportService _avTransportService = null;
    private ConnectionManagerService _connectionManagerService = null;

    private IBaseFilter _filterStreamSource = null;
    private IPin _firstFilterInputPin = null;

    private int _connectionId = -1;
    private int _avTransportId = -1;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDri"/> class.
    /// </summary>
    /// <param name="descriptor">The device description. Essentially an XML document describing the device interface.</param>
    /// <param name="controlPoint">The control point to use to connect to the device.</param>
    public TunerDri(DeviceDescriptor descriptor, UPnPControlPoint controlPoint)
      : base(null)
    {
      _descriptor = descriptor;
      _controlPoint = controlPoint;
      _name = descriptor.FriendlyName;
      _devicePath = descriptor.DeviceUDN;   // unique device name is as good as a device path for a unique identifier

      GetPreloadBitAndCardId();
      GetSupportsPauseGraph();
    }

    public override void Dispose()
    {
      RemoveStreamSourceFilter();
      if (_firstFilterInputPin != null)
      {
        Release.ComObject("DRI first filter input pin", _firstFilterInputPin);
        _firstFilterInputPin = null;
      }
      base.Dispose();

      if (_tunerService != null)
      {
        _tunerService.Dispose();
        _tunerService = null;
      }
      if (_fdcService != null)
      {
        _fdcService.Dispose();
        _fdcService = null;
      }
      if (_auxService != null)
      {
        _auxService.Dispose();
        _auxService = null;
      }
      if (_encoderService != null)
      {
        _encoderService.Dispose();
        _encoderService = null;
      }
      if (_casService != null)
      {
        _casService.Dispose();
        _casService = null;
      }
      if (_securityService != null)
      {
        _securityService.Dispose();
        _securityService = null;
      }
      if (_avTransportService != null)
      {
        //_avTransportService.Stop((uint)_avTransportId);
        _avTransportService.Dispose();
        _avTransportService = null;
      }
      if (_connectionManagerService != null)
      {
        try
        {
          _connectionManagerService.ConnectionComplete(_connectionId);
        }
        catch (Exception ex)
        {
          Log.Log.Debug("debug: connectioncomplete ex {0}", ex.ToString());
        }
        _connectionManagerService.Dispose();
        _connectionManagerService = null;
      }

      if (_deviceConnection != null)
      {
        _deviceConnection.Disconnect();
        _deviceConnection = null;
      }
    }

    /// <summary>
    /// Build the graph.
    /// </summary>
    public override void BuildGraph()
    {
      try
      {
        if (_graphState != GraphState.Idle)
        {
          Log.Log.Info("DRI CC: graph already built");
          return;
        }
        Log.Log.Info("DRI CC: build graph");
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);
        AddTsWriterFilterToGraph();
        _firstFilterInputPin = DsFindPin.ByDirection(_filterTsWriter, PinDirection.Input, 0);
        AddStreamSourceFilter(DefaultUrl);
        FilterGraphTools.SaveGraphFile(_graphBuilder, _name + " - DRI Graph.grf");

        Log.Log.Debug("DRI CC: connect to device");
        _deviceConnection = _controlPoint.Connect(_descriptor.RootDescriptor, _descriptor.DeviceUUID, DriExtendedDataTypes.ResolveDataType);

        // services
        Log.Log.Debug("DRI CC: setup services");
        _stateVariableDelegate = new StateVariableChangedDlgt(StateVariableChanged);
        _tunerService = new TunerService(_deviceConnection.Device, _stateVariableDelegate);
        _fdcService = new FdcService(_deviceConnection.Device, _stateVariableDelegate);
        _auxService = new AuxService(_deviceConnection.Device, _stateVariableDelegate);
        _encoderService = new EncoderService(_deviceConnection.Device, _stateVariableDelegate);
        _casService = new CasService(_deviceConnection.Device, _stateVariableDelegate);
        _muxService = new MuxService(_deviceConnection.Device);
        _securityService = new SecurityService(_deviceConnection.Device, _stateVariableDelegate);
        _diagService = new DiagService(_deviceConnection.Device);
        _avTransportService = new AvTransportService(_deviceConnection.Device, _stateVariableDelegate);
        _connectionManagerService = new ConnectionManagerService(_deviceConnection.Device, _stateVariableDelegate);

        int rcsId = -1;
        try
        {
          _connectionManagerService.PrepareForConnection(string.Empty, string.Empty, -1, UpnpConnectionDirection.Output, out _connectionId, out _avTransportId, out rcsId);
        }
        catch
        {
          Log.Log.Debug("PrepareForConnection FAILED!!! :(");
        }
        Log.Log.Debug("DRI CC: PrepareForConnection, connection ID = {0}, AV transport ID = {1}", _connectionId, _avTransportId);

        LogDeviceInfo();

        _graphState = GraphState.Created;
      }
      catch (Exception)
      {
        Dispose();
        throw;
      }
    }

    private void LogDeviceInfo()
    {
      try
      {
        Log.Log.Debug("DRI CC: current tuner status...");
        bool isCarrierLocked = false;
        uint frequency = 0;
        DriTunerModulation modulation = DriTunerModulation.All;
        bool isPcrLocked = false;
        int signalLevel = 0;
        uint snr = 0;
        _tunerService.GetTunerParameters(out isCarrierLocked, out frequency, out modulation, out isPcrLocked, out signalLevel, out snr);
        Log.Log.Debug("  carrier lock = {0}", isCarrierLocked);
        Log.Log.Debug("  frequency    = {0} kHz", frequency);
        Log.Log.Debug("  modulation   = {0}", modulation.ToString());
        Log.Log.Debug("  PCR lock     = {0}", isPcrLocked);
        Log.Log.Debug("  signal level = {0} dBmV", signalLevel);
        Log.Log.Debug("  SNR          = {0} dB", snr);

        Log.Log.Debug("DRI CC: current forward data channel status...");
        uint bitrate = 0;
        bool spectrumInversion = false;
        IList<ushort> pids;
        _fdcService.GetFdcStatus(out bitrate, out isCarrierLocked, out frequency, out spectrumInversion, out pids);
        Log.Log.Debug("  bitrate           = {0} kbps", bitrate);
        Log.Log.Debug("  carrier lock      = {0}", isCarrierLocked);
        Log.Log.Debug("  frequency         = {0} kHz", frequency);
        Log.Log.Debug("  spectrum inverted = {0}", spectrumInversion);
        Log.Log.Debug("  PIDs              = {0}", string.Join(", ", pids));

        IList<DriAuxFormat> formats;
        byte svideoInputCount = 0;
        byte compositeInputCount = 0;
        if (_auxService.GetAuxCapabilities(out formats, out svideoInputCount, out compositeInputCount))
        {
          Log.Log.Debug("DRI CC: auxiliary input info...");
          Log.Log.Debug("  supported formats     = {0}", string.Join(", ", formats.Select(x => x.ToString())));
          Log.Log.Debug("  S-video input count   = {0}", svideoInputCount);
          Log.Log.Debug("  composite input count = {0}", compositeInputCount);
        }
        else
        {
          Log.Log.Debug("DRI CC: auxiliary inputs not present/supported");
        }

        IList<DriEncoderAudioProfile> audioProfiles;
        IList<DriEncoderVideoProfile> videoProfiles;
        _encoderService.GetEncoderCapabilities(out audioProfiles, out videoProfiles);
        if (audioProfiles.Count > 0)
        {
          Log.Log.Debug("DRI CC: encoder audio profiles...");
          foreach (DriEncoderAudioProfile ap in audioProfiles)
          {
            Log.Log.Debug("  codec = {0}, bit depth = {1}, channel count = {2}, sample rate = {3} Hz", Enum.GetName(typeof(DriEncoderAudioAlgorithm), ap.AudioAlgorithmCode), ap.BitDepth, ap.NumberChannel, ap.SamplingRate);
          }
        }
        if (videoProfiles.Count > 0)
        {
          Log.Log.Debug("DRI CC: encoder video profiles...");
          foreach (DriEncoderVideoProfile vp in videoProfiles)
          {
            Log.Log.Debug("  hor. pixels = {0}, vert. pixels = {1}, aspect ratio = {2}, frame rate = {3}, {4}", vp.HorizontalSize, vp.VerticalSize, Enum.GetName(typeof(DriEncoderVideoAspectRatio), vp.AspectRatioInformation), Enum.GetName(typeof(DriEncoderVideoFrameRate), vp.FrameRateCode));
          }
        }

        uint maxAudioBitrate = 0;
        uint minAudioBitrate = 0;
        DriEncoderAudioMode audioBitrateMode = DriEncoderAudioMode.CBR;
        uint audioBitrateStepping = 0;
        uint audioBitrate = 0;
        byte audioProfileIndex = 0;
        bool isMuted = false;
        bool sapDetected = false; // second audio program (additional audio stream)
        bool sapActive = false;
        DriEncoderFieldOrder fieldOrder = DriEncoderFieldOrder.HIGHER;
        DriEncoderInputSelection source = DriEncoderInputSelection.AUX;
        bool noiseFilterActive = false;
        bool pulldownDetected = false;
        bool pulldownActive = false;
        uint maxVideoBitrate = 0;
        uint minVideoBitrate = 0;
        DriEncoderVideoMode videoBitrateMode = DriEncoderVideoMode.CBR;
        uint videoBitrate = 0;
        uint videoBitrateStepping = 0;
        byte videoProfileIndex = 0;
        _encoderService.GetEncoderParameters(out maxAudioBitrate, out minAudioBitrate, out audioBitrateMode,
          out audioBitrateStepping, out audioBitrate, out audioProfileIndex, out isMuted,
          out fieldOrder, out source, out noiseFilterActive, out pulldownDetected,
          out pulldownActive, out sapDetected, out sapActive, out maxVideoBitrate, out minVideoBitrate,
          out videoBitrateMode, out videoBitrate, out videoBitrateStepping, out videoProfileIndex);
        Log.Log.Debug("DRI CC: current encoder audio parameters...");
        Log.Log.Debug("  max bitrate  = {0} kbps", maxAudioBitrate);
        Log.Log.Debug("  min bitrate  = {0} kbps", minAudioBitrate);
        Log.Log.Debug("  bitrate mode = {0}", Enum.GetName(typeof(DriEncoderAudioMode), audioBitrateMode));
        Log.Log.Debug("  bitrate step = {0} kbps", audioBitrateStepping);
        Log.Log.Debug("  bitrate      = {0} kbps", audioBitrate);
        Log.Log.Debug("  profile      = {0}", audioProfileIndex);
        Log.Log.Debug("  is muted     = {0}", isMuted);
        Log.Log.Debug("  SAP detected = {0}", sapDetected);
        Log.Log.Debug("  SAP active   = {0}", sapActive);
        Log.Log.Debug("DRI CC: current encoder video parameters...");
        Log.Log.Debug("  max bitrate  = {0} kbps", maxVideoBitrate);
        Log.Log.Debug("  min bitrate  = {0} kbps", minVideoBitrate);
        Log.Log.Debug("  bitrate mode = {0}", Enum.GetName(typeof(DriEncoderVideoMode), videoBitrateMode));
        Log.Log.Debug("  bitrate step = {0} kbps", videoBitrateStepping);
        Log.Log.Debug("  bitrate      = {0} kbps", videoBitrate);
        Log.Log.Debug("  profile      = {0}", videoProfileIndex);
        Log.Log.Debug("  field order  = {0}", Enum.GetName(typeof(DriEncoderFieldOrder), fieldOrder));
        Log.Log.Debug("  source       = {0}", Enum.GetName(typeof(DriEncoderInputSelection), source));
        Log.Log.Debug("  noise filter = {0}", noiseFilterActive);
        Log.Log.Debug("  3:2 detected = {0}", pulldownDetected);
        Log.Log.Debug("  3:2 active   = {0}", pulldownActive);

        Log.Log.Debug("DRI CC: card status...");
        DriCasCardStatus status = DriCasCardStatus.Removed;
        string manufacturer = string.Empty;
        string version = string.Empty;
        bool isDst = false;
        uint eaLocationCode = 0;
        byte ratingRegion = 0;
        int timeZone = 0;
        _casService.GetCardStatus(out status, out manufacturer, out version, out isDst, out eaLocationCode, out ratingRegion, out timeZone);
        Log.Log.Debug("  status        = {0}", status.ToString());
        Log.Log.Debug("  manufacturer  = {0}", manufacturer);
        Log.Log.Debug("  version       = {0}", version);
        Log.Log.Debug("  time zone     = {0}", timeZone);
        Log.Log.Debug("  DST           = {0}", isDst);
        Log.Log.Debug("  EA loc. code  = {0}", eaLocationCode);  // EA = emergency alert
        Log.Log.Debug("  rating region = {0}", ratingRegion);

        Log.Log.Debug("DRI CC: diag service parameters...");
        string value = string.Empty;
        bool isVolatile = false;
        foreach (DriDiagParameter p in DriDiagParameter.Values)
        {
          _diagService.GetParameter(p, out value, out isVolatile);
          Log.Log.Debug("  {0}{1} = {2}", p.ToString(), isVolatile ? " [volatile]" : "", value);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("EXCEPTION: {0}", ex.ToString());
      }
    }

    /// <summary>
    /// Add the stream source filter to the graph.
    /// </summary>
    /// <param name="url">The URL to </param>
    private void AddStreamSourceFilter(string url)
    {
      Log.Log.Info("DRI CC: add source filter");
      _filterStreamSource = FilterGraphTools.AddFilterFromClsid(_graphBuilder, SourceFilterClsid,
                                                                  "DRI Source Filter");
      AMMediaType mpeg2ProgramStream = new AMMediaType();
      mpeg2ProgramStream.majorType = MediaType.Stream;
      mpeg2ProgramStream.subType = MediaSubType.Mpeg2Transport;
      mpeg2ProgramStream.unkPtr = IntPtr.Zero;
      mpeg2ProgramStream.sampleSize = 0;
      mpeg2ProgramStream.temporalCompression = false;
      mpeg2ProgramStream.fixedSizeSamples = true;
      mpeg2ProgramStream.formatType = FormatType.None;
      mpeg2ProgramStream.formatSize = 0;
      mpeg2ProgramStream.formatPtr = IntPtr.Zero;
      ((IFileSourceFilter)_filterStreamSource).Load(url, mpeg2ProgramStream);

      // (Re)connect the source filter to the first filter in the chain.
      IPin sourceOutputPin = DsFindPin.ByDirection(_filterStreamSource, PinDirection.Output, 0);
      if (sourceOutputPin == null)
      {
        throw new TvException("DRI CC: failed to find source filter output pin");
      }
      int hr = _graphBuilder.Connect(sourceOutputPin, _firstFilterInputPin);
      Release.ComObject("DRI source filter output pin", sourceOutputPin);
      if (hr != 0)
      {
        throw new TvExceptionGraphBuildingFailed(string.Format("DRI CC: failed to connect source filter into graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr)));
      }
      Log.Log.Debug("DRI CC: result = success");
    }

    /// <summary>
    /// Remove the stream source filter from the graph.
    /// </summary>
    private void RemoveStreamSourceFilter()
    {
      Log.Log.Info("DRI CC: remove source filter");
      if (_filterStreamSource != null)
      {
        if (_graphBuilder != null)
        {
          _graphBuilder.RemoveFilter(_filterStreamSource);
        }
        Release.ComObject("DRI source filter", _filterStreamSource);
        _filterStreamSource = null;
      }
    }

    /// <summary>
    /// Check if the device can tune to a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the device can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel != null && atscChannel.ModulationType == ModulationType.ModNotSet)
      {
        return true;
      }
      return false;
    }

    private void StateVariableChanged(CpStateVariable stateVariable, object newValue)
    {
      Log.Log.Debug("DRI CC: state variable {0} for tuner {3} service {1} changed to {2}", stateVariable.Name, stateVariable.ParentService.FullQualifiedName, newValue ?? "[null]", _devicePath);
    }
  }
}
