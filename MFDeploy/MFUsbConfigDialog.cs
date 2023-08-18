// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.MFUsbConfigDialog
// Assembly: MFDeploy, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 2BA2704E-D6D9-4FE0-ACF3-88D9CCA0E272
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  internal class MFUsbConfigDialog : Form
  {
    private const string c_name = "USB_NAME_CONFIG";
    private MFConfigHelper m_cfgHelper;
    private MFUsbConfiguration m_cfg;
    private IContainer components;
    private Label label1;
    private TextBox textBoxName;
    private Button button1;
    private Button button2;

    public MFUsbConfigDialog(MFDevice device)
    {
      this.m_cfgHelper = new MFConfigHelper(device);
      this.m_cfg = new MFUsbConfiguration(device);
      this.InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      Exception exception = (Exception) null;
      Cursor current = Cursor.Current;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        this.m_cfg.Name = this.textBoxName.Text;
        this.m_cfgHelper.MaintainConnection = false;
        this.m_cfg.Save();
      }
      catch (MFInvalidConfigurationDataException ex)
      {
        exception = (Exception) ex;
      }
      catch (MFConfigSectorEraseFailureException ex)
      {
        exception = (Exception) ex;
      }
      catch (MFConfigSectorWriteFailureException ex)
      {
        exception = (Exception) ex;
      }
      catch (MFConfigurationSectorOutOfMemoryException ex)
      {
        exception = (Exception) ex;
      }
      finally
      {
        Cursor.Current = current;
      }
      if (exception == null)
        return;
      int num = (int) MessageBox.Show((IWin32Window) this, exception.Message, Resources.UsbTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

    private void UsbConfigDialog_Load(object sender, EventArgs e)
    {
      this.m_cfg.Load();
      this.m_cfgHelper.MaintainConnection = true;
      this.textBoxName.Text = this.m_cfg.Name;
    }

    private void button2_Click(object sender, EventArgs e) => this.m_cfgHelper.MaintainConnection = false;

    private void MFUsbConfigDialog_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (this.m_cfgHelper == null)
        return;
      this.m_cfgHelper.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.label1 = new Label();
      this.textBoxName = new TextBox();
      this.button1 = new Button();
      this.button2 = new Button();
      this.SuspendLayout();
      this.label1.AutoSize = true;
      this.label1.Location = new Point(12, 27);
      this.label1.Name = "label1";
      this.label1.Size = new Size(102, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "USB Friendly Name:";
      this.textBoxName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxName.Location = new Point(120, 24);
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new Size(206, 20);
      this.textBoxName.TabIndex = 1;
      this.button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.button1.DialogResult = DialogResult.OK;
      this.button1.Location = new Point(170, 63);
      this.button1.Name = "button1";
      this.button1.Size = new Size(75, 23);
      this.button1.TabIndex = 2;
      this.button1.Text = Resources.ButtonApply;
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.button2.DialogResult = DialogResult.Cancel;
      this.button2.Location = new Point(251, 63);
      this.button2.Name = "button2";
      this.button2.Size = new Size(75, 23);
      this.button2.TabIndex = 3;
      this.button2.Text = Resources.ButtonCancel;
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new EventHandler(this.button2_Click);
      this.AcceptButton = (IButtonControl) this.button1;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.button2;
      this.ClientSize = new Size(344, 98);
      this.ControlBox = false;
      this.Controls.Add((Control) this.button2);
      this.Controls.Add((Control) this.button1);
      this.Controls.Add((Control) this.textBoxName);
      this.Controls.Add((Control) this.label1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (MFUsbConfigDialog);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = SizeGripStyle.Hide;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "Network Configuration";
      this.FormClosing += new FormClosingEventHandler(this.MFUsbConfigDialog_FormClosing);
      this.Load += new EventHandler(this.UsbConfigDialog_Load);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
