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
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.View
{
  /// <summary>
  /// Summary description for ViewDefinition.
  /// </summary>
  [Serializable]
  public class ViewDefinitionNew
  {
    protected List<FilterLevel> _listFilterLevels = new List<FilterLevel>();
    protected List<ViewDefinitionNew> _listSubViews = new List<ViewDefinitionNew>();
    protected List<FilterDefinitionNew> _listFilters = new List<FilterDefinitionNew>();
    private string _name;
    private string _parent;

    public ViewDefinitionNew() {}

    [XmlElement("Name")]
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    [XmlElement("Parent")]
    public string Parent
    {
      get { return _parent; }
      set { _parent = value; }
    }

    [XmlElement("Filter")]
    public List<FilterDefinitionNew> Filters
    {
      get { return _listFilters; }
      set { _listFilters = value; }
    }

    [XmlElement("FilterLevel")]
    public List<FilterLevel> Levels
    {
      get { return _listFilterLevels; }
      set { _listFilterLevels = value; }
    }

    [XmlElement("SubView")]
    public List<ViewDefinitionNew> SubViews
    {
      get { return _listSubViews; }
      set { _listSubViews = value; }
    }

    public string LocalizedName
    {
      get
      {
        String localizedName = _name;
        GUILocalizeStrings.LocalizeLabel(ref localizedName);
        return localizedName;
      }
    }

    public override string ToString()
    {
      return LocalizedName;
    }
  }
}