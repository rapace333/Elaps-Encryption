using ElapsEncryption;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


public static class RecentFilesCleaner
{

    public static void RemoveFileFromRecentFiles(string targetFilePath)
    {
        // Répertoire des fichiers récents
        string recentFilesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Recent));

        // Parcourir chaque fichier .lnk dans le répertoire des fichiers récents
        foreach (var shortcut in Directory.GetFiles(recentFilesDirectory, "*.lnk"))
        {
            string shortcutTarget = GetShortcutTarget(shortcut);

            // Si le raccourci pointe vers le fichier cible, le supprimer
            if (string.Equals(shortcutTarget, targetFilePath, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    EncryptionCore.SecureDeleteFile(shortcut);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when trying to delete {shortcut}: {ex.Message}");
                }
            }
        }
    }

    public static string GetShortcutTarget(string file)
    {
        StringBuilder targetPath = new StringBuilder(260); // MAX_PATH est de 260 caractères.

        // CLSID_ShellLink {00021401-0000-0000-C000-000000000046}
        Type shellLinkType = Type.GetTypeFromCLSID(new Guid("{00021401-0000-0000-C000-000000000046}"));
        IShellLinkW shellLink = (IShellLinkW)Activator.CreateInstance(shellLinkType);

        try
        {
            // IPersistFile : Pour charger le fichier .lnk
            var persistFile = (IPersistFile)shellLink;
            persistFile.Load(file, 0); // Charge le fichier .lnk

            // Récupère le chemin cible du raccourci
            shellLink.GetPath(targetPath, targetPath.Capacity, IntPtr.Zero, 0);
        }
        finally
        {
            Marshal.ReleaseComObject(shellLink);
        }

        return targetPath.ToString();
    }

    [ComImport]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellLinkW
    {
        void GetPath([Out] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, int fFlags);
    }

    [ComImport]
    [Guid("0000010B-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        void IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
    }
}


