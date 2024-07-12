using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ElapsEncryption;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Runtime.InteropServices;


namespace ElapsEncryption
{
    public partial class Form1 : Form
    {
        private Timer opacityTimer;
        private const int transitionDuration = 500;
        private const int timerInterval = 20;
        private double targetOpacity = 1.0;
        private bool changesMade = false; // Variable to track unsaved changes

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        EncryptionCore encryptionCore = new EncryptionCore();
        private static readonly RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        static string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        static string fileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Elaps\\ElapsEncryption" );
        public Form1()
        {
            InitializeComponent();
            Opacity = 0;
        }

        private void StartOpacityTransition()
        {
            opacityTimer = new System.Windows.Forms.Timer();
            opacityTimer.Interval = timerInterval;
            opacityTimer.Tick += timer1_Tick;
            opacityTimer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartOpacityTransition();
        }


        private void OpenFolder(object sender, EventArgs e)
        {
            if(!Directory.Exists(Path.Combine(fileFolder, "files")))
            {
                Directory.CreateDirectory(Path.Combine(fileFolder, "files\\encrypted"));
                Directory.CreateDirectory(Path.Combine(fileFolder, "files\\decrypted"));
                System.IO.File.Copy(Path.Combine(appDirectory, "files\\README.txt"), Path.Combine(fileFolder, "files\\README.txt"));
            }

            string folderPath = Path.Combine(fileFolder, "files");

            try
            {
                if (Directory.Exists(folderPath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = folderPath
                    };

                    Process.Start(startInfo);
                }
                else
                {
                    Console.WriteLine("The specified folder does not exist : " + folderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error has occurred : " + ex.Message);
            }
        }

        private void Encrypt(object sender, EventArgs e)
        {

            string password = InputBox.Show("Enter your password:", "Password");

            string decryptedFolderPath = Path.Combine(fileFolder, "files\\decrypted\\");

            try
            {
                if (Directory.Exists(decryptedFolderPath))
                {
                    string[] files = Directory.GetFiles(decryptedFolderPath);

                    foreach (string filePath in files)
                    {
                        string encryptedString = encryptionCore.EncryptString(Path.GetFileName(filePath), password).Replace("/", "$");

                        encryptionCore.EncryptFile(filePath, Path.Combine(fileFolder, "files\\encrypted\\" + encryptedString + ".ElapsEncryption"), password);
                    }
                }
                else
                {
                    Console.WriteLine("The specified folder does not exist : " + decryptedFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error has occurred : " + ex.Message);
            }
        }

        private void Decrypt(object sender, EventArgs e)
        {

            string password = InputBox.Show("Enter your password:", "Password");

            string encryptedFolderPath = Path.Combine(fileFolder, "files\\encrypted\\");

            try
            {
                if (Directory.Exists(encryptedFolderPath))
                {
                    string[] files = Directory.GetFiles(encryptedFolderPath);

                    foreach (string filePath in files)
                    {
                        string decryptedString = encryptionCore.DecryptString(Path.GetFileNameWithoutExtension(filePath).Replace("$", "/"), password);
                        if (decryptedString != "*")
                        {
                            encryptionCore.DecryptFile(filePath, Path.Combine(fileFolder, "files\\decrypted\\" + decryptedString), password);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("The specified folder does not exist : " + encryptedFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error has occurred : " + ex.Message);
            }
        }

        private void RemoveDecryptedFile(object sender, EventArgs e)
        {


            string message = "Are you sure you want to delete the decrypted file?\n\n(Make sure you have backed it up correctly)";
            string caption = "Deletion confirmation";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Warning;

            DialogResult result = MessageBox.Show(message, caption, buttons, icon);

            if (result == DialogResult.Yes)
            {
                string decryptedFolderPath = Path.Combine(fileFolder, "files\\decrypted\\");
                string[] files = Directory.GetFiles(decryptedFolderPath);

                foreach (string filePath in files)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            encryptionCore.SecureDeleteFile(filePath);
                            Console.WriteLine("The file has been secured and deleted.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error securing and deleting file : {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The specified file does not exist.");
                    }
                }
            }
        }

        public static class InputBox
        {
            public static string Show(string prompt, string title)
            {
                Form form = new Form();
                Label label = new Label();
                TextBox textBox = new TextBox();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = prompt;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                textBox.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                textBox.PasswordChar = '•';
                textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new System.Drawing.Size(396, 107);
                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                DialogResult dialogResult = form.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    return textBox.Text;
                }
                else
                {
                    return null;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double opacityIncrement = timerInterval / (double)transitionDuration;
            Opacity += opacityIncrement;

            if (Opacity >= targetOpacity)
            {
                Opacity = targetOpacity;
                opacityTimer.Stop();
            }
        }
    }
}
