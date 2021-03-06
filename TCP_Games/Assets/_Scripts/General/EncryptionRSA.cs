using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;


class EncryptionRSA
{
    private RSAParameters   m_privateKey;
    private RSAParameters   m_publicKey;
    private static int      MAXENCRYPTSIZE = 100;

    public RSAParameters PrivateKey
    {
        get => m_privateKey;
        set => m_privateKey = value;
    }
    public RSAParameters PublicKey
    {
        get => m_publicKey;
        set => m_publicKey = value;
    }

    public EncryptionRSA()
    {

    }

    public string Encrypt(string dataText, RSAParameters key)
    {
        if (key.Equals(null))
        {
            Console.WriteLine("Error Encrypting RSA : Empty key!");
            return null;
        }

        try
        {
            var rCsp                = new RSACryptoServiceProvider();
            rCsp.ImportParameters(key);
            byte[] byteData         = Encoding.Unicode.GetBytes(dataText);
            int readPos             = 0;
            string encryptedData    = string.Empty;

            while (byteData.Length - readPos > 0)
            {
                byte[] splitToEncrypt = new byte[MAXENCRYPTSIZE];

                if (byteData.Length - (readPos + MAXENCRYPTSIZE) > 0)
                {
                    Array.Copy(byteData, readPos, splitToEncrypt, 0, 100);
                    readPos += MAXENCRYPTSIZE;
                }
                else
                {
                    Array.Copy(byteData, readPos, splitToEncrypt, 0, byteData.Length - readPos);
                    readPos += byteData.Length - readPos;
                }

                byte[] encryptedByte = rCsp.Encrypt(splitToEncrypt, false);
                encryptedData += Convert.ToBase64String(encryptedByte);
                encryptedData += "|";
            }

            return encryptedData;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error encrypting : " + e.Message);
        }

        return null;
    }
    public string Decrypt(string dataCypher, RSAParameters key)
    {
        if (key.Equals(null))
        {
            Console.WriteLine("Error Decrypting RSA : Empty key!");
            return null;
        }

        try
        {
            var rCsp                = new RSACryptoServiceProvider();
            rCsp.ImportParameters(key);
            string[] splitData      = dataCypher.Split('|');
            string dataDecrypted    = string.Empty;

            for (int i = 0; i < splitData.Length - 1; i++)
            {
                byte[] dataByte      = Convert.FromBase64String(splitData[i]);
                byte[] dataPlain     = rCsp.Decrypt(dataByte, false);
                dataDecrypted       += Encoding.Unicode.GetString(dataPlain);
            }

            return dataDecrypted;
        }
        catch (Exception e)
        {
            Console.WriteLine("Decryption server error : " + e.Message);
        }

        return null;
    }

    public string ConvertKeyToString(RSAParameters key)
    {
        try
        {
            StringWriter sw     = new StringWriter();
            XmlSerializer xs    = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, key);
            return sw.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error Convert Key to String : " + e.Message);
        }
        return null;
    }
    public RSAParameters ConvertStringToKey(String key)
    {
        try
        {
            StringReader reader     = new StringReader(key);
            XmlSerializer xs        = new XmlSerializer(typeof(RSAParameters));
            RSAParameters newKey    = (RSAParameters)xs.Deserialize(reader);

            return newKey;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error Convert String to Key : " + e.Message);
        }

        RSAParameters emptyKey      = new RSAParameters();
        return emptyKey;
    }
}

