using KeyboardHookClass;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Toolkit.Uwp.Notifications;

namespace BSCScan_Helper
{
    static class Program
    {
        private static readonly NameValueCollection Config = ConfigurationManager.AppSettings;
        private static Regex Supported;
        private static string Server;
        private static bool InstantCopy;
        private static KeyboardHook hook;
        private static ClipboardWatcher ClipboardViewer;
        private static string clipboard_temp;


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
            Server = Config.Get("Server");

            InstantCopy = Convert.ToBoolean(Config.Get("UseInstantcopy"));

            string _Supported = "(?'Transaction'0x[a-zA-Z0-9]{64})|(?'Address'0x[a-zA-Z0-9]{40})";

            Supported = new Regex(_Supported, RegexOptions.ExplicitCapture | RegexOptions.Compiled);

            ClipboardViewer = new ClipboardWatcher();

            ToastNotificationManagerCompat.OnActivated += toastArgs => { Execute(); };
            ClipboardViewer.ClipboardChanged += new EventHandler<ClipboardChangedEventArgs>(ClipboardNotification);

            hook = new KeyboardHook();
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(Hotkey);
            hook.RegisterHotKey(ModifierKeys.Win | ModifierKeys.NoRepeat, Keys.Escape);

        }

        private static void ClipboardNotification(object sender, ClipboardChangedEventArgs e)
        {
            if (Check_Clipboard())
            {
                Notify();
            }
        }

        private static void Hotkey(object sender, EventArgs e)
        {
            TriggerClipboardCopy();

            if (Check_Clipboard())
            {
                Execute();
            }
        }

        private static void TriggerClipboardCopy()
        {
            if (InstantCopy)
            {
                Thread.Sleep(200);
                SendKeys.Send("^c");
                Thread.Sleep(200);
            }
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
                .AddArgument("TitleText", "BSCScan Helper")
                .AddText("blockchain address(es) found in clipboard.")
                .AddText("tap to open")
                .AddToastActivationInfo("", ToastActivationType.Background)
                .SetToastDuration(ToastDuration.Short)
                .SetBackgroundActivation()
                .Show();
        }

        private static void Execute()
        {
            List<string> ProcessedItems = GenerateLinks(clipboard_temp);
            LaunchLinks(ProcessedItems);
        }

        private static List<string> GenerateLinks(in string Clipboard_Content)
        {
            List<string> ProcessedItems = new List<string>();

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

                if (!ProcessedItems.Contains(Link))
                {
                    ProcessedItems.Add(Link);
                }
            }

            return ProcessedItems;
        }

        private static void LaunchLinks(List<string> ProcessedItems)
        {
            foreach (string Address in ProcessedItems)
            {
                Process.Start(Address);
            }
        }
    }
}
