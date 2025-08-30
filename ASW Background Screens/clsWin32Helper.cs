using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ASW_Background_Screens
{

    static class clsWin32Helper
    {
        public const int GW_OWNER = 4;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll")] public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static bool IsDialogClass(IntPtr hWnd)
        {
            var sb = new StringBuilder(256);
            GetClassName(hWnd, sb, sb.Capacity);
            // Classic dialogs are "#32770"
            return sb.ToString() == "#32770";
        }

        public static IntPtr FindFolderDialogOwnedBy(IntPtr owner)
        {
            var currentPid = (uint)Process.GetCurrentProcess().Id;
            IntPtr found = IntPtr.Zero;

            EnumWindows((h, l) =>
            {
                if (!IsWindowVisible(h)) return true;

                // Only windows in THIS process
                uint pid;
                GetWindowThreadProcessId(h, out pid);
                if (pid != currentPid) return true;

                // Only classic dialog windows
                if (!IsDialogClass(h)) return true;

                // Must be owned by the owner form
                var ownerWnd = GetWindow(h, GW_OWNER);
                if (ownerWnd != owner) return true;

                found = h;
                return false; // stop enumeration
            }, IntPtr.Zero);

            return found;
        }

    }
}
