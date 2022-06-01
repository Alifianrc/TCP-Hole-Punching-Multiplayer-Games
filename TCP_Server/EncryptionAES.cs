using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TCP_Server
{
    class EncryptionAES
    {
        private AesCryptoServiceProvider m_aesKey;
        byte[] m_keyIV = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public AesCryptoServiceProvider Key
        {
            get => m_aesKey;
            set => m_aesKey = value;
        }

        public EncryptionAES()
        {
            m_aesKey    = new AesCryptoServiceProvider();
            m_aesKey.IV = m_keyIV;
        }

        public void GenerateNewKey()
        {
            m_aesKey.GenerateKey();
        }

        public string Encrypt(string data)
        {
            try
            {
                ICryptoTransform transform = m_aesKey.CreateEncryptor();
                byte[] dataByte = Encoding.ASCII.GetBytes(data);
                byte[] encryptedByte = transform.TransformFinalBlock(dataByte, 0, dataByte.Length);
                string encryptedData = Convert.ToBase64String(encryptedByte);
                return encryptedData;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Encrypting AES : " + e.Message);
            }

            return null;
        }
        public string Decrypt(string data)
        {
            try
            {
                ICryptoTransform transform  = m_aesKey.CreateDecryptor();
                byte[] encryptedByte        = Convert.FromBase64String(data);
                byte[] decryptedByte        = transform.TransformFinalBlock(encryptedByte, 0, encryptedByte.Length);
                string decryptedData        = Encoding.ASCII.GetString(decryptedByte);
                return decryptedData;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Decrypting AES : " + e.Message);
            }

            return null;
        }

        public string ConvertKeyToString(AesCryptoServiceProvider key)
        {
            string strKey = Convert.ToBase64String(key.Key);
            return strKey;
        }
        public AesCryptoServiceProvider ConvertStringToKey(string key)
        {
            AesCryptoServiceProvider newKey  = new AesCryptoServiceProvider();
            newKey.Key                       = Convert.FromBase64String(Split64(key));
            newKey.IV                        = m_keyIV;
            return newKey;
        }

        private string Split64(string data)
        {
            string[] spt = data.Split("=");
            return spt[0] + "=";
        }
    }
}
