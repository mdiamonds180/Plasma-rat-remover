using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Remover
{
    class U
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);
        static public void RestorePermission(String BadCrap)
        {
            Process.Start("cmd.exe", " /C icacls \"" + BadCrap + "\" /Q /C /RESET").WaitForExit();
        }
        static public void RecursiveRestorePermission(String BadCrap)
        {
            Process.Start("cmd.exe", " /C icacls \"" + BadCrap + "\" /T /Q /C /RESET").WaitForExit();
        }
        static public void KillAll()
        {
            Process I = Process.GetCurrentProcess();
            foreach (Process Proc in Process.GetProcesses())
            {
                if (Proc.Id != I.Id
                        && !Proc.ProcessName.ToLower().StartsWith("winlogon")
                        && !Proc.ProcessName.ToLower().StartsWith("system idle process")
                        && !Proc.ProcessName.ToLower().StartsWith("spoolsv")
                        && !Proc.ProcessName.ToLower().StartsWith("csrss")
                        && !Proc.ProcessName.ToLower().StartsWith("smss")
                        && !Proc.ProcessName.ToLower().StartsWith("svchost")
                        && !Proc.ProcessName.ToLower().StartsWith("services")
                        && !Proc.ProcessName.ToLower().StartsWith("lsass")
                        && !Proc.ProcessName.ToLower().StartsWith("lsm")
                        && !Proc.ProcessName.ToLower().StartsWith("wininit")
                        && !Proc.ProcessName.ToLower().StartsWith("cmd")
                        && !Proc.ProcessName.ToLower().StartsWith("conhost")
                        && !Proc.ProcessName.ToLower().StartsWith("comhost")
                        && !Proc.ProcessName.ToLower().StartsWith("ntkrnlpa")
                        && Proc.ProcessName.ToLower() != "system"
                )
                {
                    try
                    {
                        int ic = 0;
                        int bkot = 0x1D;
                        uint WM_DESTROY = 0x2;
                        //Fuck BSoD
                        NtSetInformationProcess(Proc.Handle, bkot, ref ic, sizeof(int));
                        if (Proc.MainWindowHandle != IntPtr.Zero)
                        {
                            SendMessage(Proc.MainWindowHandle, WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
                        }
                        try
                        {
                            Proc.Kill();
                        }
                        catch { continue; }
                    }
                    catch { continue; }
                }
            }

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
                    RegistryKey Winlogon = Users.OpenSubKey(User + @"\Software\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
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
                                Console.WriteLine("Shell " + Shell_);
                                RestorePermission(Directory.GetParent(Shell_).ToString());
                                RestorePermission(Shell_);
                                FDelete(Shell_);
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
            foreach (String dir in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)))
            {
                try
                {
                    String[] eee = Directory.GetFiles(dir);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("AV "+dir);
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
                    Console.WriteLine("AV " +dir);
                    RecursiveRestorePermission(dir);
                    continue;
                }
            }
        }
        static public void ResetIFEO()
        {
            Process.Start("subinacl.exe", " /subkeyreg \"HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\" /revoke=everyone").WaitForExit();
            RegistryKey IFEOkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options", true);
            foreach (String IFEO in IFEOkey.GetSubKeyNames())
            {
                        if (!IFEO.ToLower().StartsWith("ieinstal.exe")
                        && !IFEO.ToLower().StartsWith("dllnxoptions")
                        && !IFEO.ToLower().StartsWith("{applicationverifierglobalsettings}")
                        )
                        {
                            Console.Write("Reset IFEO " + IFEO +"  ");
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
                    RegistryKey IMESubkey = Users.OpenSubKey(User + @"\Software\Microsoft\Windows\CurrentVersion\ime", true);
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
                                Console.WriteLine("IME " + IME_);
                                IMESubkey.DeleteValue(IME);
                            }
                        }
                    }
                }
                catch { continue; }
                try
                {
                    RegistryKey SBSubkey = Users.OpenSubKey(User + @"\Software\Microsoft\Windows\CurrentVersion\Sidebar", true);
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
                                Console.WriteLine("Sidebar " + SB_);
                                SBSubkey.DeleteValue(SB);
                            }
                        }
                    }

                }
                catch { continue; }
            }
        }
    }
}
