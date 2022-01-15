using PVZ_plugin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SetWindowTitle
{
    internal class Program
    {
        /// <summary>
        /// Set window title.
        /// </summary>
        /// <param name="hwnd">Window handle.</param>
        /// <param name="lPstring">New title name.</param>
        /// <returns>Modify result. <see langword="true" /> if modify successfully, <see langword="false" /> if modify failed.</returns>
        [DllImport("user32.dll")]
        private static extern bool SetWindowText(IntPtr hwnd, string lPstring);
    
        static void Main(string[] args)
        {
            // IntPtr ptrHwndCE = WinApi.FindWindow(null, "Cheat Engine 6.7");
            // bool b = SetWindowText(ptrHwndCE, "Hi there");
            int baseAddress = Marshal.ReadInt32(0x006A9EC0, 0) + 0x768;
            int offsetAddress = Marshal.ReadInt32(baseAddress, 0) + 0x5560;
            int value = Marshal.ReadInt32(offsetAddress, 0);
            Console.WriteLine(value);
            Console.ReadKey();
        }

    }
}
