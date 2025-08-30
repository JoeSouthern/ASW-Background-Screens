using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASW_Background_Screens
{
    public partial class Form1 : Form
    { 
        // ===== Screensaver P/Invoke =====
        private const uint SPI_GETSCREENSAVEACTIVE = 0x0010;
        private const uint SPI_SETSCREENSAVEACTIVE = 0x0011;
        private const uint SPI_GETSCREENSAVETIMEOUT = 0x000E;
        private const uint SPI_SETSCREENSAVETIMEOUT = 0x000F;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(
            uint uiAction, uint uiParam, ref uint pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(
            uint uiAction, uint uiParam, bool pvParam, uint fWinIni);

        // ===== IDesktopWallpaper COM interop =====
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int left, top, right, bottom; }

        [ComImport, Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDesktopWallpaper
        {
            void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID,
                              [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);
            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID);
            void GetMonitorDevicePathAt(uint monitorIndex,
                [MarshalAs(UnmanagedType.LPWStr)] out string monitorID);
            uint GetMonitorDevicePathCount();
            void GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorID, out RECT displayRect);
            void SetBackgroundColor(uint color);
            uint GetBackgroundColor();
            void SetPosition(int position); // DESKTOP_WALLPAPER_POSITION
            int GetPosition();
            void SetSlideshow(IntPtr /* IShellItemArray */ items);
            IntPtr GetSlideshow(); // IShellItemArray
            void SetSlideshowOptions(int options, uint slideshowTick);
            void GetSlideshowOptions(out int options, out uint slideshowTick);
            void AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitorID, int direction);
            int GetStatus(); // DESKTOP_SLIDESHOW_STATE
            bool Enable(bool enable);
        }

        [ComImport, Guid("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD")]
        class DesktopWallpaperCom { }

        // ===== Fields =====
        private IDesktopWallpaper _dw;
        private List<string> _monitorIds = new();
        private Dictionary<string, string> _originalWallpapers = new();
        private bool _origSaverActive;
        private uint _origSaverTimeout;
        private readonly Timer _timer = new();
        private readonly Random _rng = new();

        // Per-monitor folder selections
        private readonly Dictionary<string, string> _monitorDirs = new();
        private int _durationSeconds = 120; // change duration here or expose via UI

        public Form1()
        {
            //InitializeComponent();
            clsConfig.Initialize("MyConfig.cfg");

            Text = "Per-Monitor Wallpaper Rotator";
            Width = 520;
            Height = 260;

            // Simple UI
            var lbl = new Label
            {
                AutoSize = true,
                Left = 16,
                Top = 16,
                Text = "Duration (seconds) between changes:"
            };
            var num = new NumericUpDown { Left = 16, Top = 40, Width = 120, Minimum = 5, Maximum = 86400, Value = _durationSeconds };
            var btnStart = new Button { Left = 16, Top = 80, Width = 160, Height = 32, Text = "Choose Folders & Start" };
            var btnOnce = new Button { Left = 190, Top = 80, Width = 160, Height = 32, Text = "Apply Once Now" };
            var btnStop = new Button { Left = 360, Top = 80, Width = 120, Height = 32, Text = "Stop Timer" };
            var info = new Label
            {
                Left = 16,
                Top = 130,
                Width = 480,
                Height = 80,
                Text = "Tip: You’ll be prompted once per monitor to pick a folder.\r\n" +
                       "Images are randomized and not duplicated across screens each cycle.\r\n" +
                       "Closing the app restores original wallpapers and screensaver settings."
            };

            Controls.Add(lbl); Controls.Add(num); Controls.Add(btnStart); Controls.Add(btnOnce); Controls.Add(btnStop); Controls.Add(info);

            num.ValueChanged += (s, e) =>
            {
                _durationSeconds = (int)num.Value;
                _timer.Interval = Math.Max(5, _durationSeconds) * 1000;
            };
            btnStart.Click += (s, e) => 
            {
                _durationSeconds = (int)num.Value;
                if (EnsureInitAndFolders()) StartRotation(); 
            };
            btnOnce.Click += (s, e) => { if (EnsureInitAndFolders()) ApplyRandomWallpapersOnce(); };
            btnStop.Click += (s, e) => { _timer.Stop(); Text = "Per-Monitor Wallpaper Rotator (Stopped)"; };

            _timer.Tick += (s, e) => ApplyRandomWallpapersOnce();
        }

        private bool EnsureInitAndFolders()
        {
            try
            {
                if (_dw == null)
                {
                    _dw = (IDesktopWallpaper)new DesktopWallpaperCom();

                    // Enumerate monitors
                    uint count = _dw.GetMonitorDevicePathCount();
                    _monitorIds.Clear();
                    for (uint i = 0; i < count; i++)
                    {
                        _dw.GetMonitorDevicePathAt(i, out string id);
                        _monitorIds.Add(id);
                    }

                    if (_monitorIds.Count == 0)
                    {
                        MessageBox.Show("No monitors detected by IDesktopWallpaper.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // Save original wallpapers
                    _originalWallpapers.Clear();
                    foreach (var id in _monitorIds)
                    {
                        try { _originalWallpapers[id] = _dw.GetWallpaper(id); }
                        catch { _originalWallpapers[id] = null; }
                    }

                    // Save and disable screensaver
                    uint active = 0; SystemParametersInfo(SPI_GETSCREENSAVEACTIVE, 0, ref active, 0);
                    _origSaverActive = active != 0;

                    uint timeout = 0; SystemParametersInfo(SPI_GETSCREENSAVETIMEOUT, 0, ref timeout, 0);
                    _origSaverTimeout = timeout;

                    // Disable screensaver while running
                    SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 0, false, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                }

                // Pick folders per monitor if not chosen yet
                int iScreen = 0;
                foreach (var id in _monitorIds)
                {
                    if (_monitorDirs.ContainsKey(id)) continue;


                    using var fbd = new FolderBrowserDialog
                    {
                        //Description = $"Choose image folder for monitor:\r\n{id}\r\n(You can point multiple monitors to the same folder if you wish.)"
                         Description = "Choose a folder",
                        //SelectedPath = @"C:\Pictures";
                       SelectedPath = clsConfig.GetOption("Screen" + iScreen.ToString())
                    };

                    if (fbd.ShowDialog(this) != DialogResult.OK)
                    {
                        MessageBox.Show("Folder selection cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                    _monitorDirs[id] = fbd.SelectedPath;
                    clsConfig.SetOption("Screen" + iScreen.ToString(), fbd.SelectedPath);
                    iScreen++;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialization failed:\r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void StartRotation()
        {
            _timer.Stop();
            _timer.Interval = Math.Max(5, _durationSeconds) * 1000;
            ApplyRandomWallpapersOnce();
            _timer.Start();
            Text = $"Per-Monitor Wallpaper Rotator (Every {_durationSeconds}s)";
        }

        private void ApplyRandomWallpapersOnce()
        {
            try
            {
                // Collect candidates for each monitor
                var perMonitorFiles = new Dictionary<string, List<string>>();
                foreach (var id in _monitorIds)
                {
                    var dir = _monitorDirs[id];
                    var files = Directory.EnumerateFiles(dir)
                        .Where(f => HasImageExt(f))
                        .ToList();

                    if (files.Count == 0)
                        throw new InvalidOperationException($"No images found in folder:\r\n{dir}");

                    perMonitorFiles[id] = files;
                }

                // Choose unique images across monitors for this cycle
                var chosen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var picks = new Dictionary<string, string>(); // monitorId -> file path

                foreach (var id in _monitorIds)
                {
                    var files = perMonitorFiles[id];

                    // Shuffle a copy
                    foreach (var f in files.OrderBy(_ => _rng.Next()))
                    {
                        if (!chosen.Contains(f))
                        {
                            picks[id] = f;
                            chosen.Add(f);
                            break;
                        }
                    }

                    // If we couldn't avoid duplicates (not enough unique files), allow a duplicate
                    if (!picks.ContainsKey(id))
                    {
                        picks[id] = files[_rng.Next(files.Count)];
                    }
                }

                // Apply wallpapers
                foreach (var kv in picks)
                {
                    _dw.SetWallpaper(kv.Key, kv.Value);
                }
            }
            catch (Exception ex)
            {
                _timer.Stop();
                MessageBox.Show("Failed to apply wallpapers:\r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Text = "Per-Monitor Wallpaper Rotator (Stopped due to error)";
            }
        }

        private static bool HasImageExt(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                _timer.Stop();

                // Restore original wallpapers
                if (_dw != null && _originalWallpapers.Count > 0)
                {
                    foreach (var id in _monitorIds)
                    {
                        if (_originalWallpapers.TryGetValue(id, out var wp))
                        {
                            try { _dw.SetWallpaper(id, wp); } catch { /* ignore */ }
                        }
                    }
                }

                // Restore screensaver state
                SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 0, _origSaverActive, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                if (_origSaverTimeout > 0)
                {
                    SystemParametersInfo(SPI_SETSCREENSAVETIMEOUT, _origSaverTimeout, ref _origSaverTimeout, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                }
            }
            catch
            {
                // best effort; swallow
            }
            clsConfig.Store();
        }

       // private void InitializeComponent()
      //  {
        //    // Designer-less form init
       //     this.StartPosition = FormStartPosition.CenterScreen;
      //  }
    }
}
