#region Copyright (C) 2005-2023 Team MediaPortal

// Copyright (C) 2005-2023 Team MediaPortal
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

namespace MediaPortal.DeployTool.Sections
{
  partial class BaseInstallationTypeDlg
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
      this.labelOneClickCaption = new System.Windows.Forms.Label();
      this.labelOneClickDesc = new System.Windows.Forms.Label();
      this.labelAdvancedDesc = new System.Windows.Forms.Label();
      this.labelAdvancedCaption = new System.Windows.Forms.Label();
      this.rbOneClick = new System.Windows.Forms.Label();
      this.rbAdvanced = new System.Windows.Forms.Label();
      this.bOneClick = new System.Windows.Forms.Button();
      this.bAdvanced = new System.Windows.Forms.Button();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Location = new System.Drawing.Point(330, 75);
      this.labelSectionHeader.Size = new System.Drawing.Size(371, 17);
      this.labelSectionHeader.Text = "Please choose which setup you want to install:";
      this.labelSectionHeader.Visible = false;
      // 
      // labelOneClickCaption
      // 
      this.labelOneClickCaption.AutoSize = true;
      this.labelOneClickCaption.BackColor = System.Drawing.Color.Transparent;
      this.labelOneClickCaption.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelOneClickCaption.ForeColor = System.Drawing.Color.White;
      this.labelOneClickCaption.Location = new System.Drawing.Point(330, 116);
      this.labelOneClickCaption.Name = "labelOneClickCaption";
      this.labelOneClickCaption.Size = new System.Drawing.Size(162, 16);
      this.labelOneClickCaption.TabIndex = 1;
      this.labelOneClickCaption.Text = "One Click Installation";
      // 
      // labelOneClickDesc
      // 
      this.labelOneClickDesc.BackColor = System.Drawing.Color.Transparent;
      this.labelOneClickDesc.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelOneClickDesc.ForeColor = System.Drawing.Color.White;
      this.labelOneClickDesc.Location = new System.Drawing.Point(345, 135);
      this.labelOneClickDesc.Name = "labelOneClickDesc";
      this.labelOneClickDesc.Size = new System.Drawing.Size(420, 40);
      this.labelOneClickDesc.TabIndex = 2;
      this.labelOneClickDesc.Text = "All required applications will be installed into their default locations and with" +
    " the default settings. The database password is \"MediaPortal\".";
      // 
      // labelAdvancedDesc
      // 
      this.labelAdvancedDesc.BackColor = System.Drawing.Color.Transparent;
      this.labelAdvancedDesc.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelAdvancedDesc.ForeColor = System.Drawing.Color.White;
      this.labelAdvancedDesc.Location = new System.Drawing.Point(342, 248);
      this.labelAdvancedDesc.Name = "labelAdvancedDesc";
      this.labelAdvancedDesc.Size = new System.Drawing.Size(399, 46);
      this.labelAdvancedDesc.TabIndex = 6;
      this.labelAdvancedDesc.Text = "The advanced installation allows you to install Server/Client setups and to speci" +
    "fy installation locations and other settings";
      // 
      // labelAdvancedCaption
      // 
      this.labelAdvancedCaption.AutoSize = true;
      this.labelAdvancedCaption.BackColor = System.Drawing.Color.Transparent;
      this.labelAdvancedCaption.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelAdvancedCaption.ForeColor = System.Drawing.Color.White;
      this.labelAdvancedCaption.Location = new System.Drawing.Point(330, 230);
      this.labelAdvancedCaption.Name = "labelAdvancedCaption";
      this.labelAdvancedCaption.Size = new System.Drawing.Size(167, 16);
      this.labelAdvancedCaption.TabIndex = 5;
      this.labelAdvancedCaption.Text = "Advanced Installation";
      // 
      // rbOneClick
      // 
      this.rbOneClick.AutoSize = true;
      this.rbOneClick.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbOneClick.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbOneClick.ForeColor = System.Drawing.Color.White;
      this.rbOneClick.Location = new System.Drawing.Point(380, 188);
      this.rbOneClick.Name = "rbOneClick";
      this.rbOneClick.Size = new System.Drawing.Size(153, 13);
      this.rbOneClick.TabIndex = 14;
      this.rbOneClick.Text = "Do a one click installation";
      this.rbOneClick.Click += new System.EventHandler(this.rbOneClick_Click);
      // 
      // rbAdvanced
      // 
      this.rbAdvanced.AutoSize = true;
      this.rbAdvanced.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rbAdvanced.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rbAdvanced.ForeColor = System.Drawing.Color.White;
      this.rbAdvanced.Location = new System.Drawing.Point(380, 300);
      this.rbAdvanced.Name = "rbAdvanced";
      this.rbAdvanced.Size = new System.Drawing.Size(153, 13);
      this.rbAdvanced.TabIndex = 14;
      this.rbAdvanced.Text = "Do a one click installation";
      this.rbAdvanced.Click += new System.EventHandler(this.rbAdvanced_Click);
      // 
      // bOneClick
      // 
      this.bOneClick.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bOneClick.FlatAppearance.BorderSize = 0;
      this.bOneClick.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bOneClick.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bOneClick.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bOneClick.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bOneClick.Location = new System.Drawing.Point(337, 183);
      this.bOneClick.Name = "bOneClick";
      this.bOneClick.Size = new System.Drawing.Size(37, 23);
      this.bOneClick.TabIndex = 15;
      this.bOneClick.UseVisualStyleBackColor = true;
      this.bOneClick.Click += new System.EventHandler(this.bOneClick_Click);
      // 
      // bAdvanced
      // 
      this.bAdvanced.Cursor = System.Windows.Forms.Cursors.Hand;
      this.bAdvanced.FlatAppearance.BorderSize = 0;
      this.bAdvanced.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.bAdvanced.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.bAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.bAdvanced.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.bAdvanced.Location = new System.Drawing.Point(337, 295);
      this.bAdvanced.Name = "bAdvanced";
      this.bAdvanced.Size = new System.Drawing.Size(37, 23);
      this.bAdvanced.TabIndex = 16;
      this.bAdvanced.UseVisualStyleBackColor = true;
      this.bAdvanced.Click += new System.EventHandler(this.bAdvanced_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.Mediaportal_Box_White;
      this.pictureBox1.Location = new System.Drawing.Point(-50, 50);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(374, 357);
      this.pictureBox1.TabIndex = 17;
      this.pictureBox1.TabStop = false;
      // 
      // BaseInstallationTypeDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.bAdvanced);
      this.Controls.Add(this.rbAdvanced);
      this.Controls.Add(this.bOneClick);
      this.Controls.Add(this.rbOneClick);
      this.Controls.Add(this.labelAdvancedDesc);
      this.Controls.Add(this.labelAdvancedCaption);
      this.Controls.Add(this.labelOneClickDesc);
      this.Controls.Add(this.labelOneClickCaption);
      this.Name = "BaseInstallationTypeDlg";
      this.Controls.SetChildIndex(this.labelOneClickCaption, 0);
      this.Controls.SetChildIndex(this.labelOneClickDesc, 0);
      this.Controls.SetChildIndex(this.labelAdvancedCaption, 0);
      this.Controls.SetChildIndex(this.labelAdvancedDesc, 0);
      this.Controls.SetChildIndex(this.rbOneClick, 0);
      this.Controls.SetChildIndex(this.bOneClick, 0);
      this.Controls.SetChildIndex(this.rbAdvanced, 0);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.bAdvanced, 0);
      this.Controls.SetChildIndex(this.pictureBox1, 0);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label labelOneClickCaption;
    private System.Windows.Forms.Label labelOneClickDesc;
    private System.Windows.Forms.Label labelAdvancedDesc;
    private System.Windows.Forms.Label labelAdvancedCaption;
    private System.Windows.Forms.Label rbOneClick;
    private System.Windows.Forms.Label rbAdvanced;
    private System.Windows.Forms.Button bOneClick;
      private System.Windows.Forms.Button bAdvanced;
    private System.Windows.Forms.PictureBox pictureBox1;
  }
}