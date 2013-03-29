using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;

/** for win32 macros **/
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WiFi_Launcher_CSharp
{
    class ISocket
    {        // http://crynut84.tistory.com/34
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte vk, byte scan, int flags, ref int extrainfo);
        private const byte AltKey = 18;
        private const int KEYDOWN = 0x0000;
        private const int KEYUP = 0x0002;

        private const byte VK_LEFT = 0x0025;
        private const byte VK_UP = 0x0026;
        private const byte VK_RIGHT = 0x0027;
        private const byte VK_DOWN = 0x0028;


        // http://www.hachangho.com/homev30/bbs/zboard.php?id=tech&page=1&sn1=&divpage=1&category=31&sn=off&ss=on&sc=on&select_arrange=headnum&desc=asc&no=434&PHPSESSID=f123e21ee32e4c45237f98f305826c60
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        private const uint MOUSEMOVE = 0x0001;      // 마우스 이동
        private const uint LBUTTONDOWN = 0x0002;    // 왼쪽 마우스 버튼 눌림
        private const uint LBUTTONUP = 0x0004;      // 왼쪽 마우스 버튼 떼어짐
        private const uint ABSOLUTEMOVE = 0x8000;      // 전역 위치

        // http://breadbaker.tistory.com/126
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;  // 마우스 휠 돌리기. 돌리는 양은 dwData에 적어주세요


        private const string KEY_LEFT = "Left";
        private const string KEY_UP = "Up";
        private const string KEY_RIGHT = "Right";
        private const string KEY_DOWN = "Down";
        private const string MOUSE_LBUTTON = "LB";       // 마우스 왼쪽
        private const string MOUSE_RBUTTON = "RB";       // 마우스 오른쪽
        private const string MOUSE_MBUTTON = "MB";       // 마우스
        private const string MOUSE_SCRUP = "SUp";      // 스크롤 업
        private const string MOUSE_SCRDOWN = "SDown";    // 스크롤다운

        /***** 필수 부분 시작 *****/
        public static Socket Server;
        EndPoint localEP, remoteEP;
        public bool InitalizeServer()
        {
            // set addr obj
            localEP = new IPEndPoint(IPAddress.Any, 1234);
            remoteEP = new IPEndPoint(IPAddress.None, 1234);

            try
            {
                // set server obj
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Server.Bind(localEP);

                Util.SendAlert("-------------------------------");
                Util.SendAlert("Waiting for Connection ...");
                Util.SendAlert("-------------------------------");
                Util.ShowBalloonTip("프로그램이 실행되었습니다, 기기와의 연결 대기중...");
                RunClientThread();

            }
            catch (Exception e)
            {
                Util.SendAlert(e.Message.ToString());
                Util.SendAlert("Error Initalizing Server.");
            }

            return true;
        }

        public static Thread SThread;
        IKeyThread KeyThread;
        public void RunClientThread()
        {
            bAlive = true;
            SThread = new Thread(SocketThreadFunc);
            SThread.Start();
            KeyThread = new IKeyThread();
            KeyThread.StartThread();
        }

        Boolean bAlive;
        string stringbyte;
        byte[] getByte = new byte[1024];
        public void SocketThreadFunc()
        {
            try
            {
                while (bAlive)
                {
                    getByte = new byte[1024];
                    int l = Server.ReceiveFrom(getByte, ref remoteEP);
                    stringbyte = Encoding.ASCII.GetString(getByte);
                    String[] str_datas = stringbyte.Split('\n');
                    for (int i = 0; i < str_datas.Length; i++)
                        ProcessRecvData(str_datas[i]);
                }
            }
            catch (ThreadAbortException e)
            {
                // ignore
            }
            catch (Exception e)
            {
                Util.SendAlert(e.Message.ToString());
                Util.SendAlert("Error Socket Data Transmission.");
            }
            Util.SendAlert("연결 종료");
            Util.ShowBalloonTip("기기와의 연결이 종료되었습니다.");
        }

        /***** 필수 부분 끝 *****/

        public static int mode = 0;
        int keyMode = 0;
        private void ProcessRecvData(String str)
        {
            try
            {
                str = str.Replace("\0", string.Empty);
                if (str.Length == 0) return;
                if (str == "PING")
                {
                    sendData("PONG");
                    Util.SendAlert("Device Detected");
                }

                String[] str_param = str.Split('#');
                if (str_param.Length <= 1)
                {
                    // other message
                    Util.SendAlert(str);
                }
                else if (str_param.Length == 3)
                {
                    if (Int32.Parse(str_param[0]) == 200)
                    {
                        String[] str_args = str_param[2].Split(',');
                        float acclX = Single.Parse(str_args[0]);
                        float acclY = Single.Parse(str_args[1]);
                        float acclZ = Single.Parse(str_args[2]);
                        if (mode == 1)
                        {
                            if (acclY < -3)
                            {
                                Util.SendAlert("[EVENT] 레이싱모드 - 왼쪽이동");
                                KeyThread.AddKeyDown(VK_LEFT);
                            }
                            else if (acclY > 3)
                            {
                                Util.SendAlert("[EVENT] 레이싱모드 - 오른쪽이동");
                                KeyThread.AddKeyDown(VK_RIGHT);
                            }
                            else
                            {
                                KeyThread.AddKeyUp(VK_LEFT);
                                KeyThread.AddKeyUp(VK_RIGHT);
                            }
                        }

                        if (acclZ > 15 && mode == 3)
                        {
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        }

                    }
                    else if (Int32.Parse(str_param[0]) == 201 && mode == 2)
                    {
                        String[] str_args = str_param[2].Split(',');
                        float oriX = Single.Parse(str_args[0]);
                        float oriY = Single.Parse(str_args[1]);
                        float oriZ = Single.Parse(str_args[2]);

                        // move cursor
                        Rectangle rtScreen = Screen.PrimaryScreen.Bounds;

                        uint nX = (uint)(65535 * (1 + oriX / 40) / 2);
                        uint nY = (uint)(65535 * (1 + oriY / 30) / 2);

                        mouse_event(ABSOLUTEMOVE | MOUSEMOVE, nX, nY, 0, 0);
                    }
                    else if (Int32.Parse(str_param[0]) == 300)
                    {
                        if (str_param[2] == "null") return;
                        if (str_param[2] == KEY_LEFT)
                            KeyThread.AddKeyDown(VK_LEFT);
                        else if (str_param[2] == KEY_UP)
                            KeyThread.AddKeyDown(VK_UP);
                        else if (str_param[2] == KEY_RIGHT)
                            KeyThread.AddKeyDown(VK_RIGHT);
                        else if (str_param[2] == KEY_DOWN)
                            KeyThread.AddKeyDown(VK_DOWN);
                        else if (str_param[2] == MOUSE_LBUTTON)
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        else if (str_param[2] == MOUSE_RBUTTON)
                            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        else if (str_param[2] == MOUSE_MBUTTON)
                            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                        else if (str_param[2] == MOUSE_SCRDOWN)
                            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 3, 0);
                        else if (str_param[2] == MOUSE_SCRUP)
                            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, Convert.ToUInt32(-3), 0);
                        else
                            KeyThread.AddKeyDown((byte)str_param[2][0]);
                    }
                    else if (Int32.Parse(str_param[0]) == 301)
                    {
                        if (str_param[2] == "null") return;
                        if (str_param[2] == KEY_LEFT)
                            KeyThread.AddKeyUp(VK_LEFT);
                        else if (str_param[2] == KEY_UP)
                            KeyThread.AddKeyUp(VK_UP);
                        else if (str_param[2] == KEY_RIGHT)
                            KeyThread.AddKeyUp(VK_RIGHT);
                        else if (str_param[2] == KEY_DOWN)
                            KeyThread.AddKeyUp(VK_DOWN);
                        else if (str_param[2] == MOUSE_LBUTTON)
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        else if (str_param[2] == MOUSE_RBUTTON)
                            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        else if (str_param[2] == MOUSE_MBUTTON)
                            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                        else
                            KeyThread.AddKeyUp((byte)str_param[2][0]);
                    }
                }
            }
            catch (Exception e)
            {
                Util.SendAlert("[ERROR] 통신 상태가 좋지 않습니다");
            }
        }

        public void close()
        {
            if (bAlive)
            {
                bAlive = false;
                KeyThread.releaseAllKey();
                KeyThread.StopThread();
                Server.Shutdown(SocketShutdown.Both);
                SThread.Abort();
                Server.Close();
            }
        }

        public int sendData(String str)
        {
            ASCIIEncoding ae = new ASCIIEncoding();
            return Server.SendTo(ae.GetBytes(str), ae.GetBytes(str).Length, SocketFlags.None, remoteEP);
        }
    }
}
