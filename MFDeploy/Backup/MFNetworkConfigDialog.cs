// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.MFNetworkConfigDialog
// Assembly: MFDeploy, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 2BA2704E-D6D9-4FE0-ACF3-88D9CCA0E272
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  internal class MFNetworkConfigDialog : Form
  {
    private MFNetworkConfiguration m_cfg;
    private MFConfigHelper m_cfgHelper;
    private IContainer components;
    private Label label1;
    private TextBox textBoxIPAddress;
    private TextBox textBoxSubnetMask;
    private Label label2;
    private TextBox textBoxDefaultGateway;
    private Label label3;
    private TextBox textBoxMACAddress;
    private Label label7;
    private Button buttonUpdate;
    private Button buttonCancel;
    private Label label10;
    private CheckBox checkBoxDHCPEnable;
    private TextBox textBoxDnsPrimary;
    private Label label4;
    private TextBox textBoxDnsSecondary;
    private Label label5;

    public MFNetworkConfigDialog(MFDevice device)
    {
      this.m_cfg = new MFNetworkConfiguration(device);
      this.m_cfgHelper = new MFConfigHelper(device);
      this.InitializeComponent();
    }

    private string MacToDashedHex(byte[] mac_addr)
    {
      string str = "";
      for (int index = 0; index < mac_addr.Length; ++index)
        str += string.Format("{0:x02}:", (object) mac_addr[index]);
      return str.TrimEnd(':');
    }

    private byte[] DashedHexToMac(string mac, int maxlen)
    {
      string[] strArray = mac.Split(new char[1]{ ':' }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray.Length > maxlen)
        throw new MFInvalidMacAddressException();
      byte[] mac1 = new byte[strArray.Length];
      try
      {
        for (int index = 0; index < strArray.Length; ++index)
          mac1[index] = byte.Parse(strArray[index], NumberStyles.HexNumber);
      }
      catch
      {
        throw new MFInvalidMacAddressException();
      }
      return mac1;
    }

    private void ConfigDialog_Load(object sender, EventArgs e)
    {
      this.m_cfg.Load();
      this.m_cfgHelper.MaintainConnection = true;
      this.textBoxIPAddress.Text = this.m_cfg.IpAddress.ToString();
      this.textBoxSubnetMask.Text = this.m_cfg.SubNetMask.ToString();
      this.textBoxDefaultGateway.Text = this.m_cfg.Gateway.ToString();
      this.checkBoxDHCPEnable.Checked = this.m_cfg.EnableDhcp;
      this.textBoxDnsPrimary.Text = this.m_cfg.PrimaryDns.ToString();
      this.textBoxDnsSecondary.Text = this.m_cfg.SecondaryDns.ToString();
      this.textBoxMACAddress.Text = this.MacToDashedHex(this.m_cfg.MacAddress);
    }

    private bool CheckDecimalToIp(string name, TextBox tb, out IPAddress ip)
    {
      ip = (IPAddress) null;
      if (IPAddress.TryParse(tb.Text, out ip))
        return true;
      int num = (int) MessageBox.Show((IWin32Window) this, string.Format(Resources.ErrorInvalidX, (object) name), Resources.TitleErrorInput, MessageBoxButtons.OK, MessageBoxIcon.Hand);
      tb.Focus();
      tb.SelectAll();
      return false;
    }

    private bool SaveData()
    {
      IPAddress ip;
      if (!this.CheckDecimalToIp(Resources.IpAddress, this.textBoxIPAddress, out ip))
        return false;
      this.m_cfg.IpAddress = ip;
      if (!this.CheckDecimalToIp(Resources.SubnetMask, this.textBoxSubnetMask, out ip))
        return false;
      this.m_cfg.SubNetMask = ip;
      if (!this.CheckDecimalToIp(Resources.DefaultGateway, this.textBoxDefaultGateway, out ip))
        return false;
      this.m_cfg.Gateway = ip;
      if (!this.CheckDecimalToIp(Resources.PrimaryDnsAddress, this.textBoxDnsPrimary, out ip))
        return false;
      this.m_cfg.PrimaryDns = ip;
      if (!this.CheckDecimalToIp(Resources.SecondaryDnsAddress, this.textBoxDnsSecondary, out ip))
        return false;
      this.m_cfg.SecondaryDns = ip;
      try
      {
        this.m_cfg.MacAddress = this.DashedHexToMac(this.textBoxMACAddress.Text, this.m_cfg.MaxMacAddressLength);
      }
      catch (FormatException ex)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, string.Format(Resources.ErrorInvalidX, (object) Resources.MacAddress), Resources.TitleErrorInput, MessageBoxButtons.OK, MessageBoxIcon.Hand);
        this.textBoxMACAddress.Focus();
        this.textBoxMACAddress.SelectAll();
        return false;
      }
      this.m_cfg.EnableDhcp = this.checkBoxDHCPEnable.Checked;
      return true;
    }

    private void buttonUpdate_Click(object sender, EventArgs e)
    {
      if (!this.SaveData())
        return;
      Cursor current = Cursor.Current;
      Cursor.Current = Cursors.WaitCursor;
      Exception exception = (Exception) null;
      this.m_cfgHelper.MaintainConnection = false;
      try
      {
        this.m_cfg.Save();
        this.DialogResult = DialogResult.OK;
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
      int num = (int) MessageBox.Show((IWin32Window) this, exception.Message, Resources.NetworkTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

    private void buttonCancel_Click(object sender, EventArgs e) => this.m_cfgHelper.MaintainConnection = false;

    private void MFNetworkConfigDialog_FormClosing(object sender, FormClosingEventArgs e)
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
      this.textBoxIPAddress = new TextBox();
      this.textBoxSubnetMask = new TextBox();
      this.label2 = new Label();
      this.textBoxDefaultGateway = new TextBox();
      this.label3 = new Label();
      this.textBoxMACAddress = new TextBox();
      this.label7 = new Label();
      this.buttonUpdate = new Button();
      this.buttonCancel = new Button();
      this.label10 = new Label();
      this.checkBoxDHCPEnable = new CheckBox();
      this.textBoxDnsPrimary = new TextBox();
      this.label4 = new Label();
      this.textBoxDnsSecondary = new TextBox();
      this.label5 = new Label();
      this.SuspendLayout();
      this.label1.AutoSize = true;
      this.label1.Location = new Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new Size(90, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Static &IP address:";
      this.textBoxIPAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxIPAddress.Location = new Point(142, 6);
      this.textBoxIPAddress.Name = "textBoxIPAddress";
      this.textBoxIPAddress.Size = new Size(234, 20);
      this.textBoxIPAddress.TabIndex = 1;
      this.textBoxSubnetMask.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxSubnetMask.Location = new Point(142, 32);
      this.textBoxSubnetMask.Name = "textBoxSubnetMask";
      this.textBoxSubnetMask.Size = new Size(234, 20);
      this.textBoxSubnetMask.TabIndex = 3;
      this.label2.AutoSize = true;
      this.label2.Location = new Point(12, 35);
      this.label2.Name = "label2";
      this.label2.Size = new Size(73, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "&Subnet Mask:";
      this.textBoxDefaultGateway.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxDefaultGateway.Location = new Point(142, 58);
      this.textBoxDefaultGateway.Name = "textBoxDefaultGateway";
      this.textBoxDefaultGateway.Size = new Size(234, 20);
      this.textBoxDefaultGateway.TabIndex = 5;
      this.label3.AutoSize = true;
      this.label3.Location = new Point(12, 61);
      this.label3.Name = "label3";
      this.label3.Size = new Size(89, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Default &Gateway:";
      this.textBoxMACAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxMACAddress.Location = new Point(142, 84);
      this.textBoxMACAddress.Name = "textBoxMACAddress";
      this.textBoxMACAddress.Size = new Size(234, 20);
      this.textBoxMACAddress.TabIndex = 7;
      this.label7.AutoSize = true;
      this.label7.Location = new Point(12, 87);
      this.label7.Name = "label7";
      this.label7.Size = new Size(74, 13);
      this.label7.TabIndex = 6;
      this.label7.Text = "&MAC Address:";
      this.buttonUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.buttonUpdate.Location = new Point(220, 196);
      this.buttonUpdate.Name = "buttonUpdate";
      this.buttonUpdate.Size = new Size(75, 23);
      this.buttonUpdate.TabIndex = 14;
      this.buttonUpdate.Text = Resources.ButtonUpdate;
      this.buttonUpdate.UseVisualStyleBackColor = true;
      this.buttonUpdate.Click += new EventHandler(this.buttonUpdate_Click);
      this.buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.buttonCancel.DialogResult = DialogResult.Cancel;
      this.buttonCancel.Location = new Point(301, 196);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new Size(75, 23);
      this.buttonCancel.TabIndex = 15;
      this.buttonCancel.Text = Resources.ButtonCancel;
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
      this.label10.AutoSize = true;
      this.label10.Location = new Point(12, 165);
      this.label10.Name = "label10";
      this.label10.Size = new Size(40, 13);
      this.label10.TabIndex = 12;
      this.label10.Text = "D&HCP:";
      this.checkBoxDHCPEnable.AutoSize = true;
      this.checkBoxDHCPEnable.Location = new Point(142, 165);
      this.checkBoxDHCPEnable.Name = "checkBoxDHCPEnable";
      this.checkBoxDHCPEnable.Size = new Size(59, 17);
      this.checkBoxDHCPEnable.TabIndex = 13;
      this.checkBoxDHCPEnable.Text = Resources.CheckBoxEnable;
      this.checkBoxDHCPEnable.UseVisualStyleBackColor = true;
      this.textBoxDnsPrimary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxDnsPrimary.Location = new Point(142, 110);
      this.textBoxDnsPrimary.Name = "textBoxDnsPrimary";
      this.textBoxDnsPrimary.Size = new Size(234, 20);
      this.textBoxDnsPrimary.TabIndex = 9;
      this.label4.AutoSize = true;
      this.label4.Location = new Point(12, 113);
      this.label4.Name = "label4";
      this.label4.Size = new Size(111, 13);
      this.label4.TabIndex = 8;
      this.label4.Text = "&DNS Primary Address:";
      this.textBoxDnsSecondary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxDnsSecondary.Location = new Point(142, 136);
      this.textBoxDnsSecondary.Name = "textBoxDnsSecondary";
      this.textBoxDnsSecondary.Size = new Size(234, 20);
      this.textBoxDnsSecondary.TabIndex = 11;
      this.label5.AutoSize = true;
      this.label5.Location = new Point(12, 139);
      this.label5.Name = "label5";
      this.label5.Size = new Size(128, 13);
      this.label5.TabIndex = 10;
      this.label5.Text = "D&NS Secondary Address:";
      this.AcceptButton = (IButtonControl) this.buttonUpdate;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.buttonCancel;
      this.ClientSize = new Size(388, 231);
      this.Controls.Add((Control) this.textBoxDnsSecondary);
      this.Controls.Add((Control) this.label5);
      this.Controls.Add((Control) this.textBoxDnsPrimary);
      this.Controls.Add((Control) this.label4);
      this.Controls.Add((Control) this.checkBoxDHCPEnable);
      this.Controls.Add((Control) this.label10);
      this.Controls.Add((Control) this.buttonCancel);
      this.Controls.Add((Control) this.buttonUpdate);
      this.Controls.Add((Control) this.textBoxMACAddress);
      this.Controls.Add((Control) this.label7);
      this.Controls.Add((Control) this.textBoxDefaultGateway);
      this.Controls.Add((Control) this.label3);
      this.Controls.Add((Control) this.textBoxSubnetMask);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.textBoxIPAddress);
      this.Controls.Add((Control) this.label1);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (MFNetworkConfigDialog);
      this.ShowInTaskbar = false;
      this.Text = "Network Configuration";
      this.FormClosing += new FormClosingEventHandler(this.MFNetworkConfigDialog_FormClosing);
      this.Load += new EventHandler(this.ConfigDialog_Load);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
