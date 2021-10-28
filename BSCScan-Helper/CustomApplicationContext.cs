using BSCScan_Helper.Properties;
using System;
using System.Windows.Forms;

namespace BSCScan_Helper
{
    public class CustomApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        public CustomApplicationContext()
        {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Text = "BSCScan helper",
                Icon = (System.Drawing.Icon)Resources.ResourceManager.GetObject("icon"),
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Exit", Exit)
            }),
                Visible = true
            };
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            //Program.TokenSource.Cancel();
            Application.Exit();
        }
    }
}
