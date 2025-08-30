using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace ASW_Background_Screens
{
    public partial class frmMain : Form
    {
        const string DEFAULT_DURATION = "600";  //10 minutes   
        const string NOTE_PAD_NAME = "Notepad.exe";
        const string SAVE_PRINT_FILENAME = "print_later.csv";
        const string MY_CONFIG_FILE = "MyConfig.cfg";
        const string DEBUG_FILENAME = "DebugData.csv";

        //Resize information
        Size originalSize;
        Point originalLocation;
        Dictionary<string, List<string>> perMonitorFiles;

        //Debug
        bool bIsInDebugMode = false;

        private class MonitorItem
        {
            public string Id;
            public int DisplayNumber;        // 1..N (what you show to user)
            public int UserDisplayNumber;
            public Rectangle Bounds;
            public override string ToString()
                => $"Monitor {DisplayNumber} (My Monitor — {UserDisplayNumber}) - {Bounds.Width}x{Bounds.Height}";
        }

        private List<MonitorItem> _monitors = new List<MonitorItem>();

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
        private readonly System.Windows.Forms.Timer _timer = new();
        private readonly Random _rng = new();

        // Per-monitor folder selections
        private readonly Dictionary<string, string> _lastPickByMonitorId =
            new(StringComparer.OrdinalIgnoreCase);
        private int _durationSeconds = 120; // change duration here or expose via UI

        private int[] iMyScreenOrder;
        private bool bUpdatingScreenOrder = false;

        bool bIsF9ReadyToGo = false;

        private static readonly HashSet<string> _okExtensions = new(StringComparer.OrdinalIgnoreCase)
                { ".jpg", ".jpeg", ".png", ".bmp", ".heic" };

        private ContextMenuStrip _cms;
        private ToolStripMenuItem _miSave;

        private List<TextBox> _pathBoxes = new();
        private List<Label> _countLabels = new();
        private List<Button> _browseButtons = new();

        #region    Form Operations

        public frmMain()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.StartPosition = FormStartPosition.CenterScreen;

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            clsConfig.Initialize(MY_CONFIG_FILE);

            string tempFont = clsConfig.GetOptionOrDefault("FontName", "Segoe UI");
            string tempFontSize = clsConfig.GetOptionOrDefault("FontSize", "12");

            this.Font = new Font(tempFont, float.Parse(tempFontSize));

            chkIncludeSubfolders.Checked = string.Equals(clsConfig.GetOption("IncludeSubfolders")
                , "True", StringComparison.OrdinalIgnoreCase);

            //Resize information
            originalLocation = this.Location;
            originalSize = this.Size;
            clsConfig.GetFormSettings(this);

            txtDuration.Text = clsConfig.GetOptionOrDefault("Duration", DEFAULT_DURATION);

            FillFitCombo(cbFillStyle);

            txtDuration.Left = lblDuration.Left + lblDuration.Width + 10;
            txtMyScreenOrder.Left = lblMyScreenOrder.Left + lblMyScreenOrder.Width + 10;

            //Debug Mode
            bIsInDebugMode = clsConfig.GetOptionAsBool("Debug", false);

            //for keeping a list of certain images via right click
            AddContextMenu();  //call before initpersitence
            InitPersistence();
            this.KeyPreview = true;

            //do before ScreenInitialzation
            txtMyScreenOrder.Text = clsConfig.GetOption("MyScreenOrder");

            ScreenInitialization();
            BuildMonitorLookups();  //so we can also lookup by id and monitor # for real and user #

            BuildMonitorRows();

            _timer.Tick += (s, e) => ApplyRandomWallpapersOnce();

            this.PerformAutoScale();  // re-run scaling with current DPI
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
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

            clsConfig.SaveFormSettings(this);
            clsConfig.SetOption("Duration", txtDuration.Text);
            if (cbFillStyle.SelectedItem is DesktopWallpaperPosition selPos)
                clsConfig.SetOption("WallpaperStyle", selPos.ToString());
            clsConfig.SetOption("FontName", this.Font.Name);
            clsConfig.SetOption("FontSize", this.Font.Size.ToString());
            clsConfig.SetOption("IncludeSubfolders", chkIncludeSubfolders.Checked.ToString());

            clsConfig.Store();
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            return;
        }

        private void frmMain_DoubleClick(object sender, EventArgs e)
        {
            //Resizing
            if (MessageBox.Show("Reset Size?", "Confirm", MessageBoxButtons.OKCancel) == DialogResult.OK)
                resetFormSettingsToolStripMenuItem_Click(sender, e);
        }
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (!bIsF9ReadyToGo) return;  //has to be running to work.
            if (e.KeyCode == Keys.F9) SaveCurrentForPrint();
        }

        private void UpdateTitle()
        {
            Text = Application.ProductName + " (Every " + _durationSeconds + "s)";
        }

        #endregion

        #region     Duration
        private void txtDuration_Validating(object sender, CancelEventArgs e)
        {
            var ok = int.TryParse(txtDuration.Text.Trim(), out var seconds) && seconds >= 5;
            if (!ok) { e.Cancel = true; MessageBox.Show("Enter seconds ≥ 5."); return; }

            _durationSeconds = seconds;
            _timer.Interval = _durationSeconds * 1000;
            UpdateTitle();
        }

        #endregion

        #region  Button operations

        private void btnStart_Click(object sender, EventArgs e)
        {
            //See if we can actually run
            if (!ValidateChildren()) return; // fires txtDuration_Validating

            if (_pathBoxes.Any(tb => string.IsNullOrWhiteSpace(tb.Text)))
            {
                MessageBox.Show("All monitors don't have an image path assigned.");
                return;
            }

            if (!ScreenInitialization())
                return;
            if (!LoadUpDirectories())
                return;
            StartRotation();
            UpdateContextMenuEnabled(true);
            bIsF9ReadyToGo = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            bIsF9ReadyToGo = false;
            UpdateContextMenuEnabled(false);
        }

        #endregion

        #region Initializations
        private bool ScreenInitialization()
        {
            try
            {
                if (_dw == null)
                {
                    _dw = (IDesktopWallpaper)new DesktopWallpaperCom();

                    // Enumerate monitors
                    uint count = _dw.GetMonitorDevicePathCount();

                    ResetScreensOrder(txtMyScreenOrder);

                    // Ensure iMyScreenOrder is usable; else default to 1..count
                    if (iMyScreenOrder == null || iMyScreenOrder.Length != count)
                    {
                        iMyScreenOrder = Enumerable.Range(1, (int)count).ToArray();
                    }

                    _monitorIds.Clear();
                    _monitors.Clear();

                    for (uint i = 0; i < count; i++)
                    {
                        _dw.GetMonitorDevicePathAt(i, out string id);
                        _dw.GetMonitorRECT(id, out RECT r);
                        _monitorIds.Add(id);
                        _monitors.Add(new MonitorItem
                        {
                            Id = id,
                            //error with i + 1
                            DisplayNumber = (int)i + 1,
                            UserDisplayNumber = iMyScreenOrder[(int)i],
                            Bounds = new Rectangle(r.left, r.top, r.right - r.left, r.bottom - r.top)
                        });
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
                return true;
            }
            catch (Exception ex)
            {
                _timer.Stop();
                MessageBox.Show("Failed to apply wallpapers:\r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Text = Application.ProductName +
                    " (Stopped due to error)";
                return false;
            }
        }

        private bool LoadUpDirectories()
        {
            try
            {
                // Collect candidates for each monitor
                perMonitorFiles = new Dictionary<string, List<string>>();
                for (int i = 0; i < _monitorIds.Count; i++)
                {
                    var id = _monitorIds[i];
                    string newDir = _pathBoxes[i].Text;

                    var files = EnumerateImageFiles(newDir, chkIncludeSubfolders.Checked)
                        .ToList();

                    if (files.Count == 0)
                        throw new InvalidOperationException(
                          // $"No images found in folder (including subfolders =():\r\n{newDir}");
                          $"No images found in folder (including " +
                          $"subfolders={chkIncludeSubfolders.Checked}):\r\n{newDir}");

                    _countLabels[i].Text = files.Count.ToString();

                    perMonitorFiles[id] = files;
                }
                return true;
            }

            catch (Exception ex)
            {
                _timer.Stop();
                MessageBox.Show("Directory Setup(s) issue :\r\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Text = Application.ProductName + "(Stopped due to error)";
                return false;
            }
        }



        #endregion

        #region Operations
        private void StartRotation()
        {
            _timer.Stop();
            _timer.Interval = Math.Max(5, _durationSeconds) * 1000;


            ApplyWallpaperStyle(Color.Black);

            ApplyRandomWallpapersOnce();

            _timer.Start();
            UpdateTitle();
        }

        private static bool HasImageExt(string path) =>
                     _okExtensions.Contains(Path.GetExtension(path)?.Trim());

        private void ApplyRandomWallpapersOnce()
        {
            // Choose unique images across monitors for this cycle
            var chosen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var picks = new Dictionary<string, string>(); // monitorId -> file path

            //Debug mode
            string _saveCsvPathDebug;
            string dirDebug = Application.StartupPath;
            _saveCsvPathDebug = Path.Combine(dirDebug, DEBUG_FILENAME);
            if (!File.Exists(_saveCsvPathDebug))
                File.WriteAllText(_saveCsvPathDebug
                    , "Timestamp,Monitor,FilePath,Note\r\n");

            try
            {
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

                    // remember last file shown on that monitor
                    _lastPickByMonitorId[kv.Key] = kv.Value;

                    if (bIsInDebugMode)
                    {
                        //write out the files it's using
                        var m = _monitors.FirstOrDefault(x => x.Id == kv.Key);

                        int sysNum = 0;
                        int userNum = 0;
                        if (m != null)
                        {
                            sysNum = m.DisplayNumber;
                            userNum = m.UserDisplayNumber;
                        }
                        string path;
                        _lastPickByMonitorId.TryGetValue(kv.Key, out path);


                        string outData = DateTime.Now.ToString() + ","
                            + Path.GetFileName(path) + ","
                            + sysNum.ToString() + "," + userNum.ToString()
                            + "," + Path.GetDirectoryName(path) + "\r\n";
                        File.AppendAllText(_saveCsvPathDebug, outData);
                    }
                }
            }
            catch (Exception ex)
            {
                _timer.Stop();
                MessageBox.Show("Failed to apply wallpapers:\r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Text = "Per-Monitor Wallpaper Rotator (Stopped due to error)";
            }
        }

        private Dictionary<string, (int DisplayNumber, int UserDisplayNumber)> _idToNums;
        private void BuildMonitorLookups()
        {
            for (int i = 0; i < _monitors.Count; i++)
                _monitors[i].UserDisplayNumber = iMyScreenOrder[i];

            _idToNums = _monitors.ToDictionary(
                m => m.Id,
                m => (m.DisplayNumber, m.UserDisplayNumber),
                StringComparer.Ordinal);
        }
        private string strPathNew(int monitorNumber, Button button)
        {
            int idx = monitorNumber - 1;
            string tempStartPath = _pathBoxes[idx].Text;

            // Returns t, l, h, w
            string[] strLastSettings = clsConfig.GetNonFormSettings("FileDialog");
            //int lastY;
            int lastY = int.TryParse(strLastSettings[0], out int temp0) ? temp0 : 0;
            int lastX = int.TryParse(strLastSettings[1], out int temp1) ? temp1 : 150;
            int lastH = int.TryParse(strLastSettings[2], out int temp2) ? temp2 : 150;
            int lastW = int.TryParse(strLastSettings[3], out int temp3) ? temp3 : 75;

            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = tempStartPath;
                fbd.Description = $"Choose a folder for Monitor # {monitorNumber}";

                if (lastW > 0 && lastH > 0)
                    clsWin32PositionHelper.PositionNextFolderBrowserDialog(this.Handle, lastX, lastY, lastW, lastH);
                else
                    clsWin32PositionHelper.PositionNextFolderBrowserDialog(this.Handle, lastX, lastY); // no resize


                clsWin32PositionHelper.CaptureFolderDialogBounds(this.Handle, (x, y, w, h) =>
                 {
                     // Expects t, l, h, w
                     clsConfig.SaveNonFormSettings("FileDialog", y.ToString(), x.ToString(),
                         h.ToString(), w.ToString());
                 });

                if (fbd.ShowDialog(this) != DialogResult.OK)
                {
                    MessageBox.Show("Folder selection cancelled by user.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return "";
                }

                var files = EnumerateImageFiles(fbd.SelectedPath, chkIncludeSubfolders.Checked)
                    .ToList();

                if (files.Count == 0)
                {
                    MessageBox.Show("Zero appropriate files", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return "";
                }

                _pathBoxes[idx].Text = fbd.SelectedPath;
                _countLabels[idx].Text = files.Count.ToString();

                clsConfig.SetOption("Screen" + monitorNumber, fbd.SelectedPath);
                return fbd.SelectedPath;
            }
        }
        #endregion

        #region Menu Bar Items
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void openPrintListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(NOTE_PAD_NAME, Application.StartupPath + "\\" + SAVE_PRINT_FILENAME);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"{Application.ProductName} \r\n {Application.ProductVersion}" +
                $"\r\n {Application.CompanyName} \r\n Joe Poff, CPA - Retired \r\n");
        }
        #endregion

        #region Config File

        private void openConfigFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(NOTE_PAD_NAME, Application.StartupPath + "\\" + MY_CONFIG_FILE);
        }
        private void resetFormSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clsConfig.RemoveSettingsForForm(this);
            clsConfig.Store();
            this.Size = originalSize;
            this.Location = originalLocation;
        }
        #endregion

        #region For Saving image name to a list

        // Fields
        private readonly List<PrintCandidate> _saved = new List<PrintCandidate>();
        private string _saveCsvPath;
        private void InitPersistence()
        {
            string dir = Application.StartupPath;
            _saveCsvPath = Path.Combine(dir, SAVE_PRINT_FILENAME);
            if (!File.Exists(_saveCsvPath))
                File.WriteAllText(_saveCsvPath, "Timestamp,Monitor,FilePath,Note\r\n");
        }
        private void AppendCsv(PrintCandidate c)
        {
            string esc(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
            string line = $"{esc(c.Timestamp.ToString("s"))},{c.Monitor}," +
                $"{c.UserMonitor},{esc(c.FilePath)},{esc(c.Note)}\r\n";
            File.AppendAllText(_saveCsvPath, line);
        }

        private class PrintCandidate
        {
            public DateTime Timestamp;
            public int Monitor;         // 1-based monitor number
            public int UserMonitor;
            public string FilePath;
            public string Note;
        }
        private static string PromptNote(IWin32Window owner, string title, string message,
            Font yourFont, string seed = "")
        {
            using (var f = new Form())
            {
                f.Text = title;
                f.StartPosition = FormStartPosition.CenterParent;

                f.AutoSize = true;
                f.AutoScaleMode = AutoScaleMode.Dpi;
                f.AutoScaleDimensions = new SizeF(96F, 96F);
                f.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                f.Padding = new Padding(12);
                f.MinimizeBox = false;
                f.MaximizeBox = false;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.Font = yourFont;


                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 2,
                    RowCount = 2
                };

                layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // Label
                                                                                    //  layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // Textbox
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));   // Textbox
                                                                                    // layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 600f));   // Textbox
                                                                                    // layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // (spare)
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));              // row 1: ok
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));              // row 1: cancel button

                {
                    var lbl = new Label
                    {
                        AutoSize = true,
                        Text = message,
                        Anchor = AnchorStyles.Left,
                        TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                        Margin = new Padding(0, 3, 8, 3)
                    };

                    var txt = new TextBox
                    {
                        AutoSize = true,
                        Dock = DockStyle.Fill,              // allow full stretch in its cell
                        Anchor = AnchorStyles.Left,
                        Text = seed,
                        Margin = new Padding(0, 0, 0, 0)
                    };

                    // Compute a sensible minimum width for the textbox based on DPI and message length
                    int dpi = f.DeviceDpi;                           // 96 at 100%, 192 at 200%, etc.
                    float scale = dpi / 96f;
                    int baseMin = (int)(380 * scale);                // base minimum scaled by DPI
                    int msgWidth = TextRenderer.MeasureText(message, f.Font).Width;
                    txt.MinimumSize = new Size(Math.Max(baseMin, msgWidth), 0);



                    var ok = new Button
                    {
                        AutoSize = true,
                        Anchor = AnchorStyles.Left,
                        Text = "OK",
                        DialogResult = DialogResult.OK
                    };

                    var cancel = new Button
                    {
                        AutoSize = true,
                        Anchor = AnchorStyles.Left,
                        Text = "Cancel",
                        DialogResult = DialogResult.Cancel
                    };


                    //f.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
                    layout.Controls.Add(lbl, 0, 0);
                    layout.Controls.Add(txt, 1, 0);
                    layout.Controls.Add(ok, 0, 1); // put button on second row, 
                    layout.Controls.Add(cancel, 1, 1); // put button on second row, rightmost col

                    // Add a stretch panel in (1, 1) so the button hugs the right edge
                    //  var spacer = new Panel { AutoSize = true, Dock = DockStyle.Fill };
                    //   layout.Controls.Add(spacer, 1, 1);

                    f.Controls.Add(layout);

                    f.AcceptButton = ok;
                    f.CancelButton = cancel;
                    f.Shown += (s, e) =>
                    {
                        f.PerformLayout();
                        // f.PerformAutoScale();

                        var pref = layout.GetPreferredSize(Size.Empty);
                        f.ClientSize = new Size(Math.Max(f.ClientSize.Width, pref.Width + 8), pref.Height + 8);
                        txt.SelectAll();
                        txt.Focus();
                    };




                    return f.ShowDialog(owner) == DialogResult.OK ? txt.Text : null;
                }
            }
        }
        private void SaveCurrentForPrint()
        {
            // choose monitor: simple dialog with a ComboBox
            using (var dlg = new Form())
            {
                dlg.Text = "Save for Print";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.AutoSize = true;
                dlg.AutoScaleMode = AutoScaleMode.Dpi;
                dlg.AutoScaleDimensions = new SizeF(96F, 96F);
                dlg.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                dlg.Padding = new Padding(12);
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.Font = this.Font;

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    ColumnCount = 3,
                    RowCount = 2
                };

                layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // Label
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));   // Combo stretches
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));        // (spare)
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));              // row 0: label + combo
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));              // row 1: button




                var lbl = new Label
                {
                    Text = "Monitor:",
                    AutoSize = true,
                    Anchor = AnchorStyles.Left
                };
                var cbo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    IntegralHeight = false
                };

                cbo.Items.Clear();
                foreach (var m in _monitors)
                {
                    cbo.Items.Add(m);
                }
                cbo.SelectedIndex = 0;
                // Measure the longest item to set a sensible width(works across DPI &fonts)
                int textPad = 40; // room for padding + arrow
                int maxItemWidth = cbo.Items.Cast<object>()
                    .Select(item => TextRenderer.MeasureText(cbo.GetItemText(item), cbo.Font).Width)
                    .DefaultIfEmpty(240)
                    .Max();

                int screenCap = Screen.FromControl(this).WorkingArea.Width - 100;
                int desiredWidth = Math.Min(screenCap, Math.Max(300, maxItemWidth + textPad));

                cbo.MinimumSize = new Size(desiredWidth, 0);
                cbo.DropDownWidth = Math.Min(screenCap, maxItemWidth
                    + SystemInformation.VerticalScrollBarWidth + textPad);

                var btn = new Button
                {
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Text = "Next",
                    DialogResult = DialogResult.OK
                };


                layout.Controls.Add(lbl, 0, 0);
                layout.Controls.Add(cbo, 1, 0);
                layout.SetColumnSpan(cbo, 2);  // let combo span columns 1..2 (full width)
                layout.Controls.Add(btn, 2, 0); // put button on second row, rightmost col

                // Add a stretch panel in (1, 1) so the button hugs the right edge
                var spacer = new Panel { AutoSize = true, Dock = DockStyle.Fill };
                layout.Controls.Add(spacer, 1, 1);

                dlg.Controls.Add(layout);

                dlg.AcceptButton = btn;

                dlg.PerformLayout();
                dlg.PerformAutoScale();

                dlg.Shown += (s, e) => EnsureComboWidths(cbo, dlg);


                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var sel = (MonitorItem)cbo.SelectedItem;
                string path;
                if (!_lastPickByMonitorId.TryGetValue(sel.Id, out path)
                                    || !File.Exists(path))
                {
                    MessageBox.Show($"No current image recorded for {sel}.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var note = PromptNote(this, "Add Note", "Why print this one?", this.Font);
                if (note == null) return;

                var c = new PrintCandidate
                {
                    Timestamp = DateTime.Now,
                    Monitor = sel.DisplayNumber,
                    UserMonitor = sel.UserDisplayNumber,
                    FilePath = path,
                    Note = note
                };
                _saved.Add(c);
                AppendCsv(c);
                MessageBox.Show("Saved to print list.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void EnsureComboWidths(ComboBox cbo, Control ownerForScreen)
        {
            if (cbo.Items.Count == 0) return;

            // Measure the longest item in device pixels for the combo’s current font
            int longest = cbo.Items.Cast<object>()
                .Select(item => TextRenderer.MeasureText(cbo.GetItemText(item), cbo.Font).Width)
                .DefaultIfEmpty(200)
                .Max();

            // Room for the arrow button + borders + comfortable padding
            int arrow = SystemInformation.VerticalScrollBarWidth + 24;
            int pad = 24;

            int desiredEditWidth = longest + arrow + pad;   // edit/display width
            int desiredDropWidth = longest + arrow + pad;   // dropdown popup width

            // Cap to screen working area
            int screenCap = Screen.FromControl(ownerForScreen).WorkingArea.Width - 100;
            desiredEditWidth = Math.Min(screenCap, Math.Max(300, desiredEditWidth));
            desiredDropWidth = Math.Min(screenCap, Math.Max(300, desiredDropWidth));

            // Apply
            cbo.MinimumSize = new Size(desiredEditWidth, 0);
            cbo.Width = desiredEditWidth;          // initial; TableLayout can still grow it
            cbo.DropDownWidth = desiredDropWidth;
        }
        #endregion

        #region Context Menu

        private void UpdateContextMenuEnabled(bool enabled) => _miSave.Enabled = enabled;
        private void AddContextMenu()
        {
            _cms = new ContextMenuStrip();
            _miSave = new ToolStripMenuItem("Save current image for print (F9)", null, (s, e) => SaveCurrentForPrint());
            _cms.Items.Add(_miSave);
            this.ContextMenuStrip = _cms; // right-click anywhere on the form

            UpdateContextMenuEnabled(false);
        }

        #endregion

        #region    Monitor Numbering and identifying
        private void AddHeaderRow()
        {
            //autosize doesn't work for him
            tableMonitors.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            // Add labels as headers
            var headerFont = new Font("Segoe UI", 10, FontStyle.Bold);

            tableMonitors.Controls.Add(new Label
            {
                Text = "Monitor",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = headerFont
            }, 0, 0);

            //  return;
            tableMonitors.Controls.Add(new Label
            {
                Text = "Files",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = headerFont
            }, 1, 0);

            tableMonitors.Controls.Add(new Label
            {
                Text = "Path",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = headerFont
            }, 2, 0);
        }

        private void BuildMonitorRows()
        {
            // Clear (if rebuilding)
            tableMonitors.SuspendLayout();
            tableMonitors.Controls.Clear();
            _pathBoxes.Clear();
            _countLabels.Clear();
            _browseButtons.Clear();

            tableMonitors.RowCount = _monitors.Count + 1;  //for headers
            tableMonitors.RowStyles.Clear();
            tableMonitors.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetDouble;

            AddHeaderRow();

            for (int r = 0; r < _monitors.Count; r++)
                tableMonitors.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Column styles: button | label | textbox (stretch)
            tableMonitors.ColumnStyles.Clear();
            tableMonitors.ColumnCount = 3;
            tableMonitors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableMonitors.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableMonitors.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));


            for (int i = 0; i < _monitors.Count && i < 4; i++)
            {
                var mon = _monitors[i]; // has DisplayNumber
                int row = i + 1;   //because of header row

                var btn = new Button
                {
                    Text = $"Mon {i + 1} (My {iMyScreenOrder?[i] ?? (i + 1)})",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    Anchor = AnchorStyles.None
                };
                int btnIndex = i; // capture
                btn.Click += (s, e) =>
                {
                    string strTemp = strPathNew(btnIndex + 1, btn);

                    var button = (Button)s;
                    // find the row this button is in
                    int row = tableMonitors.GetRow(button);

                    // Example: get the control in column 2 (third cell) of the same row
                    var thirdCellControl = tableMonitors.GetControlFromPosition(2, row);

                    thirdCellControl.Text = strTemp;

                    if (strTemp.Length > 0)
                    {
                        var files = EnumerateImageFiles(strTemp, chkIncludeSubfolders.Checked)
                                              .ToList();

                        if (files.Count == 0)
                        {
                            MessageBox.Show("Zero appropriate files", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        var SecondCellControl = tableMonitors.GetControlFromPosition(1, row);
                        SecondCellControl.Text = files.Count.ToString();
                    }
                };

                var lbl = new Label
                {
                    AutoSize = true,
                    Text = "0",
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    Anchor = AnchorStyles.None
                };

                var txt = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
                string strTemp = clsConfig.GetOption("Screen" + (btnIndex + 1));

                if (Directory.Exists(strTemp))
                {
                    try
                    {
                        lbl.Text =
                            Directory.EnumerateFiles(strTemp)
                            .Where(HasImageExt).Count().ToString();
                    }
                    catch { lbl.Text = ""; }
                }
                else lbl.Text = "";

                txt.Text = strTemp;

                tableMonitors.Controls.Add(btn, 0, row);
                tableMonitors.Controls.Add(lbl, 1, row);
                tableMonitors.Controls.Add(txt, 2, row);

                _browseButtons.Add(btn);
                _countLabels.Add(lbl);
                _pathBoxes.Add(txt);
            }
            tableMonitors.ResumeLayout();
        }

        private void txtMyScreenOrder_Validated(object sender, EventArgs e)
        {
            if (bUpdatingScreenOrder)
                return;

            bool wasRunning = _timer.Enabled;
            _timer.Stop();
            TextBox txt = sender as TextBox;
            ResetScreensOrder(txt);
            if (wasRunning) _timer.Start();
        }

        private string GetDefaultScreenOrder()
        {
            string defaultAnswer = "";
            //1 based
            for (int i = 1; i < _dw.GetMonitorDevicePathCount() + 1; i++)
            {
                defaultAnswer += i.ToString() + ",";
            }
            //remove last ,
            return defaultAnswer.Substring(0, defaultAnswer.Length - 1);
        }

        private void ResetScreensOrder(TextBox yourTextBox)
        {
            if (
                (!yourTextBox.Text.Contains(",") && yourTextBox.Text.Length > 1)
                ||
                yourTextBox.Text.Length == 0)
            {
                MessageBox.Show("Format is Monitor comma as in 1,2,3 for monitors 1 2 and 3. Using defualts."
                    , "Format", MessageBoxButtons.OK);

                yourTextBox.Text = GetDefaultScreenOrder();

                return;
            }
            bUpdatingScreenOrder = true;
            txtMyScreenOrder.Text = yourTextBox.Text;
            clsConfig.SetOption("MyScreenOrder", txtMyScreenOrder.Text);

            Update_iMyScreenOrder(yourTextBox.Text);

            //  BuildMonitorLookups();   //reset these after txtmyscreeorder is set

            var expected = (int)_dw.GetMonitorDevicePathCount();

            if (iMyScreenOrder.Length != expected)
            {
                MessageBox.Show($"You have {expected} monitors; please enter"
                    + " exactly that many numbers in the My Screen Order textbox. Default values assigned.");
                yourTextBox.Text = GetDefaultScreenOrder();
                Update_iMyScreenOrder(yourTextBox.Text);
                bUpdatingScreenOrder = false;
                return;
            }

            for (int i = 0; i < _monitors.Count; i++)
                _monitors[i].UserDisplayNumber = iMyScreenOrder[i];

            bUpdatingScreenOrder = false;
        }

        private void Update_iMyScreenOrder(string yourStringOrder)
        {
            iMyScreenOrder = yourStringOrder
               .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(s => int.Parse(s.Trim()))
               .ToArray();

            for (int i = 0; i < _browseButtons.Count && i < iMyScreenOrder.Length; i++)
                _browseButtons[i].Text = $"Mon {i + 1} (My {iMyScreenOrder[i]})";
        }

        #endregion

        #region    For Wallpaper Sizing
        private enum DesktopWallpaperPosition
        {
            Center = 0,
            Tile = 1,
            Stretch = 2,
            Fit = 3,
            Fill = 4,
            Span = 5
        }
        private static uint ToColorRef(Color c) => (uint)((c.B << 16) | (c.G << 8) | c.R);

        private void ApplyWallpaperStyle(Color? background = null)
        {
            // Global for all monitors
            var selected = (DesktopWallpaperPosition)cbFillStyle.SelectedItem;
            _dw.SetPosition((int)selected);

            if (background.HasValue)
                _dw.SetBackgroundColor(ToColorRef(background.Value));
        }

        private void FillFitCombo(ComboBox cb)
        {
            cb.DataSource = Enum.GetValues(typeof(DesktopWallpaperPosition));

            var saved = clsConfig.GetOption("WallpaperStyle");
            if (Enum.TryParse(saved, out DesktopWallpaperPosition pos))
                cb.SelectedItem = pos;
            else
                cb.SelectedItem = DesktopWallpaperPosition.Fill; // just an initial default

            cb.SelectedIndexChanged += cbFillStyle_SelectedIndexChanged;
        }

        private void cbFillStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Only do the “refresh” if we’re actively rotating
            if (_dw == null || !_timer.Enabled) return;

            // Optionally pause briefly to avoid racing the Tick handler
            bool wasRunning = _timer.Enabled;
            _timer.Stop();
            try
            {
                // Apply the new style (your Option B uses the ComboBox value)
                ApplyWallpaperStyle(Color.Black);

                // Force a redraw using the *current* image for each monitor
                foreach (var id in _monitorIds)
                {
                    if (_lastPickByMonitorId.TryGetValue(id, out var path) && File.Exists(path))
                        _dw.SetWallpaper(id, path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply style:\r\n" + ex.Message, "Wallpaper", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (wasRunning) _timer.Start();
            }
        }

        #endregion

        #region    Helper for going through the directories
        private IEnumerable<string> EnumerateImageFiles(string root, bool includeSubfolders)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                yield break;

            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var dir = stack.Pop();

                // Enumerate files in this dir
                IEnumerable<string> files = Array.Empty<string>();
                try { files = Directory.EnumerateFiles(dir); }
                catch { /* skip this dir on error */ }

                foreach (var f in files)
                    if (HasImageExt(f))
                        yield return f;

                if (!includeSubfolders) continue;

                // Push subdirs
                IEnumerable<string> subs = Array.Empty<string>();
                try { subs = Directory.EnumerateDirectories(dir); }
                catch { /* skip subdirs on error */ }

                foreach (var sub in subs)
                    stack.Push(sub);
            }
        }
        #endregion
    }
}

