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

    private CpAction _setDrmAction = null;

    public SecurityService(CpDevice device)
    {
      _device = device;
      if (!device.Services.TryGetValue("urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Security", out _service))
      {
        // Security is a mandatory service, so this is an error.
        Log.Log.Error("DRI: device {0} does not implement a Security service", device.UDN);
        return;
      }

      _service.Actions.TryGetValue("SetDRM", out _setDrmAction);
      _service.SubscribeStateVariables();
    }

    public void Dispose()
    {
      _service.UnsubscribeStateVariables();
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
