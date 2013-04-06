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

namespace TvLibrary.Implementations.Dri
{
  public class TunerDri : TvCardATSC
  {
    #region variables

    private DeviceDescriptor _descriptor = null;
    private UPnPControlPoint _controlPoint = null;
    private DeviceConnection _deviceConnection = null;

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

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDri"/> class.
    /// </summary>
    /// <param name="descriptor">The device description. Essentially an XML document describing the device interface.</param>
    /// <param name="controlPoint">The control point to use to connect to the device.</param>
    public TunerDri(DeviceDescriptor descriptor, UPnPControlPoint controlPoint)
      : base(null)
    {
      try
      {
        _descriptor = descriptor;
        _controlPoint = controlPoint;
        _name = descriptor.FriendlyName;
        _devicePath = descriptor.DeviceUDN;   // unique device name is as good as a device path for a unique identifier
        _deviceConnection = _controlPoint.Connect(descriptor.RootDescriptor, descriptor.DeviceUUID, DriExtendedDataTypes.ResolveDataType);

        // services
        Log.Log.Debug("debug: setup services");
        _tunerService = new TunerService(_deviceConnection.Device);
        _fdcService = new FdcService(_deviceConnection.Device);
        _auxService = new AuxService(_deviceConnection.Device);
        _encoderService = new EncoderService(_deviceConnection.Device);
        _casService = new CasService(_deviceConnection.Device);
        _muxService = new MuxService(_deviceConnection.Device);
        _securityService = new SecurityService(_deviceConnection.Device);
        _diagService = new DiagService(_deviceConnection.Device);
        _avTransportService = new AvTransportService(_deviceConnection.Device);
        _connectionManagerService = new ConnectionManagerService(_deviceConnection.Device);

        GetPreloadBitAndCardId();
        GetSupportsPauseGraph();
      }
      catch (Exception ex)
      {
        Log.Log.WriteFile("debug: got exception, {0}", ex);
      }
    }

    public override void Dispose()
    {
      base.Dispose();
      _tunerService.Dispose();
      _fdcService.Dispose();
      _auxService.Dispose();
      _encoderService.Dispose();
      _casService.Dispose();
      _securityService.Dispose();
      _avTransportService.Dispose();
      _connectionManagerService.Dispose();
      _deviceConnection.Disconnect();
    }
  }
}
