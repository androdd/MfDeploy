// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFKeyConfig
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using dotNetMFCrypto;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFKeyConfig
  {
    internal const int PublicKeySize = 260;
    internal const int PrivateKeySize = 260;
    internal const int SignatureSize = 128;
    internal const int RandomSeedSize = 16;

    public void SaveKeyPair(KeyPair keyPair, string fileName)
    {
      using (FileStream fileStream = File.Create(fileName))
        new XmlSerializer(typeof (KeyPair)).Serialize((Stream) fileStream, (object) keyPair);
    }

    public KeyPair LoadKeyPair(string fileName)
    {
      using (FileStream fileStream = File.OpenRead(fileName))
        return new XmlSerializer(typeof (KeyPair)).Deserialize((Stream) fileStream) as KeyPair;
    }

    public KeyPair CreateEmptyKeyPair()
    {
      KeyPair emptyKeyPair = new KeyPair();
      emptyKeyPair.PrivateKey = new byte[260];
      emptyKeyPair.PublicKey = new byte[260];
      for (int index = 0; index < 260; ++index)
        emptyKeyPair.PrivateKey[index] = byte.MaxValue;
      for (int index = 0; index < 260; ++index)
        emptyKeyPair.PublicKey[index] = byte.MaxValue;
      return emptyKeyPair;
    }

    public KeyPair CreateKeyPair()
    {
      byte[] numArray1 = new byte[20];
      byte[] numArray2 = new byte[260];
      byte[] pubkey = new byte[260];
      Random random = new Random();
      for (int index = 0; index < 100; ++index)
      {
        random.NextBytes(numArray1);
        ushort delta1;
        ushort delta2;
        if (CryptoWrapper.Crypto_CreateZenithKey(numArray1, out delta1, out delta2) == 0)
        {
          byte[] bytes1 = BitConverter.GetBytes(delta1);
          byte[] bytes2 = BitConverter.GetBytes(delta2);
          numArray1[16] = bytes1[0];
          numArray1[17] = bytes1[1];
          numArray1[18] = bytes2[0];
          numArray1[19] = bytes2[1];
          if (CryptoWrapper.Crypto_GeneratePrivateKey(numArray1, numArray2) == 0 && CryptoWrapper.Crypto_PublicKeyFromPrivate(numArray2, pubkey) == 0)
            return new KeyPair()
            {
              PrivateKey = numArray2,
              PublicKey = pubkey
            };
        }
      }
      throw new ApplicationException("Could not generate key pair");
    }

    public byte[] SignData(byte[] data, byte[] keyPrivate)
    {
      byte[] signature = new byte[128];
      bool flag = true;
      for (int index = 0; index < keyPrivate.Length; ++index)
      {
        if (keyPrivate[index] != byte.MaxValue)
        {
          flag = false;
          break;
        }
      }
      if (flag)
        return signature;
      int num;
      do
        ;
      while ((num = CryptoWrapper.Crypto_SignBuffer(data, data.Length, keyPrivate, signature, signature.Length)) == 1);
      if (num != 0)
        throw new ApplicationException("Could not sign data");
      return signature;
    }

    public void UpdateDeviceKey(
      MFDevice device,
      PublicKeyUpdateInfo.KeyIndex index,
      byte[] publicKey)
    {
      this.UpdateDeviceKey(device, index, publicKey, (byte[]) null);
    }

    public void UpdateDeviceKey(
      MFDevice device,
      PublicKeyUpdateInfo.KeyIndex index,
      byte[] publicKey,
      byte[] publicKeySignature)
    {
      MFConfigHelper mfConfigHelper = new MFConfigHelper(device);
      mfConfigHelper.UpdatePublicKey(new PublicKeyUpdateInfo()
      {
        NewPublicKey = publicKey,
        NewPublicKeySignature = publicKeySignature,
        PublicKeyIndex = index
      });
      mfConfigHelper.Dispose();
    }

    public string FormatPublicKey(KeyPair keyPair)
    {
      int num1 = 0;
      int num2 = 0;
      string str = "// exponent length\r\n";
      foreach (byte num3 in keyPair.PublicKey)
      {
        if (num1 == 4)
        {
          str += "\r\n\r\n// module\r\n";
          num2 = 0;
        }
        if (num2 != 0 && num2 % 10 == 0)
          str += "\r\n";
        if (num1 == 132)
        {
          str += "\r\n\r\n// exponent\r\n";
          num2 = 0;
        }
        str += string.Format("0x{0:x02}, ", (object) num3);
        ++num2;
        ++num1;
      }
      return str;
    }
  }
}
