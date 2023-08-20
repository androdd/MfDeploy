// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.MFNetworkConfigDialog
// Assembly: MFDeploy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 9518305E-2E58-43E8-9F45-41B78F83BC9E
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeploy.exe

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
    private MFWirelessConfiguration m_wirelessCfg;
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
    private GroupBox groupBox1;
    private TextBox textBoxSSID;
    private Label labelSSID;
    private TextBox textBoxNetworkKey;
    private Label labelNetworkKey;
    private TextBox textBoxPassPhrase;
    private Label labelPassPhrase;
    private Label labelRadio;
    private Label labelEncryption;
    private Label labelAuthentication;
    private ComboBox comboBoxEncryption;
    private ComboBox comboBoxAuthentication;
    private CheckBox checkBoxEncryptConfigData;
    private CheckBox checkBox80211n;
    private CheckBox checkBox80211g;
    private CheckBox checkBox80211b;
    private CheckBox checkBox80211a;
    private ComboBox comboBoxNetKey;
    private TextBox textBoxReKeyInternal;
    private Label labelReKeyInternal;

    public MFNetworkConfigDialog(MFDevice device)
    {
      this.m_cfg = new MFNetworkConfiguration(device);
      this.m_cfgHelper = new MFConfigHelper(device);
      this.m_wirelessCfg = new MFWirelessConfiguration(device);
      this.InitializeComponent();
    }

    private string MacToDashedHex(byte[] mac_addr)
    {
      string str = "";
      for (int index = 0; index < mac_addr.Length; ++index)
        str += string.Format("{0:x02}-", (object) mac_addr[index]);
      return str.TrimEnd('-');
    }

    private byte[] DashedHexToMac(string mac, int maxlen)
    {
      string[] strArray = mac.Split(new char[2]{ ':', '-' }, StringSplitOptions.RemoveEmptyEntries);
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
      if (this.m_cfg.ConfigurationType == MFNetworkConfiguration.NetworkConfigType.Wireless)
      {
        this.EnableWireless(true);
        this.m_wirelessCfg.Load();
        this.textBoxPassPhrase.Text = this.m_wirelessCfg.PassPhrase;
        int num1 = (int) Math.Log((double) this.m_wirelessCfg.NetworkKeyLength / 8.0, 2.0);
        this.comboBoxNetKey.SelectedIndex = num1 >= this.comboBoxNetKey.Items.Count || num1 <= 0 ? this.comboBoxNetKey.Items.Count - 1 : num1;
        this.textBoxNetworkKey.Text = this.m_wirelessCfg.NetworkKey;
        this.textBoxReKeyInternal.Text = this.m_wirelessCfg.ReKeyInternal;
        this.textBoxSSID.Text = this.m_wirelessCfg.SSID;
        this.checkBox80211a.Checked = (this.m_wirelessCfg.Radio & 1) != 0;
        this.checkBox80211b.Checked = (this.m_wirelessCfg.Radio & 2) != 0;
        this.checkBox80211g.Checked = (this.m_wirelessCfg.Radio & 4) != 0;
        this.checkBox80211n.Checked = (this.m_wirelessCfg.Radio & 8) != 0;
        try
        {
          this.comboBoxAuthentication.SelectedIndex = this.m_wirelessCfg.Authentication;
          this.comboBoxEncryption.SelectedIndex = this.m_wirelessCfg.Encryption;
        }
        catch (Exception ex)
        {
          int num2 = (int) MessageBox.Show((IWin32Window) this, ex.Message, Resources.NetworkTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        this.checkBoxEncryptConfigData.Checked = this.m_wirelessCfg.UseEncryption;
      }
      else
        this.EnableWireless(false);
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

    private bool ValidateWirelessData()
    {
      object obj = (object) null;
      try
      {
        this.m_wirelessCfg.Authentication = this.comboBoxAuthentication.SelectedIndex;
        this.m_wirelessCfg.Encryption = this.comboBoxEncryption.SelectedIndex;
        this.m_wirelessCfg.Radio = (this.checkBox80211a.Checked ? 1 : 0) | (this.checkBox80211b.Checked ? 2 : 0) | (this.checkBox80211g.Checked ? 4 : 0) | (this.checkBox80211n.Checked ? 8 : 0);
        this.m_wirelessCfg.UseEncryption = this.checkBoxEncryptConfigData.Checked;
        obj = (object) this.textBoxPassPhrase;
        this.m_wirelessCfg.PassPhrase = this.textBoxPassPhrase.Text;
        this.m_wirelessCfg.NetworkKeyLength = (int) (Math.Pow(2.0, (double) this.comboBoxNetKey.SelectedIndex) * 8.0);
        obj = (object) this.textBoxNetworkKey;
        this.m_wirelessCfg.NetworkKey = this.textBoxNetworkKey.Text;
        this.m_wirelessCfg.ReKeyLength = this.textBoxReKeyInternal.Text.Length / 2;
        obj = (object) this.textBoxReKeyInternal;
        this.m_wirelessCfg.ReKeyInternal = this.textBoxReKeyInternal.Text;
        obj = (object) this.textBoxSSID;
        this.m_wirelessCfg.SSID = this.textBoxSSID.Text;
        return true;
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, ex.Message, Resources.TitleErrorInput, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        if (obj != null)
        {
          if (obj is TextBox)
          {
            ((Control) obj).Focus();
            ((TextBoxBase) obj).Select(0, ((Control) obj).Text.Length);
          }
        }
      }
      return false;
    }

    private void buttonUpdate_Click(object sender, EventArgs e)
    {
      Cursor current = Cursor.Current;
      Cursor.Current = Cursors.WaitCursor;
      Exception exception = (Exception) null;
      try
      {
        if (!this.SaveData())
          return;
        this.m_cfgHelper.MaintainConnection = false;
        if (this.m_cfg.ConfigurationType == MFNetworkConfiguration.NetworkConfigType.Wireless)
        {
          if (!this.ValidateWirelessData())
            return;
          this.m_wirelessCfg.Save();
          this.m_cfg.SwapConfigBuffer(this.m_wirelessCfg.m_cfgHelper);
        }
        this.m_cfg.Save();
        this.DialogResult = DialogResult.OK;
      }
      catch (Exception ex)
      {
        exception = ex;
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

    private void checkBoxDHCPEnable_CheckedChanged(object sender, EventArgs e)
    {
      bool flag = this.checkBoxDHCPEnable.Checked;
      this.textBoxDefaultGateway.Enabled = !flag;
      this.textBoxIPAddress.Enabled = !flag;
      this.textBoxSubnetMask.Enabled = !flag;
    }

    private void EnableWireless(bool enable)
    {
      this.comboBoxAuthentication.Enabled = enable;
      this.comboBoxEncryption.Enabled = enable;
      this.checkBox80211a.Enabled = enable;
      this.checkBox80211b.Enabled = enable;
      this.checkBox80211g.Enabled = enable;
      this.checkBox80211n.Enabled = enable;
      this.textBoxPassPhrase.Enabled = enable;
      this.textBoxNetworkKey.Enabled = enable;
      this.textBoxReKeyInternal.Enabled = enable;
      this.textBoxSSID.Enabled = enable;
      this.labelAuthentication.Enabled = enable;
      this.labelEncryption.Enabled = enable;
      this.labelRadio.Enabled = enable;
      this.labelPassPhrase.Enabled = enable;
      this.labelNetworkKey.Enabled = enable;
      this.labelReKeyInternal.Enabled = enable;
      this.labelSSID.Enabled = enable;
      this.checkBoxEncryptConfigData.Enabled = enable;
      this.comboBoxNetKey.Enabled = enable;
      this.comboBoxAuthentication.SelectedIndex = 0;
      this.comboBoxEncryption.SelectedIndex = 0;
      this.comboBoxNetKey.SelectedIndex = 0;
      this.checkBox80211a.Checked = false;
      this.checkBox80211b.Checked = false;
      this.checkBox80211g.Checked = false;
      this.checkBox80211n.Checked = false;
      if (enable)
        this.m_cfg.ConfigurationType = MFNetworkConfiguration.NetworkConfigType.Wireless;
      else
        this.m_cfg.ConfigurationType = MFNetworkConfiguration.NetworkConfigType.Generic;
    }

    private void comboBoxNetKey_SelectionChangeCommitted(object sender, EventArgs e)
    {
      switch (this.comboBoxNetKey.SelectedIndex)
      {
        case 0:
          this.textBoxNetworkKey.MaxLength = 16;
          break;
        case 1:
          this.textBoxNetworkKey.MaxLength = 32;
          break;
        case 2:
          this.textBoxNetworkKey.MaxLength = 64;
          break;
        case 3:
          this.textBoxNetworkKey.MaxLength = 128;
          break;
        case 4:
          this.textBoxNetworkKey.MaxLength = 256;
          break;
        case 5:
          this.textBoxNetworkKey.MaxLength = 512;
          break;
      }
      if (this.textBoxNetworkKey.Text.Length <= this.textBoxNetworkKey.MaxLength)
        return;
      this.textBoxNetworkKey.Text = this.textBoxNetworkKey.Text.Substring(0, this.textBoxNetworkKey.MaxLength);
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
      this.groupBox1 = new GroupBox();
      this.comboBoxNetKey = new ComboBox();
      this.checkBox80211n = new CheckBox();
      this.checkBox80211g = new CheckBox();
      this.checkBox80211b = new CheckBox();
      this.checkBox80211a = new CheckBox();
      this.checkBoxEncryptConfigData = new CheckBox();
      this.comboBoxEncryption = new ComboBox();
      this.comboBoxAuthentication = new ComboBox();
      this.textBoxSSID = new TextBox();
      this.labelSSID = new Label();
      this.textBoxNetworkKey = new TextBox();
      this.labelNetworkKey = new Label();
      this.textBoxPassPhrase = new TextBox();
      this.labelPassPhrase = new Label();
      this.labelRadio = new Label();
      this.labelEncryption = new Label();
      this.labelAuthentication = new Label();
      this.textBoxReKeyInternal = new TextBox();
      this.labelReKeyInternal = new Label();
      this.groupBox1.SuspendLayout();
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
      this.textBoxIPAddress.Size = new Size(241, 20);
      this.textBoxIPAddress.TabIndex = 1;
      this.textBoxSubnetMask.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxSubnetMask.Location = new Point(142, 32);
      this.textBoxSubnetMask.Name = "textBoxSubnetMask";
      this.textBoxSubnetMask.Size = new Size(241, 20);
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
      this.textBoxDefaultGateway.Size = new Size(241, 20);
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
      this.textBoxMACAddress.Size = new Size(241, 20);
      this.textBoxMACAddress.TabIndex = 7;
      this.label7.AutoSize = true;
      this.label7.Location = new Point(12, 87);
      this.label7.Name = "label7";
      this.label7.Size = new Size(74, 13);
      this.label7.TabIndex = 6;
      this.label7.Text = "&MAC Address:";
      this.buttonUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.buttonUpdate.Location = new Point(226, 484);
      this.buttonUpdate.Name = "buttonUpdate";
      this.buttonUpdate.Size = new Size(75, 23);
      this.buttonUpdate.TabIndex = 14;
      this.buttonUpdate.Text = Resources.ButtonUpdate;
      this.buttonUpdate.UseVisualStyleBackColor = true;
      this.buttonUpdate.Click += new EventHandler(this.buttonUpdate_Click);
      this.buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.buttonCancel.DialogResult = DialogResult.Cancel;
      this.buttonCancel.Location = new Point(307, 484);
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
      this.checkBoxDHCPEnable.CheckedChanged += new EventHandler(this.checkBoxDHCPEnable_CheckedChanged);
      this.textBoxDnsPrimary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxDnsPrimary.Location = new Point(142, 110);
      this.textBoxDnsPrimary.Name = "textBoxDnsPrimary";
      this.textBoxDnsPrimary.Size = new Size(241, 20);
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
      this.textBoxDnsSecondary.Size = new Size(241, 20);
      this.textBoxDnsSecondary.TabIndex = 11;
      this.label5.AutoSize = true;
      this.label5.Location = new Point(12, 139);
      this.label5.Name = "label5";
      this.label5.Size = new Size(128, 13);
      this.label5.TabIndex = 10;
      this.label5.Text = "D&NS Secondary Address:";
      this.groupBox1.Controls.Add((Control) this.comboBoxNetKey);
      this.groupBox1.Controls.Add((Control) this.checkBox80211n);
      this.groupBox1.Controls.Add((Control) this.checkBox80211g);
      this.groupBox1.Controls.Add((Control) this.checkBox80211b);
      this.groupBox1.Controls.Add((Control) this.checkBox80211a);
      this.groupBox1.Controls.Add((Control) this.checkBoxEncryptConfigData);
      this.groupBox1.Controls.Add((Control) this.comboBoxEncryption);
      this.groupBox1.Controls.Add((Control) this.comboBoxAuthentication);
      this.groupBox1.Controls.Add((Control) this.textBoxSSID);
      this.groupBox1.Controls.Add((Control) this.labelSSID);
      this.groupBox1.Controls.Add((Control) this.textBoxReKeyInternal);
      this.groupBox1.Controls.Add((Control) this.labelReKeyInternal);
      this.groupBox1.Controls.Add((Control) this.textBoxNetworkKey);
      this.groupBox1.Controls.Add((Control) this.labelNetworkKey);
      this.groupBox1.Controls.Add((Control) this.textBoxPassPhrase);
      this.groupBox1.Controls.Add((Control) this.labelPassPhrase);
      this.groupBox1.Controls.Add((Control) this.labelRadio);
      this.groupBox1.Controls.Add((Control) this.labelEncryption);
      this.groupBox1.Controls.Add((Control) this.labelAuthentication);
      this.groupBox1.Location = new Point(15, 194);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(367, 284);
      this.groupBox1.TabIndex = 16;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Wireless Configuration";
      this.comboBoxNetKey.DropDownStyle = ComboBoxStyle.DropDownList;
      this.comboBoxNetKey.Enabled = false;
      this.comboBoxNetKey.FormattingEnabled = true;
      this.comboBoxNetKey.Items.AddRange(new object[6]
      {
        (object) "64-bit",
        (object) "128-bit",
        (object) "256-bit",
        (object) "512-bit",
        (object) "1024-bit",
        (object) "2048-bit"
      });
      this.comboBoxNetKey.Location = new Point((int) sbyte.MaxValue, 171);
      this.comboBoxNetKey.Name = "comboBoxNetKey";
      this.comboBoxNetKey.Size = new Size(220, 21);
      this.comboBoxNetKey.TabIndex = 26;
      this.comboBoxNetKey.SelectionChangeCommitted += new EventHandler(this.comboBoxNetKey_SelectionChangeCommitted);
      this.checkBox80211n.AutoSize = true;
      this.checkBox80211n.Location = new Point(239, 99);
      this.checkBox80211n.Name = "checkBox80211n";
      this.checkBox80211n.Size = new Size(65, 17);
      this.checkBox80211n.TabIndex = 25;
      this.checkBox80211n.Text = "802.11n";
      this.checkBox80211n.UseVisualStyleBackColor = true;
      this.checkBox80211g.AutoSize = true;
      this.checkBox80211g.Location = new Point((int) sbyte.MaxValue, 99);
      this.checkBox80211g.Name = "checkBox80211g";
      this.checkBox80211g.Size = new Size(65, 17);
      this.checkBox80211g.TabIndex = 24;
      this.checkBox80211g.Text = "802.11g";
      this.checkBox80211g.UseVisualStyleBackColor = true;
      this.checkBox80211b.AutoSize = true;
      this.checkBox80211b.Location = new Point(239, 76);
      this.checkBox80211b.Name = "checkBox80211b";
      this.checkBox80211b.Size = new Size(65, 17);
      this.checkBox80211b.TabIndex = 23;
      this.checkBox80211b.Text = "802.11b";
      this.checkBox80211b.UseVisualStyleBackColor = true;
      this.checkBox80211a.AutoSize = true;
      this.checkBox80211a.Location = new Point((int) sbyte.MaxValue, 76);
      this.checkBox80211a.Name = "checkBox80211a";
      this.checkBox80211a.Size = new Size(65, 17);
      this.checkBox80211a.TabIndex = 22;
      this.checkBox80211a.Text = "802.11a";
      this.checkBox80211a.UseVisualStyleBackColor = true;
      this.checkBoxEncryptConfigData.CheckAlign = ContentAlignment.MiddleRight;
      this.checkBoxEncryptConfigData.Location = new Point(5, 115);
      this.checkBoxEncryptConfigData.Name = "checkBoxEncryptConfigData";
      this.checkBoxEncryptConfigData.Size = new Size(136, 24);
      this.checkBoxEncryptConfigData.TabIndex = 21;
      this.checkBoxEncryptConfigData.Text = "Encrypt Config Data";
      this.checkBoxEncryptConfigData.UseVisualStyleBackColor = true;
      this.comboBoxEncryption.DropDownStyle = ComboBoxStyle.DropDownList;
      this.comboBoxEncryption.Enabled = false;
      this.comboBoxEncryption.FormattingEnabled = true;
      this.comboBoxEncryption.Items.AddRange(new object[5]
      {
        (object) "None",
        (object) "WEP",
        (object) "WPA",
        (object) "WPAPSK",
        (object) "Certificate"
      });
      this.comboBoxEncryption.Location = new Point((int) sbyte.MaxValue, 46);
      this.comboBoxEncryption.Name = "comboBoxEncryption";
      this.comboBoxEncryption.Size = new Size(220, 21);
      this.comboBoxEncryption.TabIndex = 19;
      this.comboBoxAuthentication.DropDownStyle = ComboBoxStyle.DropDownList;
      this.comboBoxAuthentication.Enabled = false;
      this.comboBoxAuthentication.FormattingEnabled = true;
      this.comboBoxAuthentication.Items.AddRange(new object[6]
      {
        (object) "None",
        (object) "EAP",
        (object) "PEAP",
        (object) "WCN",
        (object) "Open",
        (object) "Shared"
      });
      this.comboBoxAuthentication.Location = new Point((int) sbyte.MaxValue, 19);
      this.comboBoxAuthentication.Name = "comboBoxAuthentication";
      this.comboBoxAuthentication.Size = new Size(220, 21);
      this.comboBoxAuthentication.TabIndex = 18;
      this.textBoxSSID.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxSSID.Enabled = false;
      this.textBoxSSID.Location = new Point((int) sbyte.MaxValue, 250);
      this.textBoxSSID.MaxLength = 31;
      this.textBoxSSID.Name = "textBoxSSID";
      this.textBoxSSID.Size = new Size(220, 20);
      this.textBoxSSID.TabIndex = 17;
      this.labelSSID.AutoSize = true;
      this.labelSSID.Enabled = false;
      this.labelSSID.Location = new Point(7, 253);
      this.labelSSID.Name = "labelSSID";
      this.labelSSID.Size = new Size(32, 13);
      this.labelSSID.TabIndex = 16;
      this.labelSSID.Text = "SSID";
      this.textBoxNetworkKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxNetworkKey.Enabled = false;
      this.textBoxNetworkKey.Location = new Point((int) sbyte.MaxValue, 198);
      this.textBoxNetworkKey.MaxLength = 16;
      this.textBoxNetworkKey.Name = "textBoxNetworkKey";
      this.textBoxNetworkKey.Size = new Size(220, 20);
      this.textBoxNetworkKey.TabIndex = 13;
      this.labelNetworkKey.AutoSize = true;
      this.labelNetworkKey.Enabled = false;
      this.labelNetworkKey.Location = new Point(6, 176);
      this.labelNetworkKey.Name = "labelNetworkKey";
      this.labelNetworkKey.Size = new Size(68, 13);
      this.labelNetworkKey.TabIndex = 12;
      this.labelNetworkKey.Text = "Network Key";
      this.textBoxPassPhrase.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxPassPhrase.Enabled = false;
      this.textBoxPassPhrase.Location = new Point((int) sbyte.MaxValue, 145);
      this.textBoxPassPhrase.MaxLength = 63;
      this.textBoxPassPhrase.Name = "textBoxPassPhrase";
      this.textBoxPassPhrase.Size = new Size(220, 20);
      this.textBoxPassPhrase.TabIndex = 11;
      this.labelPassPhrase.AutoSize = true;
      this.labelPassPhrase.Enabled = false;
      this.labelPassPhrase.Location = new Point(6, 148);
      this.labelPassPhrase.Name = "labelPassPhrase";
      this.labelPassPhrase.Size = new Size(65, 13);
      this.labelPassPhrase.TabIndex = 10;
      this.labelPassPhrase.Text = "Pass phrase";
      this.labelRadio.AutoSize = true;
      this.labelRadio.Enabled = false;
      this.labelRadio.Location = new Point(6, 76);
      this.labelRadio.Name = "labelRadio";
      this.labelRadio.Size = new Size(35, 13);
      this.labelRadio.TabIndex = 3;
      this.labelRadio.Text = "Radio";
      this.labelEncryption.AutoSize = true;
      this.labelEncryption.Enabled = false;
      this.labelEncryption.Location = new Point(6, 49);
      this.labelEncryption.Name = "labelEncryption";
      this.labelEncryption.Size = new Size(57, 13);
      this.labelEncryption.TabIndex = 2;
      this.labelEncryption.Text = "Encryption";
      this.labelAuthentication.AutoSize = true;
      this.labelAuthentication.Enabled = false;
      this.labelAuthentication.Location = new Point(6, 22);
      this.labelAuthentication.Name = "labelAuthentication";
      this.labelAuthentication.Size = new Size(75, 13);
      this.labelAuthentication.TabIndex = 1;
      this.labelAuthentication.Text = "Authentication";
      this.textBoxReKeyInternal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.textBoxReKeyInternal.Enabled = false;
      this.textBoxReKeyInternal.Location = new Point((int) sbyte.MaxValue, 224);
      this.textBoxReKeyInternal.MaxLength = 64;
      this.textBoxReKeyInternal.Name = "textBoxReKeyInternal";
      this.textBoxReKeyInternal.Size = new Size(220, 20);
      this.textBoxReKeyInternal.TabIndex = 15;
      this.labelReKeyInternal.AutoSize = true;
      this.labelReKeyInternal.Enabled = false;
      this.labelReKeyInternal.Location = new Point(6, 227);
      this.labelReKeyInternal.Name = "labelReKeyInternal";
      this.labelReKeyInternal.Size = new Size(77, 13);
      this.labelReKeyInternal.TabIndex = 14;
      this.labelReKeyInternal.Text = "ReKey Internal";
      this.AcceptButton = (IButtonControl) this.buttonUpdate;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.buttonCancel;
      this.ClientSize = new Size(395, 519);
      this.Controls.Add((Control) this.groupBox1);
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
      this.Load += new EventHandler(this.ConfigDialog_Load);
      this.FormClosing += new FormClosingEventHandler(this.MFNetworkConfigDialog_FormClosing);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
