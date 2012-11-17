namespace MediaPortal.Configuration.Sections
{
  partial class BaseViewsNew
  {
    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      this.groupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.dataGrid = new System.Windows.Forms.DataGridView();
      this.dgSelection = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgViewAs = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgSortBy = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgSortAsc = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.dgSkip = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.dgEditFilter = new System.Windows.Forms.DataGridViewButtonColumn();
      this.lblActionCodes = new MediaPortal.UserInterface.Controls.MPLabel();
      this.treeViewMenu = new System.Windows.Forms.TreeView();
      this.btnEditFilter = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnSetDefaults = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnAddView = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnDeleteView = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox.Controls.Add(this.dataGrid);
      this.groupBox.Controls.Add(this.lblActionCodes);
      this.groupBox.Controls.Add(this.treeViewMenu);
      this.groupBox.Controls.Add(this.btnEditFilter);
      this.groupBox.Controls.Add(this.btnSetDefaults);
      this.groupBox.Controls.Add(this.btnAddView);
      this.groupBox.Controls.Add(this.btnDeleteView);
      this.groupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox.Location = new System.Drawing.Point(6, 0);
      this.groupBox.Name = "groupBox";
      this.groupBox.Size = new System.Drawing.Size(462, 408);
      this.groupBox.TabIndex = 0;
      this.groupBox.TabStop = false;
      // 
      // dataGrid
      // 
      this.dataGrid.AllowDrop = true;
      this.dataGrid.AllowUserToAddRows = false;
      this.dataGrid.AllowUserToDeleteRows = false;
      this.dataGrid.AllowUserToResizeColumns = false;
      this.dataGrid.AllowUserToResizeRows = false;
      this.dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightSteelBlue;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgSelection,
            this.dgViewAs,
            this.dgSortBy,
            this.dgSortAsc,
            this.dgSkip,
            this.dgEditFilter});
      this.dataGrid.Location = new System.Drawing.Point(16, 238);
      this.dataGrid.MultiSelect = false;
      this.dataGrid.Name = "dataGrid";
      this.dataGrid.RowHeadersVisible = false;
      this.dataGrid.Size = new System.Drawing.Size(433, 129);
      this.dataGrid.TabIndex = 13;
      this.dataGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellClick);
      this.dataGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGrid_OnCellPainting);
      this.dataGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGrid_CurrentCellDirtyStateChanged);
      this.dataGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGrid_DataError);
      this.dataGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGrid_OnDragDrop);
      this.dataGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.dataGrid_OnDragOver);
      this.dataGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGrid_KeyDown);
      this.dataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGrid_OnMouseDown);
      this.dataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGrid_OnMouseMove);
      // 
      // dgSelection
      // 
      this.dgSelection.HeaderText = "Selection";
      this.dgSelection.Name = "dgSelection";
      this.dgSelection.ToolTipText = "Select the field that should be retrieved from the database";
      this.dgSelection.Width = 140;
      // 
      // dgViewAs
      // 
      this.dgViewAs.HeaderText = "Layout";
      this.dgViewAs.Name = "dgViewAs";
      this.dgViewAs.ToolTipText = "Select how the returned data should be shown";
      this.dgViewAs.Width = 90;
      // 
      // dgSortBy
      // 
      this.dgSortBy.HeaderText = "SortBy";
      this.dgSortBy.Name = "dgSortBy";
      this.dgSortBy.ToolTipText = "Choose the sort field";
      this.dgSortBy.Width = 90;
      // 
      // dgSortAsc
      // 
      this.dgSortAsc.FalseValue = "false";
      this.dgSortAsc.HeaderText = "Asc";
      this.dgSortAsc.Name = "dgSortAsc";
      this.dgSortAsc.ToolTipText = "Chose sort direction";
      this.dgSortAsc.TrueValue = "true";
      this.dgSortAsc.Width = 30;
      // 
      // dgSkip
      // 
      this.dgSkip.FalseValue = "false";
      this.dgSkip.HeaderText = "Skip";
      this.dgSkip.Name = "dgSkip";
      this.dgSkip.ToolTipText = "Don\'t display this level, if only 1 row is returned";
      this.dgSkip.TrueValue = "true";
      this.dgSkip.Width = 30;
      // 
      // dgEditFilter
      // 
      this.dgEditFilter.HeaderText = "Filter";
      this.dgEditFilter.Name = "dgEditFilter";
      this.dgEditFilter.Text = "Edit";
      this.dgEditFilter.UseColumnTextForButtonValue = true;
      this.dgEditFilter.Width = 50;
      // 
      // lblActionCodes
      // 
      this.lblActionCodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblActionCodes.Location = new System.Drawing.Point(15, 370);
      this.lblActionCodes.Name = "lblActionCodes";
      this.lblActionCodes.Size = new System.Drawing.Size(430, 29);
      this.lblActionCodes.TabIndex = 12;
      this.lblActionCodes.Text = "Use the \"Ins\" and \"Del\" key to insert and delete lines. Drag the rows to change o" +
    "rder";
      this.lblActionCodes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // treeViewMenu
      // 
      this.treeViewMenu.AllowDrop = true;
      this.treeViewMenu.LabelEdit = true;
      this.treeViewMenu.Location = new System.Drawing.Point(16, 20);
      this.treeViewMenu.Name = "treeViewMenu";
      this.treeViewMenu.Size = new System.Drawing.Size(344, 200);
      this.treeViewMenu.TabIndex = 11;
      this.treeViewMenu.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewMenu_AfterLabelEdit);
      this.treeViewMenu.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewMenu_ItemDrag);
      this.treeViewMenu.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewMenu_BeforeSelect);
      this.treeViewMenu.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMenu_AfterSelect);
      this.treeViewMenu.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewMenu_DragDrop);
      this.treeViewMenu.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewMenu_DragEnter);
      // 
      // btnEditFilter
      // 
      this.btnEditFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnEditFilter.Enabled = false;
      this.btnEditFilter.Location = new System.Drawing.Point(366, 102);
      this.btnEditFilter.Name = "btnEditFilter";
      this.btnEditFilter.Size = new System.Drawing.Size(83, 23);
      this.btnEditFilter.TabIndex = 10;
      this.btnEditFilter.Text = "Edit Filter";
      this.btnEditFilter.UseVisualStyleBackColor = true;
      this.btnEditFilter.Click += new System.EventHandler(this.btEditFilter_Click);
      // 
      // btnSetDefaults
      // 
      this.btnSetDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSetDefaults.Location = new System.Drawing.Point(366, 198);
      this.btnSetDefaults.Name = "btnSetDefaults";
      this.btnSetDefaults.Size = new System.Drawing.Size(83, 22);
      this.btnSetDefaults.TabIndex = 9;
      this.btnSetDefaults.Text = "Set defaults";
      this.btnSetDefaults.UseVisualStyleBackColor = true;
      this.btnSetDefaults.Click += new System.EventHandler(this.btnSetDefaults_Click);
      // 
      // btnAddView
      // 
      this.btnAddView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAddView.Location = new System.Drawing.Point(366, 20);
      this.btnAddView.Name = "btnAddView";
      this.btnAddView.Size = new System.Drawing.Size(83, 22);
      this.btnAddView.TabIndex = 5;
      this.btnAddView.Text = "Add View";
      this.btnAddView.UseVisualStyleBackColor = true;
      this.btnAddView.Click += new System.EventHandler(this.btnAdd_Click);
      // 
      // btnDeleteView
      // 
      this.btnDeleteView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDeleteView.Enabled = false;
      this.btnDeleteView.Location = new System.Drawing.Point(366, 59);
      this.btnDeleteView.Name = "btnDeleteView";
      this.btnDeleteView.Size = new System.Drawing.Size(83, 22);
      this.btnDeleteView.TabIndex = 6;
      this.btnDeleteView.Text = "Delete View";
      this.btnDeleteView.UseVisualStyleBackColor = true;
      this.btnDeleteView.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // BaseViewsNew
      // 
      this.Controls.Add(this.groupBox);
      this.Name = "BaseViewsNew";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox;
    private MediaPortal.UserInterface.Controls.MPButton btnDeleteView;
    private MediaPortal.UserInterface.Controls.MPButton btnAddView;
    private UserInterface.Controls.MPButton btnSetDefaults;
    private UserInterface.Controls.MPButton btnEditFilter;
    private System.Windows.Forms.TreeView treeViewMenu;
    private UserInterface.Controls.MPLabel lblActionCodes;
    private System.Windows.Forms.DataGridView dataGrid;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSelection;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgViewAs;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSortBy;
    private System.Windows.Forms.DataGridViewCheckBoxColumn dgSortAsc;
    private System.Windows.Forms.DataGridViewCheckBoxColumn dgSkip;
    private System.Windows.Forms.DataGridViewButtonColumn dgEditFilter;
  }
}
