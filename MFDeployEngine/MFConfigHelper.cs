// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFConfigHelper
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

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
    private const int c_MaxDriverNameLength = 63;
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
    private bool m_fRestartClr = true;
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
          if (this.m_fRestartClr)
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

    public MFConfigHelper(MFDevice device)
    {
      this.m_device = device;
      if (device.DbgEngine.ConnectionSource == ConnectionSource.Unknown)
        device.Connect(500, true);
      this.m_fRestartClr = device.DbgEngine.ConnectionSource == ConnectionSource.TinyCLR;
    }

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

    public unsafe byte[] DeploymentPublicKey
    {
      get
      {
        byte[] deploymentPublicKey = new byte[260];
        fixed (byte* numPtr = this.m_StaticConfig.PublicKeyDeployment.SectorKey)
        {
          for (int index = 0; index < 260; ++index)
            deploymentPublicKey[index] = numPtr[index];
        }
        return deploymentPublicKey;
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
      uint length1, num;
      unsafe
      {
          length1 = (uint)sizeof(HAL_CONFIG_BLOCK);
          num = (uint)sizeof(HAL_CONFIGURATION_SECTOR) - length1;
      }

      this.m_cfg_sector.m_address = uint.MaxValue;
      this.m_cfg_sector.m_size = 0U;
      Microsoft.SPOT.Debugger.Engine dbgEngine = this.m_device.DbgEngine;
      if (!dbgEngine.TryToConnect(10, 100, true, ConnectionSource.Unknown))
        throw new MFDeviceNoResponseException();
      if (this.m_device.DbgEngine.PortDefinition is PortDefinition_Tcp)
        this.m_device.UpgradeToSsl();
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
        int length2;
        unsafe
        {
            length2 = sizeof(HAL_CONFIGURATION_SECTOR);
        }

        dbgEngine.ReadMemory(this.m_cfg_sector.m_address, (uint) length2, out this.m_all_cfg_data);
        this.m_StaticConfig = (HAL_CONFIGURATION_SECTOR) MFConfigHelper.UnmarshalData(this.m_all_cfg_data, typeof (HAL_CONFIGURATION_SECTOR));
        if (this.m_StaticConfig.ConfigurationLength == uint.MaxValue)
        {
          this.m_StaticConfig.ConfigurationLength = (uint) length2;
          this.m_StaticConfig.Version.Major = (byte) 3;
          this.m_StaticConfig.Version.Minor = (byte) 0;
          this.m_StaticConfig.Version.Extra = (byte) 0;
          this.m_StaticConfig.Version.TinyBooter = (byte) 4;
        }
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
        this.m_lastCfgIndex = configurationLength;
label_17:
        byte[] buf1;
        dbgEngine.ReadMemory((uint) ((ulong) this.m_cfg_sector.m_address + (ulong) configurationLength), length1, out buf1);
        HAL_CONFIG_BLOCK halConfigBlock = (HAL_CONFIG_BLOCK) MFConfigHelper.UnmarshalData(buf1, typeof (HAL_CONFIG_BLOCK));
        if (halConfigBlock.Size > this.m_cfg_sector.m_size)
        {
          this.m_lastCfgIndex = configurationLength;
          this.m_all_cfg_data = new byte[this.m_lastCfgIndex];
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
          goto label_17;
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
      if (this.m_device.DbgEngine.ConnectionSource != ConnectionSource.TinyBooter && !this.m_device.ConnectToTinyBooter())
        throw new MFDeviceNoResponseException();
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

    public void WriteConfig(string configName, IHAL_CONFIG_BASE config) => this.WriteConfig(configName, config, true);

    public void WriteConfig(string configName, IHAL_CONFIG_BASE config, bool updateConfigSector)
    {
      HAL_CONFIG_BLOCK configHeader = config.ConfigHeader;
      uint index;
      unsafe
      {
          index = (uint)sizeof(HAL_CONFIG_BLOCK);
      }

      configHeader.DriverNameString = configName;
      configHeader.HeaderCRC = 0U;
      configHeader.DataCRC = 0U;
      configHeader.Size = (uint) config.Size - index;
      configHeader.Signature = 843858248U;
      config.ConfigHeader = configHeader;
      byte[] rgBlock1 = this.MarshalData((object) config);
      configHeader.DataCRC = CRC.ComputeCRC(rgBlock1, (int) index, (int) configHeader.Size, 0U);
      config.ConfigHeader = configHeader;
      byte[] rgBlock2 = this.MarshalData((object) config);
      configHeader.HeaderCRC = CRC.ComputeCRC(rgBlock2, 8, (int) index - 8, 1U);
      config.ConfigHeader = configHeader;
      byte[] data = this.MarshalData((object) config);
      this.WriteConfig(configName, data, true, updateConfigSector);
    }

    public void WriteConfig(string configName, byte[] data)
    {
      HAL_CONFIG_BLOCK halConfigBlock = new HAL_CONFIG_BLOCK();
      uint num;
      unsafe
      {
          num = (uint)sizeof(HAL_CONFIG_BLOCK);
      }

      halConfigBlock.DriverNameString = configName;
      halConfigBlock.HeaderCRC = 0U;
      halConfigBlock.DataCRC = CRC.ComputeCRC(data, 0, data.Length, 0U);
      halConfigBlock.Size = (uint) data.Length;
      halConfigBlock.Signature = 843858248U;
      halConfigBlock.DataCRC = CRC.ComputeCRC(data, 0, data.Length, 0U);
      byte[] rgBlock = this.MarshalData((object) halConfigBlock);
      halConfigBlock.HeaderCRC = CRC.ComputeCRC(rgBlock, 8, (int) num - 8, 1U);
      byte[] sourceArray = this.MarshalData((object) halConfigBlock);
      byte[] numArray = new byte[(long) num + (long) data.Length];
      Array.Copy((Array) sourceArray, (Array) numArray, (long) num);
      Array.Copy((Array) data, 0L, (Array) numArray, (long) num, (long) data.Length);
      this.WriteConfig(configName, numArray, false, true);
    }

    private void WriteConfig(
      string configName,
      byte[] data,
      bool staticSize,
      bool updateConfigSector)
    {
      Microsoft.SPOT.Debugger.Engine dbgEngine = this.m_device.DbgEngine;
      if (!this.m_init)
        this.InitializeConfigData();
      if (this.m_cfgHash.ContainsKey((object) configName))
      {
        MFConfigHelper.ConfigIndexData configIndexData = (MFConfigHelper.ConfigIndexData) this.m_cfgHash[(object) configName];
        if (configIndexData.Size != data.Length)
        {
          if (staticSize)
            throw new MFInvalidConfigurationDataException();
          uint destinationIndex = (uint) (configIndexData.Index + data.Length);
          while (destinationIndex % 4U != 0U)
            ++destinationIndex;
          uint sourceIndex = (uint) (configIndexData.Index + configIndexData.Size);
          while (sourceIndex % 4U != 0U)
            ++sourceIndex;
          int num = (int) destinationIndex - (int) sourceIndex;
          byte[] destinationArray = new byte[this.m_lastCfgIndex + num];
          Array.Copy((Array) this.m_all_cfg_data, (Array) destinationArray, configIndexData.Index);
          Array.Copy((Array) data, 0, (Array) destinationArray, configIndexData.Index, data.Length);
          if ((long) sourceIndex < (long) this.m_lastCfgIndex)
            Array.Copy((Array) this.m_all_cfg_data, (long) sourceIndex, (Array) destinationArray, (long) destinationIndex, (long) this.m_all_cfg_data.Length - (long) sourceIndex);
          this.m_all_cfg_data = destinationArray;
          this.m_lastCfgIndex += num;
        }
        else
          Array.Copy((Array) data, 0, (Array) this.m_all_cfg_data, configIndexData.Index, data.Length);
      }
      else
      {
        if (this.m_lastCfgIndex == -1)
          throw new MFConfigurationSectorOutOfMemoryException();
        uint num = (uint) (this.m_lastCfgIndex + data.Length);
        while (num % 4U != 0U)
          ++num;
        byte[] destinationArray = new byte[this.m_lastCfgIndex >= this.m_all_cfg_data.Length ? this.m_lastCfgIndex + data.Length : this.m_all_cfg_data.Length];
        Array.Copy((Array) this.m_all_cfg_data, 0, (Array) destinationArray, 0, this.m_all_cfg_data.Length);
        Array.Copy((Array) data, 0, (Array) destinationArray, this.m_lastCfgIndex, data.Length);
        this.m_all_cfg_data = destinationArray;
        this.m_lastCfgIndex = (int) num;
      }
      if (!updateConfigSector)
        return;
      if (!dbgEngine.EraseMemory(this.m_cfg_sector.m_address, (uint) this.m_all_cfg_data.Length))
        throw new MFConfigSectorEraseFailureException();
      if (!dbgEngine.WriteMemory(this.m_cfg_sector.m_address, this.m_all_cfg_data))
        throw new MFConfigSectorWriteFailureException();
      this.m_cfgHash.Clear();
      uint length;
      unsafe
      {
          length = (uint)sizeof(HAL_CONFIG_BLOCK);
      }

      int configurationLength = (int) this.m_StaticConfig.ConfigurationLength;
      byte[] numArray = new byte[length];
      while (configurationLength < this.m_lastCfgIndex)
      {
        Array.Copy((Array) this.m_all_cfg_data, (long) configurationLength, (Array) numArray, 0L, (long) length);
        HAL_CONFIG_BLOCK halConfigBlock = (HAL_CONFIG_BLOCK) MFConfigHelper.UnmarshalData(numArray, typeof (HAL_CONFIG_BLOCK));
        this.m_cfgHash[(object) halConfigBlock.DriverNameString] = (object) new MFConfigHelper.ConfigIndexData(configurationLength, (int) halConfigBlock.Size + (int) length);
        configurationLength += (int) halConfigBlock.Size + (int) length;
        while (configurationLength % 4 != 0)
          ++configurationLength;
      }
      if (!dbgEngine.CheckSignature(new byte[128], 0U) && dbgEngine.ConnectionSource == ConnectionSource.TinyBooter)
        throw new MFConfigSectorWriteFailureException();
      if (dbgEngine.ConnectionSource != ConnectionSource.TinyBooter || !this.m_fRestartClr)
        return;
      dbgEngine.ExecuteMemory(0U);
    }

    internal void SwapAllConfigData(MFConfigHelper srcConfigHelper)
    {
      byte[] allCfgData = srcConfigHelper.m_all_cfg_data;
      if (allCfgData == null)
        throw new ArgumentNullException();
      if (this.m_all_cfg_data != null && this.m_all_cfg_data.Length != allCfgData.Length)
        throw new ArgumentException("Invalid swap target");
      this.m_all_cfg_data = allCfgData;
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
