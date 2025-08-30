using ASW_Background_Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

static class clsWin32PositionHelper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    public static void PositionNextFolderBrowserDialog(IntPtr ownerHandle, int x, int y, int? width = null, int? height = null)
    {
        Console.WriteLine("start thread");

        // Poll for up to ~5 seconds
        Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                var hDlg = clsWin32Helper.FindFolderDialogOwnedBy(ownerHandle);
                if (hDlg != IntPtr.Zero)
                {
                    if (width.HasValue && height.HasValue)
                    {
                        // Resize + move
                        clsWin32Helper.SetWindowPos(hDlg, IntPtr.Zero, x, y, width.Value, height.Value,
                            clsWin32Helper.SWP_NOZORDER | clsWin32Helper.SWP_NOACTIVATE);
                    }
                    else
                    {
                        // Move only
                        clsWin32Helper.SetWindowPos(hDlg, IntPtr.Zero, x, y, 0, 0,
                            clsWin32Helper.SWP_NOSIZE | clsWin32Helper.SWP_NOZORDER | clsWin32Helper.SWP_NOACTIVATE);
                    }
                    break;
                }
                Thread.Sleep(50);
            }
        });

        Console.WriteLine("finished thread");
    }

    public static void CaptureFolderDialogBounds(IntPtr ownerHandle, Action<int, int, int, int> onFound)
    {
        Task.Run(() =>
        {
            IntPtr hDlg = IntPtr.Zero;
            RECT rect = new RECT();
            bool gotBounds = false;

            // Wait until dialog opens
            for (int i = 0; i < 50; i++)
            {
                hDlg = clsWin32Helper.FindFolderDialogOwnedBy(ownerHandle);
                if (hDlg != IntPtr.Zero)
                    break;
                Thread.Sleep(50);
            }

            if (hDlg == IntPtr.Zero) return; // didn't find it

            // Track until it closes
            while (clsWin32Helper.IsWindowVisible(hDlg))
            {
                if (GetWindowRect(hDlg, out rect))
                {
                    gotBounds = true;
                }
                Thread.Sleep(100);
            }

            if (gotBounds)
            {
                int x = rect.Left;
                int y = rect.Top;
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;

                // Callback to save to config
                onFound?.Invoke(x, y, width, height);
            }
        });
    }
}

