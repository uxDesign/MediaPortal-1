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
using System.Xml.XPath;
using TvLibrary.Implementations.DVB;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;
using TvLibrary.Implementations.Helper;
using DirectShowLib;

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

      BuildGraph();
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
        //_connectionManagerService.ConnectionComplete(_connectionId);
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

        try
        {
          int rcsId = -1;
          bool executed = _connectionManagerService.PrepareForConnection(string.Empty, string.Empty, -1, UpnpConnectionDirection.Output, out _connectionId, out _avTransportId, out rcsId);
          Log.Log.Debug("debug: executed = {0}, connection ID = {1}, AV transport ID = {2}, RCS ID = {3}", executed, _connectionId, _avTransportId, rcsId);
        }
        catch (Exception ex)
        {
          Log.Log.Debug("debug: exception :(\r\n{0}", ex);
        }

        _graphState = GraphState.Created;
      }
      catch (Exception)
      {
        Dispose();
        throw;
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

    private void StateVariableChanged(CpStateVariable stateVariable, object newValue)
    {
      Log.Log.Debug("DRI CC: state variable {0} for service {1} changed to {2}", stateVariable.Name, stateVariable.ParentService.FullQualifiedName, newValue ?? "[null]");
    }
  }
}
