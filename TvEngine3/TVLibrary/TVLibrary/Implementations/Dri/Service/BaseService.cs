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
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri.Service
{
  public class BaseService : IDisposable
  {
    protected CpDevice _device = null;
    protected CpService _service = null;
    protected StateVariableChangedDlgt _stateVariableDelegate = null;

    public BaseService(CpDevice device, string serviceName, bool isOptional = false)
    {
      _device = device;
      if (!device.Services.TryGetValue(serviceName, out _service) && !isOptional)
      {
        string unqualifiedServicename = serviceName.Substring(serviceName.LastIndexOf(":"));
        throw new NotImplementedException(string.Format("DRI: device does not implement a {0} service", unqualifiedServicename));
      }
    }

    public void Dispose()
    {
      UnsubscribeStateVariables();
    }

    public void SubscribeStateVariables(StateVariableChangedDlgt svChangeDlg)
    {
      UnsubscribeStateVariables();
      if (svChangeDlg != null && _service != null)
      {
        _stateVariableDelegate = svChangeDlg;
        _service.StateVariableChanged += _stateVariableDelegate;
        _service.SubscribeStateVariables();
      }
    }

    public void UnsubscribeStateVariables()
    {
      if (_stateVariableDelegate != null && _service != null)
      {
        if (_service.IsStateVariablesSubscribed)
        {
          _service.UnsubscribeStateVariables();
        }
        _service.StateVariableChanged -= _stateVariableDelegate;
        _stateVariableDelegate = null;
      }
    }
  }
}
