using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace RemoverWOW6432
{
    class U
    {
        static public void RestorePermission(String BadCrap)
        {
            Process.Start("cmd.exe", " /C icacls \"" + BadCrap + "\" /Q /C /RESET").WaitForExit();
        }
        static public void RecursiveRestorePermission(String BadCrap)
        {
            Process.Start("cmd.exe", " /C icacls \"" + BadCrap + "\" /T /Q /C /RESET").WaitForExit();
        }
        static public void FDelete(String AFile)
        {
            try
            {
                File.SetAttributes(AFile, FileAttributes.Normal);
                File.Delete(AFile);
            }
            catch { }
        }
        static public void ResetShells()
        {
            RegistryKey Users = Registry.Users;
            foreach (String User in Users.GetSubKeyNames())
            {
                try
                {
                    RegistryKey Winlogon = Users.OpenSubKey(User + @"\Software\WOW6432Node\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
                    if (Winlogon != null && Winlogon.GetValue("shell") != null)
                    {
                        String[] Shells = Winlogon.GetValue("shell").ToString().Split(',');
                        foreach (String Shell in Shells)
                        {
                            if (Shell != null
                                && Shell.ToLower().Trim() != "explorer.exe"
                                && !Shell.ToLower().Trim().EndsWith(@"windows\system32\explorer.exe")
                                && !Shell.ToLower().Trim().EndsWith(@"windows\syswow64\explorer.exe")
                                && Shell != ""
                                && Shell != ","
                                && Shell != " "
                                /*&& File.Exists(Shell.Replace("\"", "").Trim())*/)
                            {
                                String Shell_ = Shell.Replace("\"", "").Trim();
                                RestorePermission(Directory.GetParent(Shell_).ToString());
                                RestorePermission(Shell_);
                                FDelete(Shell_);
                                Console.WriteLine("Shell32 " + Shell_);
                            }
                        }
                        Winlogon.SetValue("shell", "explorer.exe");
                    }
                }
                catch { continue; }
            }
        }
        static public void FixAV()
        {
            foreach (String dir in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)))
            {
                try
                {
                    String[] eee = Directory.GetFiles(dir);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("AV32 " + dir);
                    RecursiveRestorePermission(dir);
                    continue;
                }
            }
            foreach (String dir in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)))
            {
                try
                {
                    String[] eee = Directory.GetFiles(dir);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("AV " + dir);
                    RecursiveRestorePermission(dir);
                    continue;
                }
            }

        }
        static public void ResetIFEO()
        {
            Process.Start("subinacl.exe", " /subkeyreg \"HKEY_LOCAL_MACHINE\\Software\\WOW6432Node\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\" /revoke=everyone").WaitForExit();
            RegistryKey IFEOkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Image File Execution Options", true);
            foreach (String IFEO in IFEOkey.GetSubKeyNames())
            {
                        if (!IFEO.ToLower().StartsWith("ieinstal.exe")
                        && !IFEO.ToLower().StartsWith("dllnxoptions")
                        && !IFEO.ToLower().StartsWith("{applicationverifierglobalsettings}")
                        )
                        {
                            Console.Write("Reset " + IFEO +"  ");
                            try { IFEOkey.DeleteSubKeyTree(IFEO); }
                            catch { continue;  }
                        }
            }
        }
        static public void RemoveMisc()
        {
            RegistryKey Users = Registry.Users;
            foreach (String User in Users.GetSubKeyNames())
            {
                try
                {
                    RegistryKey IMESubkey = Users.OpenSubKey(User + @"\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\ime", true);
                    if (IMESubkey != null && IMESubkey.GetValueNames() != null)
                    {
                        String[] IMEs = IMESubkey.GetValueNames();
                        foreach (String IME in IMEs)
                        {
                            if(IME != null)
                            {
                                String IME_ = IMESubkey.GetValue(IME).ToString().Replace("\"", "").Trim();
                                RestorePermission(Directory.GetParent(IME_).ToString());
                                RestorePermission(IME_);
                                FDelete(IME_);
                                IMESubkey.DeleteValue(IME);
                                Console.WriteLine("IME32 " + IME_);
                            }
                        }
                    }
                }
                catch { continue; }
                try
                {
                    RegistryKey SBSubkey = Users.OpenSubKey(User + @"\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Sidebar", true);
                    if (SBSubkey != null && SBSubkey.GetValueNames() != null)
                    {
                        String[] SBs = SBSubkey.GetValueNames();
                        foreach (String SB in SBs)
                        {
                            if (SB != null)
                            {
                                String SB_ = SBSubkey.GetValue(SB).ToString().Replace("\"", "").Trim();
                                RestorePermission(Directory.GetParent(SB_).ToString());
                                RestorePermission(SB_);
                                FDelete(SB_);
                                SBSubkey.DeleteValue(SB);
                                Console.WriteLine("Sidebar32 " + SB_);
                            }
                        }
                    }

                }
                catch { continue; }
            }
        }
    }
}
