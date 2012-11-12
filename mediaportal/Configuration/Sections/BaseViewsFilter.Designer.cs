namespace MediaPortal.Configuration.Sections
{
  partial class BaseViewsFilter
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.dataGridView1 = new System.Windows.Forms.DataGridView();
      this.cbDatabaseTable = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.btCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.dgColField = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgColOperator = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgColSelectionValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dgColAndOr = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.mpGroupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
      this.SuspendLayout();
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Controls.Add(this.dataGridView1);
      this.mpGroupBox1.Controls.Add(this.cbDatabaseTable);
      this.mpGroupBox1.Controls.Add(this.mpLabel1);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.mpGroupBox1.Location = new System.Drawing.Point(20, 26);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(743, 245);
      this.mpGroupBox1.TabIndex = 0;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Filter Definition";
      // 
      // dataGridView1
      // 
      this.dataGridView1.AllowUserToAddRows = false;
      this.dataGridView1.AllowUserToDeleteRows = false;
      this.dataGridView1.AllowUserToResizeColumns = false;
      this.dataGridView1.AllowUserToResizeRows = false;
      this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgColField,
            this.dgColOperator,
            this.dgColSelectionValue,
            this.dgColAndOr});
      this.dataGridView1.Location = new System.Drawing.Point(22, 69);
      this.dataGridView1.Name = "dataGridView1";
      this.dataGridView1.RowHeadersVisible = false;
      this.dataGridView1.Size = new System.Drawing.Size(692, 154);
      this.dataGridView1.TabIndex = 2;
      // 
      // cbDatabaseTable
      // 
      this.cbDatabaseTable.BorderColor = System.Drawing.Color.Empty;
      this.cbDatabaseTable.FormattingEnabled = true;
      this.cbDatabaseTable.Location = new System.Drawing.Point(62, 29);
      this.cbDatabaseTable.Name = "cbDatabaseTable";
      this.cbDatabaseTable.Size = new System.Drawing.Size(160, 21);
      this.cbDatabaseTable.TabIndex = 1;
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(19, 32);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(37, 13);
      this.mpLabel1.TabIndex = 0;
      this.mpLabel1.Text = "Table:";
      // 
      // btSave
      // 
      this.btSave.Location = new System.Drawing.Point(42, 514);
      this.btSave.Name = "btSave";
      this.btSave.Size = new System.Drawing.Size(75, 23);
      this.btSave.TabIndex = 1;
      this.btSave.Text = "Save";
      this.btSave.UseVisualStyleBackColor = true;
      // 
      // btCancel
      // 
      this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btCancel.Location = new System.Drawing.Point(150, 513);
      this.btCancel.Name = "btCancel";
      this.btCancel.Size = new System.Drawing.Size(75, 23);
      this.btCancel.TabIndex = 2;
      this.btCancel.Text = "Cancel";
      this.btCancel.UseVisualStyleBackColor = true;
      // 
      // dgColField
      // 
      this.dgColField.HeaderText = "Field";
      this.dgColField.Name = "dgColField";
      this.dgColField.ReadOnly = true;
      this.dgColField.Width = 150;
      // 
      // dgColOperator
      // 
      this.dgColOperator.HeaderText = "Operator";
      this.dgColOperator.Items.AddRange(new object[] {
            "Equals",
            "Not Equals",
            "Contains",
            "Not Contains",
            "Greater Than",
            "Greater Equals",
            "Less Than",
            "Less Equals",
            "In",
            "Not In",
            "Starts With",
            "Not Starts With",
            "Ends With",
            "Not Ends With"});
      this.dgColOperator.Name = "dgColOperator";
      this.dgColOperator.ReadOnly = true;
      this.dgColOperator.Width = 80;
      // 
      // dgColSelectionValue
      // 
      this.dgColSelectionValue.HeaderText = "Selection Value";
      this.dgColSelectionValue.Name = "dgColSelectionValue";
      this.dgColSelectionValue.Width = 400;
      // 
      // dgColAndOr
      // 
      this.dgColAndOr.HeaderText = "A/O";
      this.dgColAndOr.Items.AddRange(new object[] {
            "",
            "AND",
            "OR"});
      this.dgColAndOr.Name = "dgColAndOr";
      this.dgColAndOr.ReadOnly = true;
      this.dgColAndOr.Width = 50;
      // 
      // BaseViewsFilter
      // 
      this.AcceptButton = this.btSave;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btCancel;
      this.ClientSize = new System.Drawing.Size(784, 562);
      this.ControlBox = false;
      this.Controls.Add(this.btCancel);
      this.Controls.Add(this.btSave);
      this.Controls.Add(this.mpGroupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "BaseViewsFilter";
      this.ShowInTaskbar = false;
      this.mpGroupBox1.ResumeLayout(false);
      this.mpGroupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private UserInterface.Controls.MPGroupBox mpGroupBox1;
    private UserInterface.Controls.MPComboBox cbDatabaseTable;
    private UserInterface.Controls.MPLabel mpLabel1;
    private System.Windows.Forms.DataGridView dataGridView1;
    private UserInterface.Controls.MPButton btSave;
    private UserInterface.Controls.MPButton btCancel;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgColField;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgColOperator;
    private System.Windows.Forms.DataGridViewTextBoxColumn dgColSelectionValue;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgColAndOr;
  }
}
