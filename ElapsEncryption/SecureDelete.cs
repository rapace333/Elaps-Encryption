using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


    public static class SecureDelete
    {
        public static void SecureDeleteFile(string filePath)
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
    }

