using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LauncherDK
{
    public class Cryptor
    {
        public static string Encrypt(string text)
        {
            try
            {
                string textToEncrypt = text;
                string ToReturn = "";
                string publickey = ANY_PUBLIC_KEY_HERE; // Your public key here, the old one was invalidated due to open source commit.
                string secretkey = ANY_SECRET_KEY_HERE; // Your secret key here, the old one was invalidated due to open source commit.
                byte[] secretkeyByte = { };
                secretkeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public static string Decrypt(string text)
        {
            try
            {
                string textToDecrypt = text;
                string ToReturn = "";
                string publickey = ANY_PUBLIC_KEY_HERE; // Your public key here, the old one was invalidated due to open source commit.
                string secretkey = ANY_SECRET_KEY_HERE; // Your secret key here, the old one was invalidated due to open source commit.
                byte[] privatekeyByte = { };
                privatekeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = { };
                publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
                inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    Encoding encoding = Encoding.UTF8;
                    ToReturn = encoding.GetString(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ae)
            {
                return "Error";
                //throw new Exception(ae.Message, ae.InnerException);
            }
        }
    }
}
