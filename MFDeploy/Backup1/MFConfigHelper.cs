// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFConfigHelper
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFConfigHelper : IDisposable
  {
    private const uint c_Version_V2 = 843858248;
    private const uint c_Seed = 1;
    private const uint c_EnumerateAndLaunchAddr = 0;
    private HAL_CONFIGURATION_SECTOR m_StaticConfig;
    private bool m_init;
    private Hashtable m_cfgHash = new Hashtable();
    private int m_lastCfgIndex = -1;
    private byte[] m_all_cfg_data;
    private MFDevice m_device;
    private bool m_firmwareKeyLocked = true;
    private bool m_deployKeyLocked = true;
    private Thread m_thread;
    private bool m_fValidConfig;
    private bool m_isDisposed;
    private bool m_fRestartCLR;
    private bool m_fStaticCfgOK;
    private Commands.Monitor_FlashSectorMap.FlashSectorData m_cfg_sector;

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (this.m_isDisposed)
        return;
      if (disposing)
      {
        try
        {
          if (this.m_fRestartCLR)
          {
            if (this.m_device.DbgEngine != null)
            {
              if (this.m_device.DbgEngine.ConnectionSource == ConnectionSource.TinyBooter)
                this.m_device.Execute(0U);
            }
          }
        }
        catch
        {
        }
      }
      try
      {
        this.MaintainConnection = false;
      }
      catch
      {
      }
      this.m_isDisposed = true;
    }

    ~MFConfigHelper() => this.Dispose(false);

    public MFConfigHelper(MFDevice device) => this.m_device = device;

    private byte[] MarshalData(object obj)
    {
      byte[] numArray = new byte[Marshal.SizeOf(obj)];
      GCHandle gcHandle = GCHandle.Alloc((object) numArray, GCHandleType.Pinned);
      Marshal.StructureToPtr(obj, gcHandle.AddrOfPinnedObject(), false);
      gcHandle.Free();
      return numArray;
    }

    public static object UnmarshalData(byte[] data, System.Type type)
    {
      GCHandle gcHandle = GCHandle.Alloc((object) data, GCHandleType.Pinned);
      object structure = Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), type);
      gcHandle.Free();
      return structure;
    }

    public bool MaintainConnection
    {
      set
      {
        if (value)
        {
          if (this.m_thread != null)
            return;
          this.m_thread = new Thread(new ThreadStart(this.TickleWireProtocol));
          this.m_thread.Start();
        }
        else
        {
          if (this.m_thread == null)
            return;
          this.m_thread.Abort();
          this.m_thread.Join();
          this.m_thread = (Thread) null;
        }
      }
      get => this.m_thread != null;
    }

    private void TickleWireProtocol()
    {
      while (true)
      {
        int num = (int) this.m_device.Ping();
        Thread.Sleep(2000);
      }
    }

    private unsafe bool CheckKeyLocked(HAL_CONFIGURATION_SECTOR cfg, bool firmwareKey)
    {
      bool flag = false;
      byte* numPtr = firmwareKey ? cfg.PublicKeyFirmware.SectorKey : cfg.PublicKeyDeployment.SectorKey;
      for (int index = 0; index < 260; ++index)
      {
        if (numPtr[index] != byte.MaxValue)
        {
          flag = true;
          break;
        }
      }
      return flag;
    }

    private void InitializeConfigData()
    {
      uint length1 = (uint) sizeof (HAL_CONFIG_BLOCK);
      uint num = (uint) sizeof (HAL_CONFIGURATION_SECTOR) - length1;
      this.m_cfg_sector.m_address = uint.MaxValue;
      this.m_cfg_sector.m_size = 0U;
      Microsoft.SPOT.Debugger.Engine dbgEngine = this.m_device.DbgEngine;
      if (!dbgEngine.TryToConnect(10, 100, true, ConnectionSource.Unknown))
        throw new MFDeviceNoResponseException();
      if (this.m_device.DbgEngine.ConnectionSource != ConnectionSource.TinyBooter)
      {
        this.m_fRestartCLR = true;
        if (!this.m_device.ConnectToTinyBooter())
          throw new MFDeviceNoResponseException();
      }
      foreach (Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData in dbgEngine.GetFlashSectorMap().m_map)
      {
        if (48 == ((int) flashSectorData.m_flags & 240))
        {
          this.m_cfg_sector = flashSectorData;
          break;
        }
      }
      if (this.m_cfg_sector.m_address != uint.MaxValue)
      {
        int length2 = sizeof (HAL_CONFIGURATION_SECTOR);
        dbgEngine.ReadMemory(this.m_cfg_sector.m_address, (uint) length2, out this.m_all_cfg_data);
        this.m_StaticConfig = (HAL_CONFIGURATION_SECTOR) MFConfigHelper.UnmarshalData(this.m_all_cfg_data, typeof (HAL_CONFIGURATION_SECTOR));
        if (this.m_StaticConfig.ConfigurationLength >= this.m_cfg_sector.m_size)
          throw new MFInvalidConfigurationSectorException();
        this.m_fStaticCfgOK = this.m_StaticConfig.Version.TinyBooter == (byte) 4 || (int) this.m_StaticConfig.ConfigurationLength != (int) num;
        this.m_fValidConfig = this.m_StaticConfig.Version.TinyBooter == (byte) 4;
        if (this.m_fStaticCfgOK)
        {
          this.m_firmwareKeyLocked = this.CheckKeyLocked(this.m_StaticConfig, true);
          this.m_deployKeyLocked = this.CheckKeyLocked(this.m_StaticConfig, false);
        }
        int configurationLength = (int) this.m_StaticConfig.ConfigurationLength;
label_16:
        byte[] buf1;
        dbgEngine.ReadMemory((uint) ((ulong) this.m_cfg_sector.m_address + (ulong) configurationLength), length1, out buf1);
        HAL_CONFIG_BLOCK halConfigBlock = (HAL_CONFIG_BLOCK) MFConfigHelper.UnmarshalData(buf1, typeof (HAL_CONFIG_BLOCK));
        if (halConfigBlock.Size > this.m_cfg_sector.m_size)
        {
          if (halConfigBlock.Size == uint.MaxValue)
            this.m_lastCfgIndex = configurationLength;
          this.m_all_cfg_data = new byte[configurationLength];
          int length3;
          for (int destinationIndex = 0; destinationIndex < configurationLength; destinationIndex += length3)
          {
            length3 = 512;
            if (configurationLength - destinationIndex < length3)
              length3 = configurationLength - destinationIndex;
            byte[] buf2;
            dbgEngine.ReadMemory((uint) ((ulong) this.m_cfg_sector.m_address + (ulong) destinationIndex), (uint) length3, out buf2);
            Array.Copy((Array) buf2, 0, (Array) this.m_all_cfg_data, destinationIndex, buf2.Length);
          }
        }
        else if ((long) (halConfigBlock.Size + length1) + (long) configurationLength <= (long) this.m_cfg_sector.m_size)
        {
          this.m_cfgHash[(object) halConfigBlock.DriverNameString] = (object) new MFConfigHelper.ConfigIndexData(configurationLength, (int) halConfigBlock.Size + (int) length1);
          configurationLength += (int) halConfigBlock.Size + (int) length1;
          while (configurationLength % 4 != 0)
            ++configurationLength;
          goto label_16;
        }
      }
      this.m_init = true;
    }

    public bool IsValidConfig
    {
      get
      {
        if (!this.m_init)
        {
          try
          {
            this.InitializeConfigData();
          }
          catch
          {
          }
        }
        return this.m_fValidConfig && this.m_fStaticCfgOK;
      }
    }

    public bool IsFirmwareKeyLocked
    {
      get
      {
        if (!this.m_init)
          this.InitializeConfigData();
        if (!this.m_fStaticCfgOK)
          throw new MFInvalidConfigurationSectorException();
        return this.m_firmwareKeyLocked;
      }
    }

    public bool IsDeploymentKeyLocked
    {
      get
      {
        if (!this.m_init)
          this.InitializeConfigData();
        if (!this.m_fStaticCfgOK)
          throw new MFInvalidConfigurationSectorException();
        return this.m_deployKeyLocked;
      }
    }

    public bool UpdatePublicKey(PublicKeyUpdateInfo newKeyInfo)
    {
      bool flag1 = false;
      if (!this.m_init)
        this.InitializeConfigData();
      if (!this.m_init)
        throw new MFInvalidConfigurationSectorException();
      if (!this.m_fStaticCfgOK)
        throw new MFInvalidConfigurationSectorException();
      if (newKeyInfo.NewPublicKey.Length != 260)
        throw new MFInvalidKeyLengthException();
      PublicKeyIndex keyIndex;
      bool flag2;
      if (newKeyInfo.PublicKeyIndex == PublicKeyUpdateInfo.KeyIndex.DeploymentKey)
      {
        keyIndex = PublicKeyIndex.DeploymentKey;
        flag2 = this.m_deployKeyLocked;
      }
      else
      {
        keyIndex = PublicKeyIndex.FirmwareKey;
        flag2 = this.m_firmwareKeyLocked;
      }
      if (!this.m_device.ConnectToTinyBooter())
        throw new MFTinyBooterConnectionFailureException();
      if (this.m_device.DbgEngine.UpdateSignatureKey(keyIndex, newKeyInfo.NewPublicKeySignature, newKeyInfo.NewPublicKey, (byte[]) null))
      {
        if (flag2)
          this.m_device.Reboot(true);
        flag1 = true;
      }
      if (PingConnectionType.TinyBooter == this.m_device.Ping())
        this.m_device.Execute(0U);
      return flag1;
    }

    public byte[] FindConfig(string configName)
    {
      byte[] destinationArray = (byte[]) null;
      if (!this.m_init)
        this.InitializeConfigData();
      if (this.m_cfgHash.ContainsKey((object) configName))
      {
        MFConfigHelper.ConfigIndexData configIndexData = (MFConfigHelper.ConfigIndexData) this.m_cfgHash[(object) configName];
        destinationArray = new byte[configIndexData.Size];
        Array.Copy((Array) this.m_all_cfg_data, configIndexData.Index, (Array) destinationArray, 0, configIndexData.Size);
      }
      return destinationArray;
    }

    public unsafe void WriteConfig(string configName, IHAL_CONFIG_BASE config)
    {
      HAL_CONFIG_BLOCK configHeader = config.ConfigHeader;
      uint index1 = (uint) sizeof (HAL_CONFIG_BLOCK);
      for (int index2 = 0; index2 < configName.Length; ++index2)
        configHeader.DriverName[index2] = (byte) configName[index2];
      configHeader.HeaderCRC = 0U;
      configHeader.DataCRC = 0U;
      configHeader.Size = (uint) config.Size - index1;
      configHeader.Signature = 843858248U;
      config.ConfigHeader = configHeader;
      byte[] rgBlock1 = this.MarshalData((object) config);
      configHeader.DataCRC = CRC.ComputeCRC(rgBlock1, (int) index1, (int) configHeader.Size, 0U);
      config.ConfigHeader = configHeader;
      byte[] rgBlock2 = this.MarshalData((object) config);
      configHeader.HeaderCRC = CRC.ComputeCRC(rgBlock2, 8, (int) index1 - 8, 1U);
      config.ConfigHeader = configHeader;
      byte[] sourceArray = this.MarshalData((object) config);
      Microsoft.SPOT.Debugger.Engine dbgEngine = this.m_device.DbgEngine;
      if (this.m_cfgHash.ContainsKey((object) configName))
      {
        MFConfigHelper.ConfigIndexData configIndexData = (MFConfigHelper.ConfigIndexData) this.m_cfgHash[(object) configName];
        if (configIndexData.Size != sourceArray.Length)
          throw new MFInvalidConfigurationDataException();
        Array.Copy((Array) sourceArray, 0, (Array) this.m_all_cfg_data, configIndexData.Index, sourceArray.Length);
        if (!dbgEngine.EraseMemory(this.m_cfg_sector.m_address, (uint) this.m_all_cfg_data.Length))
          throw new MFConfigSectorEraseFailureException();
        if (!dbgEngine.WriteMemory(this.m_cfg_sector.m_address, this.m_all_cfg_data))
          throw new MFConfigSectorWriteFailureException();
      }
      else
      {
        if (this.m_lastCfgIndex == -1)
          throw new MFConfigurationSectorOutOfMemoryException();
        byte[] destinationArray = new byte[this.m_all_cfg_data.Length + sourceArray.Length];
        Array.Copy((Array) this.m_all_cfg_data, 0, (Array) destinationArray, 0, this.m_all_cfg_data.Length);
        Array.Copy((Array) sourceArray, 0, (Array) destinationArray, this.m_all_cfg_data.Length, sourceArray.Length);
        this.m_all_cfg_data = destinationArray;
        if (!dbgEngine.EraseMemory(this.m_cfg_sector.m_address, (uint) this.m_all_cfg_data.Length))
          throw new MFConfigSectorEraseFailureException();
        if (!dbgEngine.WriteMemory(this.m_cfg_sector.m_address, this.m_all_cfg_data))
          throw new MFConfigSectorWriteFailureException();
        MFConfigHelper.ConfigIndexData configIndexData = new MFConfigHelper.ConfigIndexData(this.m_lastCfgIndex, sourceArray.Length);
        this.m_cfgHash.Add((object) configName, (object) configIndexData);
        this.m_lastCfgIndex += sourceArray.Length;
        while (this.m_lastCfgIndex % 4 != 0)
          ++this.m_lastCfgIndex;
      }
      if (!dbgEngine.CheckSignature(new byte[128], 0U) && dbgEngine.ConnectionSource == ConnectionSource.TinyBooter)
        throw new MFConfigSectorWriteFailureException();
      if (dbgEngine.ConnectionSource != ConnectionSource.TinyBooter)
        return;
      dbgEngine.ExecuteMemory(0U);
    }

    internal struct ConfigIndexData
    {
      internal int Index;
      internal int Size;

      internal ConfigIndexData(int idx, int size)
      {
        this.Index = idx;
        this.Size = size;
      }
    }
  }
}
