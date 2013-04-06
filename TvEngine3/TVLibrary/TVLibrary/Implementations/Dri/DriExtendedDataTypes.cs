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
using TvLibrary.Log;
using UPnP.Infrastructure.Common;
using System.Xml;

namespace TvLibrary.Implementations.Dri
{
  /// <summary>
  /// This class resolves OpenCable DRI-specific data types.
  /// </summary>
  public class DriExtendedDataTypes
  {
    /// <summary>
    /// Resolve a DRI-specific data type.
    /// </summary>
    /// <param name="dataTypeName">The fully qualified name of the data type.</param>
    /// <param name="dataType">The data type.</param>
    /// <returns><c>true</c> if the data type has been resolved, otherwise <c>false</c></returns>
    public static bool ResolveDataType(string dataTypeName, out UPnPExtendedDataType dataType)
    {
      Log.Log.Debug("debug: resolve data type, name = {0}", dataTypeName);
      dataType = new UpnpDtDummy();
      return true;
    }
  }

  /// <summary>
  /// Data type serializing and deserializing <see cref="Share"/> objects.
  /// </summary>
  public class UpnpDtDummy : UPnPExtendedDataType
  {
    public const string DATATYPE_NAME = "DtDummy";

    internal UpnpDtDummy()
      : base("urn:schemas-opencable-com", DATATYPE_NAME)
    {
    }

    public override bool SupportsStringEquivalent
    {
      get
      {
        Log.Log.Debug("DtDummy: SupportsStringEquivalent");
        return false;
      }
    }

    public override bool IsNullable
    {
      get
      {
        Log.Log.Debug("DtDummy: IsNullable");
        return false;
      }
    }

    public override bool IsAssignableFrom(Type type)
    {
      Log.Log.Debug("DtDummy: IsAssignableFrom");
      return false;
    }

    protected override void DoSerializeValue(object value, bool forceSimpleValue, XmlWriter writer)
    {
      Log.Log.Debug("DtDummy: DoSerializeValue");
    }

    protected override object DoDeserializeValue(XmlReader reader, bool isSimpleValue)
    {
      Log.Log.Debug("DtDummy: DoDeserializeValue");
      reader.ReadStartElement(); // Read start of enclosing element
      //Share result = Share.Deserialize(reader);
      reader.ReadEndElement(); // End of enclosing element
      return string.Empty;
    }
  }
}
