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
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.GUI.View;
using MediaPortal.GUI.Library;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class BaseViewsNew : SectionSettings
  {
    #region Variables

    private DataTable datasetFilters;
    private ViewDefinitionNew currentView;
    public List<ViewDefinitionNew> views;
    private bool updating = false;
    public bool settingsChanged = false;

    private List<string> _selections = new List<string>();
    private List<string> _viewsAs = new List<string>();
    private List<string> _sortBy = new List<string>();

    // Drag & Drop
    private int _dragDropCurrentIndex = -1;
    private Rectangle _dragDropRectangle;
    private int _dragDropSourceIndex;
    private int _dragDropTargetIndex;

    private string _section = string.Empty;

    #endregion

    #region Properties

    public string[] Selections
    {
      get { return _selections.ToArray(); }
      set
      {
        _selections.Clear();
        _selections.AddRange(value);
      }
    }

    public string[] ViewsAs
    {
      get { return _viewsAs.ToArray(); }
      set
      {
        _viewsAs.Clear();
        _viewsAs.AddRange(value);
      }
    }

    public string[] SortBy
    {
      get { return _sortBy.ToArray(); }
      set
      {
        _sortBy.Clear();
        _sortBy.AddRange(value);
      }
    }

    public string Section
    {
      set
      {
        _section = value;
      }
    }

    #endregion

    #region ctor

    public BaseViewsNew()
      : base("<Unknown>")
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public BaseViewsNew(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    #endregion

    #region Initialisation

    /// <summary>
    /// Set up the Datagrid column and the DataTable to which the grid is bound
    /// </summary>

    private void SetupGrid()
    {
      // Declare and initialize local variables used
      DataColumn dtCol = null; //Data Column variable
      string[] arrColumnNames = null; //string array variable

      // Fill the Combo Values
      foreach (string strText in Selections)
      {
        dgSelection.Items.Add(strText);
      }

      foreach (string strText in ViewsAs)
      {
        dgViewAs.Items.Add(strText);
      }

      foreach (string strText in SortBy)
      {
        dgSortBy.Items.Add(strText);
      }

      //Create the String array object, initialize the array with the column
      //names to be displayed
      arrColumnNames = new string[3];
      arrColumnNames[0] = "Selection";
      arrColumnNames[1] = "ViewAs";
      arrColumnNames[2] = "SortBy";
      

      //Create the Data Table object which will then be used to hold
      //columns and rows
      datasetFilters = new DataTable();

      //Add the string array of columns to the DataColumn object       
      for (int i = 0; i < arrColumnNames.Length; i++)
      {
        string str = arrColumnNames[i];
        dtCol = new DataColumn(str);
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        datasetFilters.Columns.Add(dtCol);
      }

      // Add 2 columns with checkbox at the end of the Datarow     
      DataColumn dtcCheck = new DataColumn("SortAsc"); //create the data column object
      dtcCheck.DataType = Type.GetType("System.Boolean"); //Set its data Type
      dtcCheck.DefaultValue = true; //Set the default value
      dtcCheck.AllowDBNull = false;
      datasetFilters.Columns.Add(dtcCheck); //Add the above column to the Data Table

      DataColumn skipCheck = new DataColumn("Skip"); //create the data column object
      skipCheck.DataType = Type.GetType("System.Boolean"); //Set its data Type
      skipCheck.DefaultValue = false; //Set the default value
      skipCheck.AllowDBNull = false;
      datasetFilters.Columns.Add(skipCheck); //Add the above column to the Data Table

      // Set the Data Properties for the field to map to the data table
      dgSelection.DataPropertyName = "Selection";
      dgViewAs.DataPropertyName = "ViewAs";
      dgSortBy.DataPropertyName = "SortBy";
      dgSortAsc.DataPropertyName = "SortAsc";
      dgSkip.DataPropertyName = "Skip";

      //Set the Data Grid Source as the Data Table created above
      dataGrid.AutoGenerateColumns = false;
      dataGrid.DataSource = datasetFilters;
    }

    private void LoadTreeView()
    {
      updating = true;

      foreach (ViewDefinitionNew view in views)
      {
        TreeNode node = new TreeNode();
        node.Text = view.LocalizedName;
        node.Tag = view;
        foreach (ViewDefinitionNew subView in view.SubViews)
        {
          TreeNode subNode = new TreeNode(subView.LocalizedName);
          subNode.Tag = subView;
          node.Nodes.Add(subNode);
        }
        treeViewMenu.Nodes.Add(node);
      }

      treeViewMenu.ExpandAll();
      updating = false;

    }

    #endregion

    #region Private Methods

    private void FillViewGrid()
    {
      datasetFilters.Clear();
      if (currentView == null)
      {
        return;
      }

      //fill in all rows...
      foreach (FilterLevel level in currentView.Levels)
      {
        datasetFilters.Rows.Add(
          new object[]
            {
              level.Selection,
              level.DefaultView,
              level.SortBy,
              level.SortAscending,
              level.SkipLevel,
            }
          );
      }
      dataGrid.DataSource = datasetFilters;
    }

    private void StoreGridInView()
    {
      if (updating)
      {
        return;
      }
      if (dataGrid.DataSource == null)
      {
        return;
      }
      if (currentView == null)
      {
        return;
      }

      currentView.Levels.Clear();
      foreach (DataRow row in datasetFilters.Rows)
      {
        FilterLevel level = new FilterLevel();
        level.Selection = (string)row[0];
        level.DefaultView = (string)row[1];
        level.SortBy = (string)row[2];
        level.SortAscending = (bool)row[3];
        level.SkipLevel = (bool)row[4];
        currentView.Levels.Add(level);
      }
    }

    #endregion

    #region Event Handler
    
    /// <summary>
    /// Add a new View entry 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnAdd_Click(object sender, EventArgs e)
    {
      settingsChanged = true;

      StoreGridInView(); // Save possible pending changes in current Node

      ViewDefinitionNew view = new ViewDefinitionNew();
      TreeNode treeNode = new TreeNode("New View");
      treeNode.Tag = view;
      treeViewMenu.Nodes.Add(treeNode);
      treeViewMenu.SelectedNode = treeNode;

      datasetFilters.Rows.Clear();
      DataRow row = datasetFilters.NewRow();
      row[0] = "";
      row[1] = ViewsAs[0]; // Set default Value
      row[2] = SortBy[0];  // Set default Value
      row[3] = true;
      row[4] = false;
      datasetFilters.Rows.Add(row);

      btnDeleteView.Enabled = false;
      btnEditFilter.Enabled = false;
    }

    /// <summary>
    /// Delete the selected View
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnDelete_Click(object sender, EventArgs e)
    {
      updating = true;

      TreeNode selectedNode = treeViewMenu.SelectedNode;
      if (selectedNode == null)
      {
        return;
      }

      bool removeNode = true;
      if (selectedNode.Nodes.Count > 0)
      {
        if (MessageBox.Show("The selected View has SubViews.\r\nDo you really want to delete", "Delete View", MessageBoxButtons.YesNo,MessageBoxIcon.Warning) == DialogResult.No)
        {
          removeNode = false;
        }
      }

      if (removeNode)
      {
        settingsChanged = true;
        selectedNode.Remove();
        treeViewMenu.Invalidate();

        btnDeleteView.Enabled = false;
        btnEditFilter.Enabled = false;
      }

      updating = false;
    }

    /// <summary>
    /// Set defaults views (will copy view files from MPProgram\defaults directory)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSetDefaults_Click(object sender, EventArgs e)
    {
      string defaultViews = Path.Combine(ViewHandler.DefaultsDirectory, _section + "Views.xml");
      string customViews = Config.GetFile(Config.Dir.Config, _section + "Views.xml");

      if (File.Exists(defaultViews))
      {
        File.Copy(defaultViews, customViews, true);

        views.Clear();

        try
        {
          using (FileStream fileStream = new FileInfo(customViews).OpenRead())
          {
            XmlSerializer serializer = new XmlSerializer(typeof(List<ViewDefinitionNew>));
            views = (List<ViewDefinitionNew>)serializer.Deserialize(fileStream);
            fileStream.Close();
          }
        }
        catch (Exception) { }
        settingsChanged = true;
        LoadTreeView();
      }
    }

    #region TreeView

    /// <summary>
    /// Selection has changed. Save any changes to the node
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void treeViewMenu_BeforeSelect(object sender, TreeViewCancelEventArgs e)
    {
      if (treeViewMenu.SelectedNode == null)
      {
        return;
      }

      currentView = (ViewDefinitionNew)treeViewMenu.SelectedNode.Tag;
      StoreGridInView();
      treeViewMenu.SelectedNode.Tag = currentView;
    }

    /// <summary>
    /// A new node has been selected
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void treeViewMenu_AfterSelect(object sender, TreeViewEventArgs e)
    {
      currentView = (ViewDefinitionNew)treeViewMenu.SelectedNode.Tag;
      if (currentView.SubViews.Count > 0)
      {
        datasetFilters.Clear();
        dataGrid.Enabled = false;
      }
      else
      {
        FillViewGrid();
      }

      btnDeleteView.Enabled = true;
      btnEditFilter.Enabled = true;
    }

    /// <summary>
    /// Set the Localised Name into the Label Text
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void treeViewMenu_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
      // Don't allow empty labels
      if (e.Label == null)
      {
        e.CancelEdit = false;
        return;
      }
      
      ViewDefinitionNew view = (ViewDefinitionNew)e.Node.Tag;
      view.Name = e.Label;
      e.Node.Tag = view;
      e.Node.Text = view.LocalizedName;
      e.CancelEdit = true; // We want to have our localised version of the text
      settingsChanged = true;
    }

    #endregion

    #region Filter Grid

    /// <summary>
    /// Edit a new Filter
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btEditFilter_Click(object sender, EventArgs e)
    {
      BaseViewsFilter filterForm = new BaseViewsFilter();
      filterForm.ShowDialog();
    }

    /// <summary>
    /// Only allow valid values to be entered.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
      if (e.Exception == null) return;

      // If the user-specified value is invalid, cancel the change 
      if ((e.Context & DataGridViewDataErrorContexts.Commit) != 0 &&
          (typeof(FormatException).IsAssignableFrom(e.Exception.GetType()) ||
           typeof(ArgumentException).IsAssignableFrom(e.Exception.GetType())))
      {
        e.Cancel = true;
      }
      else
      {
        // Rethrow any exceptions that aren't related to the user input.
        e.ThrowException = true;
      }
    }

    /// <summary>
    /// Handles editing of data columns
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
      // For combo box and check box cells, commit any value change as soon
      // as it is made rather than waiting for the focus to leave the cell.
      if (!dataGrid.CurrentCell.OwningColumn.GetType().Equals(typeof(DataGridViewTextBoxColumn)))
      {
        dataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
      }
    }

    /// <summary>
    /// Handle the Keypress for the Filter Datagrid
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_KeyDown(object sender, KeyEventArgs e)
    {
      int rowSelected = -1;
      if (dataGrid.CurrentRow != null)
      {
        rowSelected = dataGrid.CurrentRow.Index;
      }

      switch (e.KeyCode)
      {
        case System.Windows.Forms.Keys.Insert:
          DataRow row = datasetFilters.NewRow();
          row[0] = "";
          row[1] = ViewsAs[0]; // Set default Value
          row[2] = SortBy[0];  // Set default Value
          row[3] = true;
          row[4] = false;
          if (rowSelected == -1)
          {
            rowSelected = 0;
          }
          datasetFilters.Rows.InsertAt(row, rowSelected + 1);
          e.Handled = true;
          settingsChanged = true;
          break;
        case System.Windows.Forms.Keys.Delete:
          if (rowSelected > -1)
          {
            datasetFilters.Rows.RemoveAt(rowSelected);
          }
          e.Handled = true;
          settingsChanged = true;
          break;
      }
    }

    #endregion

    #region Drag & Drop

    #region Treeview


    private void treeViewMenu_ItemDrag(object sender, ItemDragEventArgs e)
    {
      StoreGridInView(); // Save possible pending changes in current Node
      DoDragDrop(e.Item, DragDropEffects.Move | DragDropEffects.Copy);
    }

    private void treeViewMenu_DragEnter(object sender, DragEventArgs e)
    {
      // Ctrl-Key pressed? 
      if ((e.KeyState & 8) == 8)
      {
        e.Effect = DragDropEffects.Copy;
      }
      else
      {
        e.Effect = DragDropEffects.Move;
      }
    }

    private void treeViewMenu_DragDrop(object sender, DragEventArgs e)
    {
      TreeNode sourceNode = e.Data.GetData(typeof(TreeNode)) as TreeNode;

      Point p = treeViewMenu.PointToClient(new Point(e.X, e.Y));
      TreeNode targetNode = treeViewMenu.GetNodeAt(p);

      TreeNode newNode = (TreeNode)sourceNode.Clone();
      if (targetNode != null)
      {
        // Add new Node
        targetNode.Nodes.Add(newNode);
        ViewDefinitionNew view = (ViewDefinitionNew)targetNode.Tag;
        view.SubViews.Add((ViewDefinitionNew)sourceNode.Tag);
        targetNode.Tag = view;
      }
      else
      {
        // Add new Node to Root of Treeview
        treeViewMenu.Nodes.Add(newNode);
      }

      // if the Node was part of a parent node, we need to remove it also from the view
      TreeNode parentNode = sourceNode.Parent;
      if (parentNode != null)
      {
        ViewDefinitionNew view = (ViewDefinitionNew)parentNode.Tag;
        view.SubViews.Remove((ViewDefinitionNew)sourceNode.Tag);
        parentNode.Tag = view;
      }

      if (e.Effect == DragDropEffects.Move)
      {
        sourceNode.Remove();
      }
      treeViewMenu.ExpandAll();
      treeViewMenu.Invalidate();
    }

    #endregion

    #region Datagrid

    private void dataGrid_OnMouseDown(object sender, MouseEventArgs e)
    {
      DataGridView dgV = (DataGridView)sender;
      //stores values for drag/drop operations if necessary
      if (dgV.AllowDrop)
      {
        int selectedRow = dgV.HitTest(e.X, e.Y).RowIndex;
        if (selectedRow > -1)
        {
          Size DragSize = SystemInformation.DragSize;
          _dragDropRectangle = new Rectangle(new Point(e.X - (DragSize.Width / 2), e.Y - (DragSize.Height / 2)),
                                             DragSize);
          _dragDropSourceIndex = selectedRow;
        }
      }
      else
      {
        _dragDropRectangle = Rectangle.Empty;
      }

      base.OnMouseDown(e);
    }

    private void dataGrid_OnMouseMove(object sender, MouseEventArgs e)
    {
      DataGridView dgV = (DataGridView)sender;
      if (dgV.AllowDrop)
      {
        if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
        {
          if (_dragDropRectangle != Rectangle.Empty && !_dragDropRectangle.Contains(e.X, e.Y))
          {
            DragDropEffects DropEffect = dgV.DoDragDrop(dgV.Rows[_dragDropSourceIndex],
                                                        DragDropEffects.Move);
          }
        }
      }
      base.OnMouseMove(e);
    }

    private void dataGrid_OnDragOver(object sender, DragEventArgs e)
    {
      DataGridView dgV = (DataGridView)sender;
      //runs while the drag/drop is in progress
      if (dgV.AllowDrop)
      {
        e.Effect = DragDropEffects.Move;
        int CurRow =
          dgV.HitTest(dgV.PointToClient(new Point(e.X, e.Y)).X,
                      dgV.PointToClient(new Point(e.X, e.Y)).Y).RowIndex;
        if (_dragDropCurrentIndex != CurRow)
        {
          _dragDropCurrentIndex = CurRow;
          dgV.Invalidate(); //repaint
        }
      }
      base.OnDragOver(e);
    }

    private void dataGrid_OnDragDrop(object sender, DragEventArgs drgevent)
    {
      updating = true;
      DataGridView dgV = (DataGridView)sender;
      //runs after a drag/drop operation for column/row has completed
      if (dgV.AllowDrop)
      {
        if (drgevent.Effect == DragDropEffects.Move)
        {
          Point ClientPoint = dgV.PointToClient(new Point(drgevent.X, drgevent.Y));

          _dragDropTargetIndex = dgV.HitTest(ClientPoint.X, ClientPoint.Y).RowIndex;
          if (_dragDropTargetIndex > -1 && _dragDropCurrentIndex < dgV.RowCount - 1)
          {
            _dragDropCurrentIndex = -1;

            if (dgV.Name == "dataGrid")
            {
              DataRow row = datasetFilters.NewRow();
              // Copy the existing row elements, before removing it from table
              for (int i = 0; i < datasetFilters.Columns.Count; i++)
              {
                row[i] = datasetFilters.Rows[_dragDropSourceIndex][i];
              }
              datasetFilters.Rows.RemoveAt(_dragDropSourceIndex);

              if (_dragDropTargetIndex > _dragDropSourceIndex)
                _dragDropTargetIndex--;

              datasetFilters.Rows.InsertAt(row, _dragDropTargetIndex);
            }

            dgV.ClearSelection();
            dgV.Rows[_dragDropTargetIndex].Selected = true;
          }
        }
      }
      base.OnDragDrop(drgevent);
      updating = false;
    }

    private void dataGrid_OnCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
      DataGridView dgV = (DataGridView)sender;
      if (_dragDropCurrentIndex > -1)
      {
        if (e.RowIndex == _dragDropCurrentIndex && _dragDropCurrentIndex < dgV.RowCount - 1)
        {
          //if this cell is in the same row as the mouse cursor
          Pen p = new Pen(Color.Red, 3);
          e.Graphics.DrawLine(p, e.CellBounds.Left, e.CellBounds.Top - 1, e.CellBounds.Right, e.CellBounds.Top - 1);
        }
      }
    }

    #endregion

    #endregion

    #endregion

    #region Overridden Methods

    /// <summary>
    /// Load the Views
    /// </summary>
    /// <param name="mediaType"></param>
    /// <param name="selections"></param>
    /// <param name="viewsAs"></param>
    /// <param name="sortBy"></param>
    protected void LoadSettings(
      string mediaType,
      string[] selections,
      string[] viewsAs,
      string[] sortBy
      )
    {
      string defaultViews = Path.Combine(ViewHandler.DefaultsDirectory, mediaType + "Views.xml");
      string customViews = Config.GetFile(Config.Dir.Config, mediaType + "ViewsNew.xml");
      Selections = selections;
      ViewsAs = viewsAs;
      SortBy = sortBy;

      if (!File.Exists(customViews))
      {
        File.Copy(defaultViews, customViews);
      }
      else
      {
        // Let's see, if we got a pre 1.2 file
        try
        {
          // Can't use XPath here, as the XML Namespace is dependend on the version of MP Core
          // And this might change.
          // So we iterate through the file, until we found a filterdef.
          XmlDocument xmlDoc = new XmlDocument();
          xmlDoc.Load(customViews);
          XmlElement rootElement = xmlDoc.DocumentElement;
          if (rootElement != null)
          {
            XmlNode body = rootElement.ChildNodes[0];
            foreach (XmlNode node in body.ChildNodes)
            {
              if (node.Name == "a3:FilterDefinition")
              {
                MediaPortal.GUI.Library.Log.Info("Views: Found old view format: {0} Copying default views.",
                                                 customViews);
                File.Copy(defaultViews, customViews, true);
                break;
              }
            }
          }
        }
        catch (Exception)
        {
          MediaPortal.GUI.Library.Log.Error("Views: Exception reading view {0}. Copying default views.", customViews);
          File.Copy(defaultViews, customViews, true);
        }
      }

      views = new List<ViewDefinitionNew>();

      try
      {
        using (FileStream fileStream = new FileInfo(customViews).OpenRead())
        {
          XmlSerializer serializer = new XmlSerializer(typeof(List<ViewDefinitionNew>));
          views = (List<ViewDefinitionNew>)serializer.Deserialize(fileStream);
          fileStream.Close();
        }
      }
      catch (Exception) { }

      SetupGrid();
      LoadTreeView();
    }


    /// <summary>
    /// Save the Views
    /// </summary>
    /// <param name="mediaType"></param>
    protected void SaveSettings(string mediaType)
    {
      StoreGridInView(); // Save pending changes
      if (settingsChanged)
      {
        views.Clear();
        TreeNodeCollection nodes = treeViewMenu.Nodes;
        foreach (TreeNode node in nodes)
        {
          views.Add((ViewDefinitionNew)node.Tag);
        }

        // Now Serialize the View to the XML
        XmlSerializer serializer = new XmlSerializer(typeof(List<ViewDefinitionNew>));

        string customViews = Config.GetFile(Config.Dir.Config, mediaType + "ViewsNew.xml");
        Stream fs = new FileStream(customViews, FileMode.Create);
        XmlWriterSettings writerSettings = new XmlWriterSettings();
        writerSettings.Indent = true;
        writerSettings.Encoding = Encoding.Unicode;
        XmlWriter writer = XmlWriter.Create(fs, writerSettings);

        // Serialize using the XmlTextWriter.
        serializer.Serialize(writer, views);
        writer.Close();
      }
    }

    #endregion
  }
}