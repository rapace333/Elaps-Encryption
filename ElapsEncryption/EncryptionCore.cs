using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElapsEncryption
{
    internal class EncryptionCore
    {

        public void EncryptFile(string inputFile, string outputFile, string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = new Rfc2898DeriveBytes(password, salt, 10000).GetBytes(16);
                aes.IV = new byte[aes.BlockSize / 8];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(aes.IV);
                }

                using (var inputStream = System.IO.File.OpenRead(inputFile))
                using (var outputStream = System.IO.File.OpenWrite(outputFile))
                {
                    outputStream.Write(salt, 0, salt.Length);
                    outputStream.Write(aes.IV, 0, aes.IV.Length);
                    using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        inputStream.CopyTo(cryptoStream);
                    }
                }
            }
        }

        public void DecryptFile(string inputFile, string outputFile, string password)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[16];
            byte[] iv = new byte[16];

            using (var inputStream = System.IO.File.OpenRead(inputFile))
            {
                inputStream.Read(salt, 0, salt.Length);
                inputStream.Read(iv, 0, iv.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = new Rfc2898DeriveBytes(password, salt, 10000).GetBytes(16);
                    aes.IV = iv;

                    using (var outputStream = System.IO.File.OpenWrite(outputFile))
                    {
                        using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            cryptoStream.CopyTo(outputStream);
                        }
                    }
                }
            }
        }

        public void SecureDeleteFile(string filePath)
        {
            const int bufferSize = 1024;
            const int bytesToOverwrite = bufferSize; 

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                {
                    byte[] buffer = new byte[bufferSize];
                    long fileSize = fs.Length;

                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        for (long bytesWritten = 0; bytesWritten < fileSize;)
                        {
                            int bytesToWrite = (int)Math.Min(bytesToOverwrite, fileSize - bytesWritten);
                            rng.GetBytes(buffer);
                            fs.Write(buffer, 0, bytesToWrite);
                            fs.Flush();
                            bytesWritten += bytesToWrite;
                        }
                    }
                }

                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string EncryptString(string plainText, string password)
        {
            byte[] salt = GenerateRandomSalt();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using (Aes aesAlg = Aes.Create())
            {
                Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(passwordBytes, salt, 10000);

                aesAlg.Key = keyDerivation.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = keyDerivation.GetBytes(aesAlg.BlockSize / 8);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }

                    byte[] encryptedBytes = msEncrypt.ToArray();
                    byte[] resultBytes = new byte[salt.Length + encryptedBytes.Length];

                    Buffer.BlockCopy(salt, 0, resultBytes, 0, salt.Length);
                    Buffer.BlockCopy(encryptedBytes, 0, resultBytes, salt.Length, encryptedBytes.Length);

                    return Convert.ToBase64String(resultBytes);
                }
            }
        }

        public string DecryptString(string encryptedText, string password)
        {
            try
            {
                byte[] resultBytes = Convert.FromBase64String(encryptedText);
                byte[] salt = new byte[16];
                byte[] encryptedBytes = new byte[resultBytes.Length - salt.Length];

                Buffer.BlockCopy(resultBytes, 0, salt, 0, salt.Length);
                Buffer.BlockCopy(resultBytes, salt.Length, encryptedBytes, 0, encryptedBytes.Length);

                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                using (Aes aesAlg = Aes.Create())
                {
                    Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(passwordBytes, salt, 10000);

                    aesAlg.Key = keyDerivation.GetBytes(aesAlg.KeySize / 8);
                    aesAlg.IV = keyDerivation.GetBytes(aesAlg.BlockSize / 8);

                    using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                string result = srDecrypt.ReadToEnd();
                                Console.WriteLine("Successfully decrypted " + result);
                                return result;
                            }
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                // This exception is thrown when the decryption fails, typically due to an incorrect password.
                Console.WriteLine("Error: Incorrect password or corrupted data, when trying to decrypt " + encryptedText);
                return "*";
            }
            catch (Exception ex)
            {
                // This handles any other exceptions that may occur.
                Console.WriteLine("Error: " + ex.Message);
                return "*";
            }
        }
        private byte[] GenerateRandomSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
