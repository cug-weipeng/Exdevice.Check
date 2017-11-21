using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Skins.Info;
using DevExpress.UserSkins;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;

namespace Exdevice.Check.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool exit;
            using (IDisposable singleInstanceApplicationGuard =SingleInstanceApplicationGuard("DevExpressWinHybridApp", out exit))
            {
                if (exit && IsTablet)
                    return;
                BonusSkins.Register();
                SkinManager.EnableFormSkins();
                UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");
                ((DevExpress.LookAndFeel.Design.UserLookAndFeelDefault)DevExpress.LookAndFeel.Design.UserLookAndFeelDefault.Default).LoadSettings(() =>
                {
                    var skinCreator = new SkinBlobXmlCreator("HybridApp", "Exdevice.Check.UI.SkinData.", typeof(Program).Assembly, null);
                    SkinManager.Default.RegisterSkin(skinCreator);
                });

                UserLookAndFeel.Default.SetSkinStyle("HybridApp");
                float fontSize = 11f;
                DevExpress.XtraEditors.WindowsFormsSettings.DefaultFont = new Font("Segoe UI", fontSize);
                DevExpress.XtraEditors.WindowsFormsSettings.DefaultMenuFont = new Font("Segoe UI", fontSize);
                Application.CurrentCulture = CultureInfo.GetCultureInfo("en-us");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                MainForm = new JobForm() { Icon = AppIcon };
                Application.Run(MainForm);
            }
        }
        static bool? isTablet = null;
        public static bool IsTablet
        {
            get
            {
                if (isTablet == null)
                {
                    isTablet = false;
                }
                return isTablet.Value;
            }
        }
        public static Icon AppIcon
        {
            get { return DevExpress.Utils.ResourceImageHelper.CreateIconFromResourcesEx("Exdevice.Check.UI.Resources.AppIcon.ico", typeof(JobForm).Assembly); }
        }
        public static JobForm MainForm
        {
            get;
            private set;
        }
        /// <summary>
        /// 单例模式
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="exit"></param>
        /// <returns></returns>
        public static IDisposable SingleInstanceApplicationGuard(string applicationName, out bool exit)
        {
            Mutex mutex = new Mutex(true, applicationName + AssemblyInfo.VersionShort);
            if (mutex.WaitOne(0, false))
            {
                exit = false;
            }
            else
            {
                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id && process.MainWindowHandle != IntPtr.Zero)
                    {
                        WinApiHelper.SetForegroundWindow(process.MainWindowHandle);
                        WinApiHelper.RestoreWindowAsync(process.MainWindowHandle);
                        break;
                    }
                }
                exit = true;
            }
            return mutex;
        }
    }
    static class WinApiHelper
    {
        [SecuritySafeCritical]
        public static bool SetForegroundWindow(IntPtr hwnd)
        {
            return Import.SetForegroundWindow(hwnd);
        }
        [SecuritySafeCritical]
        public static bool RestoreWindowAsync(IntPtr hwnd)
        {
            return Import.ShowWindowAsync(hwnd, IsMaxmimized(hwnd) ? (int)Import.ShowWindowCommands.ShowMaximized : (int)Import.ShowWindowCommands.Restore);
        }
        [SecuritySafeCritical]
        public static bool IsMaxmimized(IntPtr hwnd)
        {
            Import.WINDOWPLACEMENT placement = new Import.WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            if (!Import.GetWindowPlacement(hwnd, ref placement)) return false;
            return placement.showCmd == Import.ShowWindowCommands.ShowMaximized;
        }
        static class Import
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetForegroundWindow(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
            [StructLayout(LayoutKind.Sequential)]
            public struct WINDOWPLACEMENT
            {
                public int length;
                public int flags;
                public ShowWindowCommands showCmd;
                public System.Drawing.Point ptMinPosition;
                public System.Drawing.Point ptMaxPosition;
                public System.Drawing.Rectangle rcNormalPosition;
            }
            public enum ShowWindowCommands : int
            {
                Hide = 0,
                Normal = 1,
                ShowMinimized = 2,
                ShowMaximized = 3,
                ShowNoActivate = 4,
                Show = 5,
                Minimize = 6,
                ShowMinNoActive = 7,
                ShowNA = 8,
                Restore = 9,
                ShowDefault = 10,
                ForceMinimize = 11
            }
        }
    }
}
