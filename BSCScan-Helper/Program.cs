using ClipboardWatcherClass;
using KeyboardHookClass;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace BSCScan_Helper
{
    static class Program
    {
        #region static variables
        private static Regex Supported;
        private static string Server;
        private static bool InstantCopy, NotificationBlock, Notifications;
        private static KeyboardHook hook;
        private static ClipboardWatcher ClipboardViewer;
        private static string clipboard_temp;
        private static System.Windows.Forms.Timer timerNotificationBlock;
        #endregion

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Setup();

            Application.Run(new CustomApplicationContext());
        }

        private static void Setup()
        {
            ReadFromConfig();

            MakeRegex();

            SetupClipboard();

            SetupHotkey();

            SetupTimer();

            #region local functions
            void MakeRegex()
            {
                string _Supported = "(?'Transaction'0x[a-zA-Z0-9]{64})|(?'Address'0x[a-zA-Z0-9]{40})";

                Supported = new Regex(_Supported, RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            }

            void SetupClipboard()
            {
                ClipboardViewer = new ClipboardWatcher();

                ToastNotificationManagerCompat.OnActivated += toastArgs => { Execute(); };
                ClipboardViewer.ClipboardChanged += new EventHandler<ClipboardChangedEventArgs>(ClipboardNotification);
            }

            void SetupHotkey()
            {
                hook = new KeyboardHook();
                hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(Hotkey);
                hook.RegisterHotKey(ModifierKeys.Win | ModifierKeys.NoRepeat, Keys.Escape);
            }

            void SetupTimer()
            {
                timerNotificationBlock = new System.Windows.Forms.Timer();
                timerNotificationBlock.Tick += new EventHandler(StopNotifificationBlockTimer);
                timerNotificationBlock.Interval = 1000;
            }
            #endregion
        }

        public static void ReadFromConfigProxy(Object sender, EventArgs e)
        {
            ReadFromConfig();
        }

        private static void ReadFromConfig()
        {
            ConfigurationManager.RefreshSection("appSettings");

            Server = ConfigurationManager.AppSettings.Get("Server");
            InstantCopy = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("UseInstantcopy"));
            Notifications = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("AllowNotifications"));
        }

        private static void ClipboardNotification(object sender, ClipboardChangedEventArgs e)
        {
            if (Check_Clipboard() && !NotificationBlock && Notifications)
            {
                Notify();
            }
        }

        private static void Hotkey(Object sender, EventArgs e)
        {
            TriggerClipboardCopy();

            if (Check_Clipboard())
            {
                Execute();
            }

            #region local functions
            void TriggerClipboardCopy()
            {
                if (InstantCopy)
                {
                    NotificationBlock = true;

                    Thread.Sleep(200);
                    SendKeys.Send("^c");
                    Thread.Sleep(200);
                }
            }
            #endregion
        }

        private static void StopNotifificationBlockTimer(Object sender, EventArgs e)
        {
            timerNotificationBlock.Stop();
            NotificationBlock = false;
        }

        private static bool Check_Clipboard()
        {
            if (Clipboard.ContainsText())
            {
                clipboard_temp = Clipboard.GetText(TextDataFormat.Text).Replace("\r\n", "");
                if (Supported.IsMatch(clipboard_temp))
                {
                    return true;
                }
                else
                {
                    clipboard_temp = String.Empty;
                    return false;
                }
            }
            else
            {
                clipboard_temp = String.Empty;
                return false;
            }
        }

        private static void Notify()
        {
            new ToastContentBuilder()
                .AddText("blockchain address(es) found in clipboard.")
                .AddText("tap or use hotkey (win + escape) to open")
                .AddToastActivationInfo("", ToastActivationType.Background)
                .SetToastDuration(ToastDuration.Short)
                .SetBackgroundActivation()
                .Show(toast =>
                 {
                     toast.ExpirationTime = DateTime.Now.AddSeconds(10);
                 });
        }

        private static void Execute()
        {
            List<string> ProcessedItems = GenerateLinks(clipboard_temp);

            foreach (string Address in ProcessedItems)
            {
                Process.Start(Address);
            }

            #region local functions
            List<string> GenerateLinks(in string Clipboard_Content)
            {
                List<string> Items = new List<string>();

                foreach (Match Item in Supported.Matches(Clipboard_Content))
                {
                    string Link;
                    string Address = Item.Value;

                    if (Item.Groups["Address"].Success)
                    {
                        Link = $"https://{Server}/address/{Address}";
                    }
                    else if (Item.Groups["Transaction"].Success)
                    {
                        Link = $"https://{Server}/tx/{Address}";
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    if (!Items.Contains(Link))
                    {
                        Items.Add(Link);
                    }
                }

                return Items;
            }
            #endregion
        }
    }
}
