using KeyboardHookClass;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;


namespace BSCScan_Helper
{
    static class Program
    {
        private static readonly NameValueCollection Config = ConfigurationManager.AppSettings;
        private static Regex Supported;
        private static string Server;
        private static bool InstantCopy;
        private static KeyboardHook hook;


        [STAThread]
        static void Main()
        {
            ReadConfig();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RegisterHotkey();

            Application.Run(new CustomApplicationContext());
        }

        private static void ReadConfig()
        {
            Server = Config.Get("Server");

            InstantCopy = Convert.ToBoolean(Config.Get("UseInstantcopy"));

            string _Supported = "(?'Transaction'0x[a-zA-Z0-9]{64})|(?'Address'0x[a-zA-Z0-9]{40})";

            Supported = new Regex(_Supported, RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        }

        private static void RegisterHotkey()
        {
            hook = new KeyboardHook();
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(Execute);
            hook.RegisterHotKey(ModifierKeys.Win | ModifierKeys.NoRepeat, Keys.Escape);
        }

        private static void Execute(object sender, KeyPressedEventArgs e)
        {
            TriggerClipboardCopy();

            if (Clipboard.ContainsText())
            {
                string Clipboard_Content = Clipboard.GetText(TextDataFormat.Text).Replace("\r\n", " ");

                List<string> ProcessedItems = GenerateLinks(Clipboard_Content);

                LaunchLinks(ProcessedItems);
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

        private static void LaunchLinks(List<string> ProcessedItems)
        {
            foreach (string Address in ProcessedItems)
            {
                Process.Start(Address);
            }
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

    }
}
