using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFi_Launcher_CSharp
{
    class Util
    {
        public static bool isFormInitalized = false;
        public static Form1 f;
        public static void SendAlert(String msg)
        {
            if (isFormInitalized)
                f.Status(msg);
        }
        public static void ShowBalloonTip(String msg)
        {
            Program.ShowBalloonTip(msg);
        }
    }
}
