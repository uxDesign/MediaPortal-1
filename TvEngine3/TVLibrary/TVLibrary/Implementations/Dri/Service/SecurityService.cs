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
  public class SecurityService : IDisposable
  {
    private CpDevice _device = null;
    private CpService _service = null;
    private StateVariableChangedDlgt _stateVariableDelegate = null;

    private CpAction _setDrmAction = null;

    public SecurityService(CpDevice device, StateVariableChangedDlgt svChangeDlg)
    {
      _device = device;
      if (!device.Services.TryGetValue("urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Security", out _service))
      {
        // Security is a mandatory service, so this is an error.
        throw new NotImplementedException("DRI: device does not implement a Security service");
      }

      _service.Actions.TryGetValue("SetDRM", out _setDrmAction);

      if (svChangeDlg != null)
      {
        _stateVariableDelegate = svChangeDlg;
        _service.StateVariableChanged += _stateVariableDelegate;
        _service.SubscribeStateVariables();
      }
    }

    public void Dispose()
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

    /// <summary>
    /// Upon receipt of the SetDRM action, the DRIT SHALL set the DrmPairingStatus state variable to “Red” and switch
    /// to the designated DRM systems in less than 5s.
    /// </summary>
    /// <param name="newDrm">This argument sets the DrmUUID state variable.</param>
    public void SetDrm(string newDrm)
    {
      _setDrmAction.InvokeAction(new List<object> { newDrm });
    }
  }
}
