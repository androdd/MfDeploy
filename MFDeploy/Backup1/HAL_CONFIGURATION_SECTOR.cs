﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.HAL_CONFIGURATION_SECTOR
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  internal struct HAL_CONFIGURATION_SECTOR
  {
    private const int c_MaxBootEntryFlags = 50;
    private const int c_BackwardsCompatibilityBufferSize = 88;
    internal uint ConfigurationLength;
    internal CONFIG_SECTOR_VERSION Version;
    internal unsafe fixed byte Buffer[88];
    internal unsafe fixed uint BooterFlagArray[50];
    internal SECTOR_BIT_FIELD SignatureCheck1;
    internal SECTOR_BIT_FIELD SignatureCheck2;
    internal SECTOR_BIT_FIELD SignatureCheck3;
    internal SECTOR_BIT_FIELD SignatureCheck4;
    internal SECTOR_BIT_FIELD SignatureCheck5;
    internal SECTOR_BIT_FIELD SignatureCheck6;
    internal SECTOR_BIT_FIELD SignatureCheck7;
    internal SECTOR_BIT_FIELD SignatureCheck8;
    internal TINYBOOTER_KEY_CONFIG PublicKeyFirmware;
    internal TINYBOOTER_KEY_CONFIG PublicKeyDeployment;
    internal OEM_MODEL_SKU OEM_Model_SKU;
    internal OEM_SERIAL_NUMBERS OemSerialNumbers;
    internal SECTOR_BIT_FIELD_TB CLR_ConfigData;
    internal HAL_CONFIG_BLOCK FirstConfigBlock;
  }
}
