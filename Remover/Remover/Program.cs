using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Remover
{
    class Program
    {
        static void Main(string[] args)
        {
            Process.EnterDebugMode();
            U.KillAll();
            U.ResetShells();
            if (System.IO.File.Exists("subinacl.exe"))
            {
                U.ResetIFEO();
            }
            else
            {
                Console.WriteLine("subinacl not found, unable to fix ifeo");
            }
            U.RemoveMisc();
            U.FixAV();
            if (Environment.Is64BitOperatingSystem)
            {
                RemoverWOW6432.U.ResetShells();
                if (System.IO.File.Exists("subinacl.exe"))
                {
                    RemoverWOW6432.U.ResetIFEO();
                }
                RemoverWOW6432.U.RemoveMisc();
                RemoverWOW6432.U.FixAV();
            }
            Process.LeaveDebugMode();
            Console.ReadKey(true);
        }
    }
}
