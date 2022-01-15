using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PVZ_plugin
{
    internal class Helper
    {
        /// <summary>
        /// Find PID order by process name.
        /// </summary>
        /// <param name="processName">Process name</param>
        /// <returns>PID，return 0 when find none</returns>
        internal static int GetProcessPidByName(string processName)
        {
            Process[] processes = Process.GetProcessesByName("PlantsVsZombies");
            foreach (Process process in processes)
            {
                return process.Id;
            }
            return 0;
        }

        /// <summary>
        /// Read an int from memory.
        /// </summary>
        /// <param name="baseAddress">Memory address</param>
        /// <param name="iPid">PID</param>
        /// <returns>Int value from memory reading</returns>
        public static int ReadMemoryValue(int baseAddress, int iPid)
        {
            try
            {
                byte[] buffer = new byte[4];
                //獲取緩沖區地址
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                //打開一個已存在的進程對象  0x1F0FFF 最高權限
                IntPtr hProcess = WinApi.OpenProcess(0x1F0FFF, false, iPid);
                //將制定內存中的值讀入緩沖區
                WinApi.ReadProcessMemory(hProcess, (IntPtr)baseAddress, byteAddress, 4, IntPtr.Zero);
                //關閉操作
                WinApi.CloseHandle(hProcess);
                //從非托管內存中讀取一個 32 位帶符號整數。
                return Marshal.ReadInt32(byteAddress);
            }
            catch
            {
                return 0;
            }
        }
    }
}
