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
using MediaPortal.Configuration;
using MediaPortal.GUI.View;

namespace MediaPortal.GUI.Library
{
  public class ViewHandlerNew
  {
    #region Fields

    protected ViewDefinitionNew currentView;
    protected int currentLevel = 0;
    protected List<ViewDefinitionNew> views = new List<ViewDefinitionNew>();

    #endregion

    #region Public Static Properties

    /// <summary>
    /// Returns the path to the directory, which contains all available default view definition files.
    /// </summary>
    public static string DefaultsDirectory
    {
      get { return Config.GetSubFolder(Config.Dir.Base, @"Defaults"); }
    }

    #endregion

    #region Public Virtual Properties

    public virtual int MaxLevels
    {
      get { return currentView.Levels.Count; }
    }

    public virtual int CurrentLevel
    {
      get { return currentLevel; }
      set
      {
        if (value < 0 || value >= currentView.Levels.Count)
        {
          return;
        }
        currentLevel = value;
      }
    }

    public virtual ViewDefinitionNew View
    {
      get { return currentView; }
      set { currentView = value; }
    }

    public virtual List<ViewDefinitionNew> Views
    {
      get { return views; }
      set { views = value; }
    }

    public virtual string LocalizedCurrentView
    {
      get
      {
        if (currentView == null)
        {
          return string.Empty;
        }
        return currentView.LocalizedName;
      }
    }
    
    /// <summary>
    /// Property for the view level name as localized string
    /// This will return the view level (ie. the where in view
    /// definition so artist, genre, actor etc)
    /// </summary>
    public virtual string LocalizedCurrentViewLevel
    {
      get
      {
        if (currentView == null)
        {
          return string.Empty;
        }
        
        if (currentView.Levels.Count == 0)
        {
          return currentView.LocalizedName;
        }
        
        FilterLevel def = (FilterLevel)currentView.Levels[currentLevel];

        return(GetLocalizedViewLevel(def.Selection));
      }
    }
    
    protected virtual string GetLocalizedViewLevel(String lvlName)
    {
      return lvlName;
    }

    public virtual string CurrentView
    {
      get
      {
        if (currentView == null)
        {
          return string.Empty;
        }
        return currentView.Name;
      }
      set
      {
        bool done = false;
        
        // Find out, if CurrentView was a Main View
        List<ViewDefinitionNew> searchViews = new List<ViewDefinitionNew>();

        if (currentView != null)
        {
          foreach (ViewDefinitionNew definition in views)
          {
            if (definition.ToString() == currentView.ToString())
            {
              searchViews = currentView.SubViews;
              done = true;
              break;
            }
          }
        }
        if (!done)
        {
          searchViews = views;
        }

        done = false;
        foreach (ViewDefinitionNew definition in searchViews)
        {
          if (definition.ToString() == value)
          {
            currentView = definition;
            CurrentLevel = 0;
            done = true;
            break;
          }
        }
        if (!done)
        {
          if (views.Count > 0)
          {
            currentView = (ViewDefinitionNew)views[0];
          }
        }
      }
    }

    public virtual int CurrentViewIndex
    {
      get { return views.IndexOf(currentView); }
    }

    public virtual string CurrentLevelWhere
    {
      get
      {
        FilterLevel level = (FilterLevel)currentView.Levels[CurrentLevel];
        if (level == null)
        {
          return string.Empty;
        }
        return level.Selection;
      }
    }

    #endregion
  }
}