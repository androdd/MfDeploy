// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFWirelessConfiguration
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using dotNetMFCrypto;
using System;
using System.Globalization;
using System.Text;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFWirelessConfiguration
  {
    private const string c_CfgName = "WIRELESS";
    private const int c_EncryptionBitShift = 4;
    private const int c_RadioBitShift = 8;
    private const int c_DataBitShift = 12;
    private HAL_WirelessConfiguration m_cfg = new HAL_WirelessConfiguration();
    public MFConfigHelper m_cfgHelper;

    public MFWirelessConfiguration(MFDevice dev) => this.m_cfgHelper = new MFConfigHelper(dev);

    public int Authentication
    {
      get => (int) this.m_cfg.WirelessFlags & 15;
      set
      {
        this.m_cfg.WirelessFlags &= 4294967280U;
        this.m_cfg.WirelessFlags |= (uint) (value & 15);
      }
    }

    public int Encryption
    {
      get => (int) (this.m_cfg.WirelessFlags >> 4) & 15;
      set
      {
        this.m_cfg.WirelessFlags &= 4294967055U;
        this.m_cfg.WirelessFlags |= (uint) ((value & 15) << 4);
      }
    }

    public int Radio
    {
      get => (int) (this.m_cfg.WirelessFlags >> 8) & 15;
      set
      {
        this.m_cfg.WirelessFlags &= 4294963455U;
        this.m_cfg.WirelessFlags |= (uint) ((value & 15) << 8);
      }
    }

    public bool UseEncryption
    {
      get => ((int) (this.m_cfg.WirelessFlags >> 12) & 1) != 0;
      set
      {
        this.m_cfg.WirelessFlags &= 4294905855U;
        if (value)
          this.m_cfg.WirelessFlags |= 4096U;
        else
          this.m_cfg.WirelessFlags &= 4294963199U;
      }
    }

    public unsafe string PassPhrase
    {
      get
      {
        string passPhrase = "";
        bool flag = true;
        fixed (byte* numPtr = this.m_cfg.PassPhrase)
        {
          if (this.UseEncryption)
          {
            byte[] cypher = new byte[64];
            for (int index = 0; index < 64; ++index)
            {
              cypher[index] = numPtr[index];
              if (cypher[index] != byte.MaxValue)
                flag = false;
            }
            if (!flag)
            {
              byte[] bytes = this.Decrypt(cypher);
              int count = 0;
              while (count < 64 && bytes[count] != (byte) 0)
                ++count;
              passPhrase = new string(Encoding.UTF8.GetChars(bytes, 0, count));
            }
          }
          else
          {
            for (int index = 0; index < 64; ++index)
            {
              if (numPtr[index] != byte.MaxValue)
              {
                flag = false;
                break;
              }
            }
            if (!flag)
              passPhrase = new string((sbyte*) numPtr);
          }
        }
        return passPhrase;
      }
      set
      {
        string s = value;
        if (s.Length >= 64)
          throw new ArgumentOutOfRangeException();
        foreach (char ch in s)
        {
          if (ch > 'ÿ')
            throw new ArgumentException("Pass phrase cannot have wide characters.");
        }
        fixed (byte* numPtr = this.m_cfg.PassPhrase)
        {
          byte[] bytes = Encoding.UTF8.GetBytes(s);
          byte[] numArray = new byte[64];
          int length = Math.Min(63, bytes.Length);
          Array.Copy((Array) bytes, (Array) numArray, length);
          for (; length < 64; ++length)
            numArray[length] = (byte) 0;
          if (this.UseEncryption)
            numArray = this.Encrypt(numArray);
          for (int index = 0; index < 64; ++index)
            numPtr[index] = numArray[index];
        }
      }
    }

    public int NetworkKeyLength
    {
      get => this.m_cfg.NetworkKeyLength;
      set => this.m_cfg.NetworkKeyLength = value;
    }

    public unsafe string NetworkKey
    {
      get
      {
        fixed (byte* data = this.m_cfg.NetworkKey)
          return this.ByteToString(data, this.m_cfg.NetworkKeyLength, this.UseEncryption);
      }
      set
      {
        fixed (byte* data = this.m_cfg.NetworkKey)
          this.StringToByte(value, data, 256, this.UseEncryption);
      }
    }

    public int ReKeyLength
    {
      get => this.m_cfg.ReKeyLength;
      set => this.m_cfg.ReKeyLength = value;
    }

    public unsafe string ReKeyInternal
    {
      get
      {
        fixed (byte* data = this.m_cfg.ReKeyInternal)
          return this.ByteToString(data, this.m_cfg.ReKeyLength, this.UseEncryption);
      }
      set
      {
        fixed (byte* data = this.m_cfg.ReKeyInternal)
          this.StringToByte(value, data, 32, this.UseEncryption);
      }
    }

    private unsafe string ByteCharsToString(byte* chars, int length)
    {
      string str = "";
      bool flag = true;
      for (int index = 0; index < length; ++index)
      {
        if (chars[index] == (byte) 0)
          length = index;
        if (chars[index] != byte.MaxValue)
          flag = false;
      }
      if (!flag)
        str = new string((sbyte*) chars);
      return str;
    }

    private unsafe void StringToByteChars(byte* chars, int charsLen, string data)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(data);
      int num = Math.Min(charsLen, bytes.Length);
      int index;
      for (index = 0; index < num; ++index)
        chars[index] = bytes[index];
      for (; index < charsLen; ++index)
        chars[index] = (byte) 0;
    }

    public unsafe string SSID
    {
      get
      {
        fixed (byte* chars = this.m_cfg.SSID)
          return this.ByteCharsToString(chars, 32);
      }
      set
      {
        fixed (byte* chars = this.m_cfg.SSID)
          this.StringToByteChars(chars, 32, value);
      }
    }

    public void Load()
    {
      byte[] config = this.m_cfgHelper.FindConfig("WIRELESS");
      if (config == null)
        return;
      this.m_cfg = (HAL_WirelessConfiguration) MFConfigHelper.UnmarshalData(config, typeof (HAL_WirelessConfiguration));
    }

    public void Save()
    {
      this.m_cfg.WirelessNetworkCount = 1;
      this.m_cfg.Enabled = 1;
      this.m_cfgHelper.WriteConfig("WIRELESS", (IHAL_CONFIG_BASE) this.m_cfg, false);
    }

    private unsafe string ByteToString(byte* data, int length, bool decrypt)
    {
      string str = "";
      bool flag = true;
      if (decrypt)
      {
        byte[] cypher = new byte[length];
        for (int index = 0; index < length; ++index)
        {
          cypher[index] = data[index];
          if (cypher[index] != byte.MaxValue)
            flag = false;
        }
        byte[] numArray = this.Decrypt(cypher);
        for (int index = 0; index < length; ++index)
          str += string.Format("{0:x02}", (object) numArray[index]);
      }
      else
      {
        for (int index = 0; index < length; ++index)
        {
          if (data[index] != byte.MaxValue)
            flag = false;
          str += string.Format("{0:x02}", (object) data[index]);
        }
      }
      return flag ? "" : str;
    }

    private byte[] Encrypt(byte[] data)
    {
      byte[] deploymentPublicKey = this.m_cfgHelper.DeploymentPublicKey;
      byte[] IV = new byte[16];
      byte[] pCypherText = new byte[data.Length];
      CryptoWrapper.Crypto_Encrypt(deploymentPublicKey, IV, IV.Length, data, data.Length, pCypherText, pCypherText.Length);
      return pCypherText;
    }

    private byte[] Decrypt(byte[] cypher)
    {
      byte[] deploymentPublicKey = this.m_cfgHelper.DeploymentPublicKey;
      byte[] IV = new byte[16];
      byte[] pPlainText = new byte[cypher.Length];
      CryptoWrapper.Crypto_Decrypt(deploymentPublicKey, IV, IV.Length, cypher, cypher.Length, pPlainText, pPlainText.Length);
      return pPlainText;
    }

    private unsafe void StringToByte(string stringForm, byte* data, int length, bool encrypt)
    {
      stringForm = stringForm.Replace(" ", "");
      if (1 == (stringForm.Length & 1))
        stringForm += "0";
      int num = Math.Min(stringForm.Length / 2, length);
      byte[] data1 = new byte[length];
      int index1;
      for (index1 = 0; index1 < num; ++index1)
        data1[index1] = byte.Parse(stringForm.Substring(2 * index1, 2), NumberStyles.HexNumber);
      for (; index1 < length; ++index1)
        data1[index1] = (byte) 0;
      if (encrypt)
        data1 = this.Encrypt(data1);
      for (int index2 = 0; index2 < length; ++index2)
        data[index2] = data1[index2];
    }

    [Flags]
    public enum RadioTypes
    {
      a = 1,
      b = 2,
      g = 4,
      n = 8,
    }
  }
}
