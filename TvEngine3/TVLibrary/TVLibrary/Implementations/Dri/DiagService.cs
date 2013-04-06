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
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri
{
  public sealed class DriDiagParameter
  {
    private readonly string _name;
    private static readonly IDictionary<string, DriDiagParameter> _values = new Dictionary<string, DriDiagParameter>();

    /// <summary>
    /// Serial Number of the DRIT.
    /// </summary>
    public static readonly DriDiagParameter HostSerialNumber = new DriDiagParameter("Host Serial Number");
    /// <summary>
    /// Unique ID of the DRIT used for Card/Host binding.
    /// </summary>
    public static readonly DriDiagParameter HostId = new DriDiagParameter("Host ID");
    /// <summary>
    /// Explicit description of the current power status.
    /// </summary>
    public static readonly DriDiagParameter HostPowerStatus = new DriDiagParameter("Host Power Status");
    /// <summary>
    /// Explicit description of the current boot status.
    /// </summary>
    public static readonly DriDiagParameter HostBootStatus = new DriDiagParameter("Host Boot Status");
    /// <summary>
    /// Explicit description of the current memory allocation.
    /// </summary>
    public static readonly DriDiagParameter HostMemoryReport = new DriDiagParameter("Host Memory Report");
    /// <summary>
    /// Explicit description of the DRM application supported by the devices, including name, version number and date.
    /// </summary>
    public static readonly DriDiagParameter HostApplication = new DriDiagParameter("Host Application");
    /// <summary>
    /// Explicit description of the DRIT firmware including name, version number and date.
    /// </summary>
    public static readonly DriDiagParameter HostFirmware = new DriDiagParameter("Host Firmware");

    private DriDiagParameter(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      DriDiagParameter diagParam = obj as DriDiagParameter;
      if (diagParam != null && this == diagParam)
      {
        return true;
      }
      return false;
    }

    public static explicit operator DriDiagParameter(string name)
    {
      DriDiagParameter value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DriDiagParameter diagParam)
    {
      return diagParam._name;
    }
  }

  public class DiagService
  {
    private CpDevice _device = null;
    private CpService _service = null;

    private CpAction _getParameterAction = null;

    public DiagService(CpDevice device)
    {
      _device = device;
      if (!device.Services.TryGetValue("urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Diag", out _service))
      {
        // Diag is a mandatory service, so this is an error.
        Log.Log.Error("DRI: device {0} does not implement a Diag service", device.UDN);
        return;
      }

      _service.Actions.TryGetValue("GetParameter", out _getParameterAction);
    }

    /// <summary>
    /// Upon receipt of the GetParameter action, the DRIT SHALL return the value and the type of the parameter in less
    /// than 1s.
    /// </summary>
    /// <param name="parameter">This argument sets the A_ARG_TYPE_Parameter state variable.</param>
    /// <param name="value">This argument provides the value of the A_ARG_TYPE_Value state variable when the action response is created.</param>
    /// <param name="isVolatile">This argument provides the value of the A_ARG_TYPE_Volatile state variable when the action response is created.</param>
    public void GetParameter(DriDiagParameter parameter, out string value, out bool isVolatile)
    {
      IList<object> outParams = _getParameterAction.InvokeAction(new List<object> { parameter.ToString() });
      value = (string)outParams[0];
      isVolatile = (bool)outParams[1];
    }
  }
}
