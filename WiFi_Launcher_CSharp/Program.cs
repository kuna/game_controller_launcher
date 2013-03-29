using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Resources;

namespace WiFi_Launcher_CSharp
{
    public class Program: Form
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/

            Application.Run(new Program());
        }

        private static NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private ISocket Socket;

        public Program()
        {
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);
            trayMenu.MenuItems.Add("Show Form", OnShowForm);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "PhoneController Launcher";

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            trayIcon.Icon = new Icon(((System.Drawing.Icon)(resources.GetObject("$this.Icon"))), 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            Socket = new ISocket();
            Socket.InitalizeServer();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private void OnShowForm(object sender, EventArgs e)
        {
            Form1 f=new Form1();
            Util.f = f;
            Util.isFormInitalized = true;
            f.ShowDialog();
            Util.isFormInitalized = false;
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public static void ShowBalloonTip(String msg)
        {
            trayIcon.ShowBalloonTip(2000, "PhoneController", msg, ToolTipIcon.Info);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
                Socket.close();
            }

            base.Dispose(isDisposing);
        }
    }
}