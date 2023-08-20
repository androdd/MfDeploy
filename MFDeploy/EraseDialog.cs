// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.EraseDialog
// Assembly: MFDeploy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 9518305E-2E58-43E8-9F45-41B78F83BC9E
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.SPOT.Debugger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  public class EraseDialog : Form
  {
    private MFDevice m_device;
    public List<EraseOptions> m_eraseBlocks = new List<EraseOptions>();
    private IContainer components;
    private ListView listViewEraseSectors;
    private ColumnHeader columnHeaderName;
    private Button buttonErase;
    private Button buttonEraseSectors;
    private ColumnHeader columnHeaderAddress;
    private ColumnHeader columnHeaderSize;

    public EraseDialog(MFDevice dev)
    {
      this.InitializeComponent();
      this.m_device = dev;
    }

    public EraseOptions[] EraseBlocks => this.m_eraseBlocks.ToArray();

    private void EraseDialog_Load(object sender, EventArgs e)
    {
      Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_FlashSectorMap.Reply flashSectorMap = this.m_device.DbgEngine.GetFlashSectorMap();
      if (flashSectorMap == null)
        return;
      Dictionary<EraseOptions, Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData> dictionary1 = new Dictionary<EraseOptions, Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData>();
      Dictionary<EraseOptions, string> dictionary2 = new Dictionary<EraseOptions, string>();
      for (int index = 0; index < flashSectorMap.m_map.Length; ++index)
      {
        Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData1 = flashSectorMap.m_map[index];
        string str = "";
        EraseOptions key = (EraseOptions) (-1);
        switch (flashSectorData1.m_flags & 240U)
        {
          case 32:
          case 48:
            if (this.m_device.DbgEngine.ConnectionSource == ConnectionSource.TinyBooter)
            {
              str = "Firmware";
              key = EraseOptions.Firmware;
              break;
            }
            break;
          case 64:
            str = "File System";
            key = EraseOptions.FileSystem;
            break;
          case 80:
            str = "Deployment";
            key = EraseOptions.Deployment;
            break;
          case 96:
            str = "Update Storage";
            key = EraseOptions.UpdateStorage;
            break;
          case 144:
          case 160:
            str = "Simple Storage";
            key = EraseOptions.SimpleStorage;
            break;
          case 224:
          case 240:
            str = "User Storage";
            key = EraseOptions.UserStorage;
            break;
        }
        if (key != (EraseOptions)(-1))
        {
          if (dictionary1.ContainsKey(key))
          {
            Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData2 = dictionary1[key];
            if ((int) flashSectorData2.m_address + (int) flashSectorData2.m_size == (int) flashSectorData1.m_address)
            {
              flashSectorData2.m_size += flashSectorData1.m_size;
              dictionary1[key] = flashSectorData2;
            }
          }
          else
          {
            dictionary1[key] = flashSectorData1;
            dictionary2[key] = str;
          }
        }
      }
      foreach (EraseOptions key in dictionary1.Keys)
      {
        ListViewItem listViewItem = this.listViewEraseSectors.Items.Add(new ListViewItem(new string[3]
        {
          dictionary2[key],
          string.Format("0x{0:X08}", (object) dictionary1[key].m_address),
          string.Format("0x{0:X08}", (object) dictionary1[key].m_size)
        }));
        listViewItem.Tag = (object) key;
        if (key != EraseOptions.Firmware)
          listViewItem.Checked = true;
      }
    }

    private void buttonEraseSectors_Click(object sender, EventArgs e)
    {
      for (int index = 0; index < this.listViewEraseSectors.Items.Count; ++index)
      {
        ListViewItem listViewItem = this.listViewEraseSectors.Items[index];
        if (listViewItem.Checked)
          this.m_eraseBlocks.Add((EraseOptions) listViewItem.Tag);
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.listViewEraseSectors = new ListView();
      this.columnHeaderName = new ColumnHeader();
      this.columnHeaderAddress = new ColumnHeader();
      this.buttonErase = new Button();
      this.buttonEraseSectors = new Button();
      this.columnHeaderSize = new ColumnHeader();
      this.SuspendLayout();
      this.listViewEraseSectors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.listViewEraseSectors.BackColor = SystemColors.Window;
      this.listViewEraseSectors.CheckBoxes = true;
      this.listViewEraseSectors.Columns.AddRange(new ColumnHeader[3]
      {
        this.columnHeaderName,
        this.columnHeaderAddress,
        this.columnHeaderSize
      });
      this.listViewEraseSectors.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.listViewEraseSectors.FullRowSelect = true;
      this.listViewEraseSectors.GridLines = true;
      this.listViewEraseSectors.HideSelection = false;
      this.listViewEraseSectors.Location = new Point(12, 12);
      this.listViewEraseSectors.Name = "listViewEraseSectors";
      this.listViewEraseSectors.Size = new Size(359, 131);
      this.listViewEraseSectors.TabIndex = 10;
      this.listViewEraseSectors.UseCompatibleStateImageBehavior = false;
      this.listViewEraseSectors.View = View.Details;
      this.columnHeaderName.Tag = (object) "";
      this.columnHeaderName.Text = "Block Usage";
      this.columnHeaderName.Width = 120;
      this.columnHeaderAddress.Text = "Address";
      this.columnHeaderAddress.Width = 115;
      this.buttonErase.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.buttonErase.DialogResult = DialogResult.Cancel;
      this.buttonErase.Location = new Point(296, 149);
      this.buttonErase.Name = "buttonErase";
      this.buttonErase.Size = new Size(75, 23);
      this.buttonErase.TabIndex = 12;
      this.buttonErase.Text = "&Cancel";
      this.buttonErase.UseVisualStyleBackColor = true;
      this.buttonEraseSectors.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.buttonEraseSectors.DialogResult = DialogResult.OK;
      this.buttonEraseSectors.Location = new Point(215, 149);
      this.buttonEraseSectors.Name = "buttonEraseSectors";
      this.buttonEraseSectors.Size = new Size(75, 23);
      this.buttonEraseSectors.TabIndex = 11;
      this.buttonEraseSectors.Text = "&Erase";
      this.buttonEraseSectors.UseVisualStyleBackColor = true;
      this.buttonEraseSectors.Click += new EventHandler(this.buttonEraseSectors_Click);
      this.columnHeaderSize.Text = "Size";
      this.columnHeaderSize.Width = 115;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(383, 184);
      this.Controls.Add((Control) this.listViewEraseSectors);
      this.Controls.Add((Control) this.buttonErase);
      this.Controls.Add((Control) this.buttonEraseSectors);
      this.Name = nameof (EraseDialog);
      this.Text = "Erase Sectors";
      this.Load += new EventHandler(this.EraseDialog_Load);
      this.ResumeLayout(false);
    }
  }
}
