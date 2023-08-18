// Decompiled with JetBrains decompiler
// Type: dotNetMFCrypto.CryptoWrapper
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using System.Runtime.InteropServices;

namespace dotNetMFCrypto
{
  public sealed class CryptoWrapper
  {
    [DllImport("Crypto.dll")]
    public static extern int Crypto_CreateZenithKey(
      [In] byte[] seed,
      out ushort delta1,
      out ushort delta2);

    [DllImport("Crypto.dll")]
    public static extern int Crypto_SignBuffer(
      [In] byte[] buffer,
      int bufLen,
      [In] byte[] key,
      [Out] byte[] signature,
      int siglen);

    [DllImport("Crypto.dll")]
    public static extern int Crypto_GeneratePrivateKey([In] byte[] seed, [Out] byte[] privateKey);

    [DllImport("Crypto.dll")]
    public static extern int Crypto_PublicKeyFromPrivate([In] byte[] privkey, [Out] byte[] pubkey);

    public enum CRYPTO_RESULT
    {
      FAILURE = -9, // 0xFFFFFFF7
      ACTIVATIONBADCONTROLCHAR = -8, // 0xFFFFFFF8
      ACTIVATIONBADSYNTAX = -7, // 0xFFFFFFF9
      NOMEMORY = -6, // 0xFFFFFFFA
      UNKNOWNERROR = -5, // 0xFFFFFFFB
      UNKNOWNKEY = -4, // 0xFFFFFFFC
      KEYEXPIRED = -3, // 0xFFFFFFFD
      BADPARMS = -2, // 0xFFFFFFFE
      SIGNATUREFAIL = -1, // 0xFFFFFFFF
      SUCCESS = 0,
      CONTINUE = 1,
    }

    public enum RSA_OPS
    {
      ENCRYPT,
      DECRYPT,
      VERIFYSIGNATURE,
    }
  }
}
