// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.MFAppDeployConfigDialog
// Assembly: MFDeploy, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 2BA2704E-D6D9-4FE0-ACF3-88D9CCA0E272
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  public class MFAppDeployConfigDialog : Form
  {
    private MFApplicationDeployment m_appDeploy;
    private MFKeyConfig m_keyTool = new MFKeyConfig();
    private PublicKeyUpdateInfo m_KeyUpdateInfo;
    private MFDevice m_device;
    private MFConfigHelper m_cfgHelper;
    private MFAppDeployConfigDialog.ConfigDialogCommand m_command;
    private BackgroundWorker m_backgroundWorker;
    private IContainer components;
    private Button buttonCancel;
    private Button buttonOk;
    private Label labelFile1;
    private Button buttonBrowseFile1;
    private Button buttonBrowsePrivateKey;
    private Label labelPrivateKey;
    private Label labelKeyIndex;
    private ComboBox comboBoxKeyIndex;
    private ComboBox comboBoxFile1;
    private ComboBox comboBoxPrivateKey;
    private Button buttonCreate;
    private ProgressBar progressBar;

    public MFAppDeployConfigDialog(
      MFAppDeployConfigDialog.ConfigDialogCommand command)
      : this((MFDevice) null, command)
    {
    }

    public MFAppDeployConfigDialog(
      MFDevice device,
      MFAppDeployConfigDialog.ConfigDialogCommand command)
    {
      if (device != null)
      {
        this.m_cfgHelper = new MFConfigHelper(device);
        this.m_appDeploy = new MFApplicationDeployment(device);
      }
      this.m_command = command;
      this.m_device = device;
      this.InitializeComponent();
    }

    public PublicKeyUpdateInfo KeyUpdateInfo
    {
      get => this.m_KeyUpdateInfo;
      set => this.m_KeyUpdateInfo = value;
    }

    private bool CheckKeySize(int size, MFAppDeployConfigDialog.KeyFileType type)
    {
      switch (type)
      {
        case MFAppDeployConfigDialog.KeyFileType.PublicKey:
          return size == 260;
        case MFAppDeployConfigDialog.KeyFileType.PrivateKey:
          return size == 260;
        case MFAppDeployConfigDialog.KeyFileType.Signature:
          return size == 128;
        case MFAppDeployConfigDialog.KeyFileType.HexFile:
          return true;
        default:
          return false;
      }
    }

    private bool CheckKey(byte[] key, MFAppDeployConfigDialog.KeyFileType type) => this.CheckKeySize(key.Length, type);

    private bool CheckFile(string file, MFAppDeployConfigDialog.KeyFileType type)
    {
      int size = 0;
      string format = "";
      switch (type)
      {
        case MFAppDeployConfigDialog.KeyFileType.PublicKey:
        case MFAppDeployConfigDialog.KeyFileType.PrivateKey:
          try
          {
            KeyPair keyPair = this.m_keyTool.LoadKeyPair(file);
            size = (type == MFAppDeployConfigDialog.KeyFileType.PublicKey ? keyPair.PublicKey : keyPair.PrivateKey).Length;
          }
          catch
          {
            int num = (int) MessageBox.Show((IWin32Window) this, string.Format(Resources.ErrorFileInvalid, (object) file), Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return false;
          }
          format = Resources.ErrorKeyFileInvalid;
          break;
        case MFAppDeployConfigDialog.KeyFileType.Signature:
          try
          {
            size = (int) new FileInfo(file).Length;
          }
          catch
          {
            int num = (int) MessageBox.Show((IWin32Window) this, string.Format(Resources.ErrorFileInvalid, (object) file), Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return false;
          }
          format = Resources.ErrorSignatureFileInvalid;
          break;
      }
      if (this.CheckKeySize(size, type))
        return true;
      int num1 = (int) MessageBox.Show((IWin32Window) this, string.Format(format, (object) Path.GetFileName(file)), Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
      return false;
    }

    private void buttonBrowseFile1_Click(object sender, EventArgs e)
    {
      FileDialog fileDialog = (FileDialog) null;
      MFAppDeployConfigDialog.KeyFileType type = MFAppDeployConfigDialog.KeyFileType.PrivateKey;
      switch (this.m_command)
      {
        case MFAppDeployConfigDialog.ConfigDialogCommand.Configure:
          OpenFileDialog openFileDialog1 = new OpenFileDialog();
          openFileDialog1.Filter = Resources.FileDialogFilterKeys;
          openFileDialog1.FilterIndex = 0;
          openFileDialog1.Multiselect = false;
          openFileDialog1.RestoreDirectory = true;
          type = MFAppDeployConfigDialog.KeyFileType.PublicKey;
          fileDialog = (FileDialog) openFileDialog1;
          break;
        case MFAppDeployConfigDialog.ConfigDialogCommand.CreateDeployment:
          SaveFileDialog saveFileDialog = new SaveFileDialog();
          saveFileDialog.Filter = Resources.FileDialogFilterDeploymentFiles;
          saveFileDialog.FilterIndex = 0;
          saveFileDialog.RestoreDirectory = true;
          saveFileDialog.Title = Resources.CreateDeploymentTitle;
          type = MFAppDeployConfigDialog.KeyFileType.HexFile;
          fileDialog = (FileDialog) saveFileDialog;
          break;
        case MFAppDeployConfigDialog.ConfigDialogCommand.SignDeployment:
          OpenFileDialog openFileDialog2 = new OpenFileDialog();
          openFileDialog2.Filter = Resources.FileDialogFilterDeploymentFiles;
          openFileDialog2.FilterIndex = 0;
          openFileDialog2.Multiselect = false;
          openFileDialog2.RestoreDirectory = true;
          type = MFAppDeployConfigDialog.KeyFileType.HexFile;
          fileDialog = (FileDialog) openFileDialog2;
          break;
      }
      if (DialogResult.OK != fileDialog.ShowDialog((IWin32Window) this))
        return;
      this.comboBoxFile1.Text = fileDialog.FileName;
      if (!(fileDialog is OpenFileDialog) || this.CheckFile(this.comboBoxFile1.Text, type))
        return;
      this.comboBoxFile1.SelectAll();
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      try
      {
        ComboBox comboBox = (ComboBox) null;
        string text1 = (string) null;
        string text2 = this.comboBoxFile1.Text;
        if (!File.Exists(text2) && (this.m_command == MFAppDeployConfigDialog.ConfigDialogCommand.Configure && !this.CheckFile(text2, MFAppDeployConfigDialog.KeyFileType.PublicKey) || this.m_command == MFAppDeployConfigDialog.ConfigDialogCommand.SignDeployment && !this.CheckFile(text2, MFAppDeployConfigDialog.KeyFileType.HexFile)))
        {
          comboBox = this.comboBoxFile1;
          text1 = string.Format(Resources.ErrorFileInvalid, (object) text2);
        }
        string text3 = this.comboBoxPrivateKey.Text;
        if (this.comboBoxPrivateKey.Enabled && this.m_command != MFAppDeployConfigDialog.ConfigDialogCommand.CreateDeployment)
        {
          if (text3.Trim() == "")
          {
            comboBox = this.comboBoxPrivateKey;
            text1 = Resources.ErrorKeyRequired;
          }
          else if (!File.Exists(text3) || !this.CheckFile(text3, MFAppDeployConfigDialog.KeyFileType.PublicKey))
          {
            comboBox = this.comboBoxPrivateKey;
            text1 = string.Format(Resources.ErrorFileInvalid, (object) text3);
          }
        }
        if (text1 != null)
        {
          comboBox.SelectAll();
          comboBox.Focus();
          int num = (int) MessageBox.Show((IWin32Window) this, text1, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }
        else
        {
          switch (this.m_command)
          {
            case MFAppDeployConfigDialog.ConfigDialogCommand.Configure:
              this.OnConfigure(sender, e);
              break;
            case MFAppDeployConfigDialog.ConfigDialogCommand.CreateDeployment:
              this.OnCreateDeployment(sender, e);
              break;
            case MFAppDeployConfigDialog.ConfigDialogCommand.SignDeployment:
              this.OnSignDeployment(sender, e);
              break;
          }
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, ex.Message, Resources.MenuItemPublicKeyConfiguration, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void CreateSignatureFile(byte[] binData)
    {
      string text1 = this.comboBoxPrivateKey.Text;
      string text2 = this.comboBoxFile1.Text;
      if (binData.Length == 0)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, Resources.ErrorNoDeploymentAssemblies, Resources.TitleAppDeploy);
      }
      else
      {
        if (string.IsNullOrEmpty(text1))
          return;
        byte[] privateKey = this.m_keyTool.LoadKeyPair(text1).PrivateKey;
        byte[] buffer = this.m_keyTool.SignData(binData, privateKey);
        using (FileStream fileStream = File.Create(Path.ChangeExtension(this.HexFile, ".sig")))
          fileStream.Write(buffer, 0, buffer.Length);
      }
    }

    private string PrivateKeyFile => this.comboBoxPrivateKey.Text;

    private string PublicKeyFile => this.comboBoxFile1.Text;

    private string HexFile => this.comboBoxFile1.Text;

    private void OnCreateDeployment(object sender, EventArgs e)
    {
      this.m_backgroundWorker = new BackgroundWorker();
      this.m_backgroundWorker.WorkerReportsProgress = true;
      this.m_backgroundWorker.WorkerSupportsCancellation = true;
      this.m_backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(this.OnCreateDeployment_Progress);
      this.m_backgroundWorker.DoWork += new DoWorkEventHandler(this.OnCreateDeployment_DoWork);
      this.m_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.OnCreateDeployment_Completed);
      this.m_backgroundWorker.RunWorkerAsync((object) this.m_device);
      this.comboBoxFile1.Enabled = false;
      this.labelFile1.Enabled = false;
      this.comboBoxPrivateKey.Enabled = false;
      this.labelPrivateKey.Enabled = false;
      this.buttonBrowseFile1.Enabled = false;
      this.buttonBrowsePrivateKey.Enabled = false;
      this.buttonOk.Enabled = false;
      this.buttonCreate.Enabled = false;
      this.progressBar.Visible = true;
    }

    private void OnCreateDeployment_Progress(object sender, ProgressChangedEventArgs e) => this.progressBar.Value = e.ProgressPercentage;

    private void OnCreateDeployment_Completed(object sender, RunWorkerCompletedEventArgs e)
    {
      if (e.Cancelled)
      {
        this.DialogResult = DialogResult.Cancel;
      }
      else
      {
        MFApplicationDeployment.MFApplicationDeploymentData result = e.Result as MFApplicationDeployment.MFApplicationDeploymentData;
        using (FileStream fileStream = File.Create(this.HexFile))
          fileStream.Write(result.HexData, 0, result.HexData.Length);
        this.CreateSignatureFile(result.BinaryData);
        this.DialogResult = DialogResult.OK;
      }
      this.m_backgroundWorker = (BackgroundWorker) null;
    }

    private void OnCreateDeployment_DoWork(object sender, DoWorkEventArgs e) => this.m_appDeploy.CreateDeploymentData(sender as BackgroundWorker, e);

    private void OnSignDeployment(object sender, EventArgs e)
    {
      this.CreateSignatureFile(new MFApplicationDeployment.BinToSrec().GetBinaryData(this.HexFile));
      this.DialogResult = DialogResult.OK;
    }

    private void OnConfigure(object sender, EventArgs e)
    {
      string text1 = this.comboBoxFile1.Text;
      string text2 = this.comboBoxPrivateKey.Text;
      this.m_KeyUpdateInfo = new PublicKeyUpdateInfo();
      this.m_KeyUpdateInfo.PublicKeyIndex = this.comboBoxKeyIndex.SelectedIndex == 1 ? PublicKeyUpdateInfo.KeyIndex.DeploymentKey : PublicKeyUpdateInfo.KeyIndex.FirmwareKey;
      try
      {
        this.m_KeyUpdateInfo.NewPublicKey = this.m_keyTool.LoadKeyPair(text1).PublicKey;
      }
      catch
      {
        int num = (int) MessageBox.Show((IWin32Window) this, string.Format(Resources.ErrorFileInvalid, (object) text1), Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      if (this.comboBoxPrivateKey.Enabled)
        this.m_KeyUpdateInfo.NewPublicKeySignature = this.m_keyTool.SignData(this.m_KeyUpdateInfo.NewPublicKey, this.m_keyTool.LoadKeyPair(text2).PrivateKey);
      this.m_cfgHelper.MaintainConnection = false;
      if (!this.m_cfgHelper.UpdatePublicKey(this.m_KeyUpdateInfo))
      {
        int num1 = (int) MessageBox.Show((IWin32Window) this, Resources.ErrorUnableToUpdateKey, this.Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      else
        this.DialogResult = DialogResult.OK;
    }

    private void buttonBrowsePrivateKey_Click(object sender, EventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = Resources.FileDialogFilterKeys;
      openFileDialog.FilterIndex = 0;
      openFileDialog.Multiselect = false;
      openFileDialog.RestoreDirectory = true;
      if (DialogResult.OK != openFileDialog.ShowDialog((IWin32Window) this))
        return;
      this.comboBoxPrivateKey.Text = openFileDialog.FileName;
      if (this.CheckFile(this.comboBoxPrivateKey.Text, MFAppDeployConfigDialog.KeyFileType.PrivateKey))
        return;
      this.comboBoxPrivateKey.SelectAll();
    }

    private void ConfigDialog_Load(object sender, EventArgs e)
    {
      if (this.m_cfgHelper != null)
        this.m_cfgHelper.MaintainConnection = true;
      switch (this.m_command)
      {
        case MFAppDeployConfigDialog.ConfigDialogCommand.Configure:
          this.comboBoxKeyIndex.SelectedIndex = 1;
          this.labelFile1.Text = Resources.LabelNewKey;
          this.comboBoxPrivateKey.Enabled = this.m_cfgHelper.IsDeploymentKeyLocked;
          this.buttonBrowsePrivateKey.Enabled = this.m_cfgHelper.IsDeploymentKeyLocked;
          this.labelPrivateKey.Text = Resources.LabelOldPrivateKey;
          this.Text = Resources.MenuItemPublicKeyConfiguration;
          break;
        case MFAppDeployConfigDialog.ConfigDialogCommand.CreateDeployment:
        case MFAppDeployConfigDialog.ConfigDialogCommand.SignDeployment:
          this.labelKeyIndex.Visible = false;
          this.comboBoxKeyIndex.Visible = false;
          this.labelPrivateKey.Text = Resources.LabelPrivateKey;
          this.labelFile1.Text = Resources.LabelDeploymentFile;
          this.Text = this.m_command == MFAppDeployConfigDialog.ConfigDialogCommand.CreateDeployment ? Resources.MenuItemCreateApplicationDeployment : Resources.MenuItemSignHexFile;
          break;
      }
    }

    private void comboBoxKeyIndex_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (this.comboBoxKeyIndex.SelectedIndex)
      {
        case 0:
          this.comboBoxPrivateKey.Enabled = this.m_cfgHelper.IsFirmwareKeyLocked;
          this.buttonBrowsePrivateKey.Enabled = this.m_cfgHelper.IsFirmwareKeyLocked;
          break;
        case 1:
          this.comboBoxPrivateKey.Enabled = this.m_cfgHelper.IsDeploymentKeyLocked;
          this.buttonBrowsePrivateKey.Enabled = this.m_cfgHelper.IsDeploymentKeyLocked;
          break;
      }
    }

    private void buttonCreate_Click(object sender, EventArgs e)
    {
      string keyPairFileDialog = MFAppDeployConfigDialog.ShowCreateKeyPairFileDialog();
      if (this.m_command == MFAppDeployConfigDialog.ConfigDialogCommand.Configure)
        this.comboBoxFile1.Text = keyPairFileDialog;
      else
        this.comboBoxPrivateKey.Text = keyPairFileDialog;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      if (this.m_backgroundWorker != null)
        this.m_backgroundWorker.CancelAsync();
      else
        this.DialogResult = DialogResult.Cancel;
    }

    internal static string ShowCreateKeyPairFileDialog()
    {
      string fileName = "";
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.DefaultExt = "*.key";
      saveFileDialog.CheckPathExists = true;
      saveFileDialog.Filter = Resources.FileDialogFilterKeys;
      saveFileDialog.FilterIndex = 0;
      saveFileDialog.AddExtension = true;
      saveFileDialog.OverwritePrompt = true;
      saveFileDialog.Title = Resources.SaveKeyTitle;
      if (DialogResult.OK == saveFileDialog.ShowDialog())
      {
        fileName = saveFileDialog.FileName;
        MFKeyConfig mfKeyConfig = new MFKeyConfig();
        KeyPair keyPair = mfKeyConfig.CreateKeyPair();
        mfKeyConfig.SaveKeyPair(keyPair, fileName);
      }
      return fileName;
    }

    private void MFAppDeployConfigDialog_FormClosing(object sender, FormClosingEventArgs e)
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
      this.buttonCancel = new Button();
      this.buttonOk = new Button();
      this.labelFile1 = new Label();
      this.buttonBrowseFile1 = new Button();
      this.buttonBrowsePrivateKey = new Button();
      this.labelPrivateKey = new Label();
      this.labelKeyIndex = new Label();
      this.comboBoxKeyIndex = new ComboBox();
      this.comboBoxFile1 = new ComboBox();
      this.comboBoxPrivateKey = new ComboBox();
      this.buttonCreate = new Button();
      this.progressBar = new ProgressBar();
      this.SuspendLayout();
      this.buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.buttonCancel.Location = new Point(386, 98);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new Size(82, 23);
      this.buttonCancel.TabIndex = 7;
      this.buttonCancel.Text = Resources.ButtonCancel;
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
      this.buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.buttonOk.Location = new Point(291, 98);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new Size(89, 23);
      this.buttonOk.TabIndex = 6;
      this.buttonOk.Text = "&Ok";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new EventHandler(this.buttonOk_Click);
      this.labelFile1.AutoSize = true;
      this.labelFile1.Location = new Point(14, 14);
      this.labelFile1.Name = "labelFile1";
      this.labelFile1.Size = new Size(85, 13);
      this.labelFile1.TabIndex = 4;
      this.labelFile1.Text = "New Public Key:";
      this.buttonBrowseFile1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonBrowseFile1.Location = new Point(439, 10);
      this.buttonBrowseFile1.Name = "buttonBrowseFile1";
      this.buttonBrowseFile1.Size = new Size(29, 23);
      this.buttonBrowseFile1.TabIndex = 8;
      this.buttonBrowseFile1.Text = Resources.ButtonBrowseDotDotDot;
      this.buttonBrowseFile1.TextAlign = ContentAlignment.TopCenter;
      this.buttonBrowseFile1.UseVisualStyleBackColor = true;
      this.buttonBrowseFile1.Click += new EventHandler(this.buttonBrowseFile1_Click);
      this.buttonBrowsePrivateKey.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonBrowsePrivateKey.Location = new Point(439, 37);
      this.buttonBrowsePrivateKey.Name = "buttonBrowsePrivateKey";
      this.buttonBrowsePrivateKey.Size = new Size(29, 23);
      this.buttonBrowsePrivateKey.TabIndex = 12;
      this.buttonBrowsePrivateKey.Text = Resources.ButtonBrowseDotDotDot;
      this.buttonBrowsePrivateKey.TextAlign = ContentAlignment.TopCenter;
      this.buttonBrowsePrivateKey.UseVisualStyleBackColor = true;
      this.buttonBrowsePrivateKey.Click += new EventHandler(this.buttonBrowsePrivateKey_Click);
      this.labelPrivateKey.AutoSize = true;
      this.labelPrivateKey.Location = new Point(14, 41);
      this.labelPrivateKey.Name = "labelPrivateKey";
      this.labelPrivateKey.Size = new Size(83, 13);
      this.labelPrivateKey.TabIndex = 10;
      this.labelPrivateKey.Text = "Old Private Key:";
      this.labelKeyIndex.AutoSize = true;
      this.labelKeyIndex.Location = new Point(14, 68);
      this.labelKeyIndex.Name = "labelKeyIndex";
      this.labelKeyIndex.Size = new Size(89, 13);
      this.labelKeyIndex.TabIndex = 13;
      this.labelKeyIndex.Text = "Public Key Index:";
      this.comboBoxKeyIndex.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.comboBoxKeyIndex.DropDownStyle = ComboBoxStyle.DropDownList;
      this.comboBoxKeyIndex.FormattingEnabled = true;
      this.comboBoxKeyIndex.Items.AddRange(new object[2]
      {
        (object) Resources.KeyIndexFirmware,
        (object) Resources.KeyIndexDeployment
      });
      this.comboBoxKeyIndex.Location = new Point(121, 65);
      this.comboBoxKeyIndex.Name = "comboBoxKeyIndex";
      this.comboBoxKeyIndex.Size = new Size(312, 21);
      this.comboBoxKeyIndex.TabIndex = 14;
      this.comboBoxKeyIndex.SelectedIndexChanged += new EventHandler(this.comboBoxKeyIndex_SelectedIndexChanged);
      this.comboBoxFile1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.comboBoxFile1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
      this.comboBoxFile1.AutoCompleteSource = AutoCompleteSource.FileSystem;
      this.comboBoxFile1.DropDownStyle = ComboBoxStyle.Simple;
      this.comboBoxFile1.FormattingEnabled = true;
      this.comboBoxFile1.Location = new Point(121, 11);
      this.comboBoxFile1.Name = "comboBoxFile1";
      this.comboBoxFile1.Size = new Size(312, 21);
      this.comboBoxFile1.TabIndex = 15;
      this.comboBoxPrivateKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.comboBoxPrivateKey.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
      this.comboBoxPrivateKey.AutoCompleteSource = AutoCompleteSource.FileSystem;
      this.comboBoxPrivateKey.DropDownStyle = ComboBoxStyle.Simple;
      this.comboBoxPrivateKey.FormattingEnabled = true;
      this.comboBoxPrivateKey.Location = new Point(121, 38);
      this.comboBoxPrivateKey.Name = "comboBoxPrivateKey";
      this.comboBoxPrivateKey.Size = new Size(312, 21);
      this.comboBoxPrivateKey.TabIndex = 16;
      this.buttonCreate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.buttonCreate.Location = new Point(121, 98);
      this.buttonCreate.Name = "buttonCreate";
      this.buttonCreate.Size = new Size(105, 23);
      this.buttonCreate.TabIndex = 7;
      this.buttonCreate.Text = "C&reate Key";
      this.buttonCreate.UseVisualStyleBackColor = true;
      this.buttonCreate.Click += new EventHandler(this.buttonCreate_Click);
      this.progressBar.Location = new Point(121, 68);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new Size(312, 18);
      this.progressBar.TabIndex = 17;
      this.progressBar.Visible = false;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(480, 133);
      this.ControlBox = false;
      this.Controls.Add((Control) this.comboBoxPrivateKey);
      this.Controls.Add((Control) this.comboBoxFile1);
      this.Controls.Add((Control) this.labelKeyIndex);
      this.Controls.Add((Control) this.buttonBrowsePrivateKey);
      this.Controls.Add((Control) this.labelPrivateKey);
      this.Controls.Add((Control) this.buttonBrowseFile1);
      this.Controls.Add((Control) this.buttonCreate);
      this.Controls.Add((Control) this.buttonCancel);
      this.Controls.Add((Control) this.buttonOk);
      this.Controls.Add((Control) this.labelFile1);
      this.Controls.Add((Control) this.progressBar);
      this.Controls.Add((Control) this.comboBoxKeyIndex);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (MFAppDeployConfigDialog);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = SizeGripStyle.Hide;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "Public Key Configuration";
      this.FormClosing += new FormClosingEventHandler(this.MFAppDeployConfigDialog_FormClosing);
      this.Load += new EventHandler(this.ConfigDialog_Load);
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    public enum ConfigDialogCommand
    {
      Configure,
      CreateDeployment,
      SignDeployment,
    }

    private enum KeyFileType
    {
      PublicKey,
      PrivateKey,
      Signature,
      HexFile,
    }
  }
}
