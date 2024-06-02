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

        EncryptionCore encryptionCore = new EncryptionCore();
        private static readonly RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        static string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void OpenFolder(object sender, EventArgs e)
        {

            string folderPath = Path.Combine(appDirectory, "files");

            try
            {
                if (System.IO.Directory.Exists(folderPath))
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
                    Console.WriteLine("Le dossier spécifié n'existe pas : " + folderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Une erreur s'est produite : " + ex.Message);
            }
        }

        private void Encrypte(object sender, EventArgs e)
        {

            string password = InputBox.Show("Entrez votre mot de passe :", "Saisie du mot de passe");

            string decryptedFolderPath = Path.Combine(appDirectory, "files\\decrypted\\");

            try
            {
                if (Directory.Exists(decryptedFolderPath))
                {
                    string[] files = Directory.GetFiles(decryptedFolderPath);

                    foreach (string filePath in files)
                    {
                        string encryptedString = encryptionCore.EncryptString(Path.GetFileName(filePath), password).Replace("/", "$");

                        encryptionCore.EncryptFile(filePath, Path.Combine(appDirectory, "files\\encrypted\\" + encryptedString + ".ElapsEncryption"), password);
                    }
                }
                else
                {
                    Console.WriteLine("Le dossier spécifié n'existe pas : " + decryptedFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Une erreur s'est produite : " + ex.Message);
            }
        }

        private void Decrypte(object sender, EventArgs e)
        {

            string password = InputBox.Show("Entrez votre mot de passe :", "Saisie du mot de passe");

            string encryptedFolderPath = Path.Combine(appDirectory, "files\\encrypted\\");

            try
            {
                if (Directory.Exists(encryptedFolderPath))
                {
                    string[] files = Directory.GetFiles(encryptedFolderPath);

                    foreach (string filePath in files)
                    {
                        string decryptedString = encryptionCore.DecryptString(Path.GetFileNameWithoutExtension(filePath).Replace("$", "/"), password);
                        encryptionCore.DecryptFile(filePath, Path.Combine(appDirectory, "files\\decrypted\\" + decryptedString), password);
                    }
                }
                else
                {
                    Console.WriteLine("Le dossier spécifié n'existe pas : " + encryptedFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Une erreur s'est produite : " + ex.Message);
            }
        }

        private void RemoveDecryptedFile(object sender, EventArgs e)
        {


            string message = "Êtes-vous sûr de vouloir supprimer le fichier décrypté ?\n\n(Assurez-vous de l'avoir correctement sauvegardé)";
            string caption = "Confirmation de suppression";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Warning;

            DialogResult result = MessageBox.Show(message, caption, buttons, icon);

            if (result == DialogResult.Yes)
            {




                string decryptedFolderPath = Path.Combine(appDirectory, "files\\decrypted\\");
                string[] files = Directory.GetFiles(decryptedFolderPath);

                foreach (string filePath in files)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            encryptionCore.SecureDeleteFile(filePath);
                            Console.WriteLine("Le fichier a été sécurisé et supprimé.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erreur lors de la sécurisation et de la suppression du fichier : {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Le fichier spécifié n'existe pas.");
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
                buttonCancel.Text = "Annuler";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                textBox.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                textBox.PasswordChar = '*';
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
    }
}
