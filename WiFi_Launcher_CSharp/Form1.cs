using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;

/** for win32 macros **/
using Microsoft.Win32;
using System.Runtime.InteropServices;

/*
 * 구현되어야 할 이벤트
 * 
 * 1. KeyDown, KeyUp 메서드 지원
 * 2. Prs하고 Move시 각도, Size 값을 받을 것인가? (SpcCtrl)
 */

namespace WiFi_Launcher_CSharp
{
    public partial class Form1 : Form
    {
        // http://crynut84.tistory.com/34
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

        // 참조중인 플래시게임: http://q828.tistory.com/838
        // 레이싱: http://kizi.com/games/free-gear
        // FPS: http://kizi.com/games/coasterracer2

        public Form1()
        {
            InitializeComponent();
        }

        public void Status(String str)
        {
            lb.Invoke(new MethodInvoker(delegate() { lb.Items.Add(str); }));
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ISocket.mode = comboBox1.SelectedIndex;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = ISocket.mode;
        }
    }
}
