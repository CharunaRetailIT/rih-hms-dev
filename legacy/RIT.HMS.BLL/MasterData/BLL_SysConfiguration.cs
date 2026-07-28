using RIT.HMS.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_SysConfiguration
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_SysConfiguration()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_SysConfiguration(string actualdb)
        {
            _unitofwork = new UnitOfWork(actualdb);
        }
        public string GetServerName()
            {
            var ssss = _unitofwork.SysConfigurationRepository.Get();
           
                var encryptedsysname = _unitofwork.SysConfigurationRepository.Get().FirstOrDefault().SysName;
            string decryptedsysname = string.Empty;

            if (!string.IsNullOrEmpty(encryptedsysname))
            {
                // LEGACY-SECRET-SCRUBBED: original literal key was reused as the SQL Server SA
                // password and the HMSKeyGenerator TripleDES key. Removed for fork hygiene.
                // See /SECURITY.md. v2 cloud uses Azure Key Vault.
                var sysConfigKey = Environment.GetEnvironmentVariable("HMS_SYSCONFIG_KEY") ?? "__LEGACY_SECRET_REMOVED__";
                decryptedsysname = Decrypt(sysConfigKey, encryptedsysname);
                if(!string.IsNullOrEmpty(decryptedsysname))
                decryptedsysname = decryptedsysname.Substring(6, decryptedsysname.Length - 6);

            }


            return decryptedsysname;
        }


        public string Decrypt(string key, string data)
        {
            string decData = null;
            byte[][] keys = GetHashKeys(key);

            try
            {
                decData = DecryptStringFromBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return decData;
        }
        private byte[][] GetHashKeys(string key)
        {
            byte[][] result = new byte[2][];
            Encoding enc = Encoding.UTF8;

            SHA256 sha2 = new SHA256CryptoServiceProvider();

            byte[] rawKey = enc.GetBytes(key);
            byte[] rawIV = enc.GetBytes(key);

            byte[] hashKey = sha2.ComputeHash(rawKey);
            byte[] hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            result[0] = hashKey;
            result[1] = hashIV;

            return result;
        }

        private static string DecryptStringFromBytes_Aes(string cipherTextString, byte[] Key, byte[] IV)
        {
            byte[] cipherText = null;
            try
            {
                cipherText = Convert.FromBase64String(cipherTextString);
            }
            catch (FormatException e) { }

            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt =
                            new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}
