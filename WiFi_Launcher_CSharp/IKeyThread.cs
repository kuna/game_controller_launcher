using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using System.Collections;

/** for win32 macros **/
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WiFi_Launcher_CSharp
{
    class IKeyThread
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            int dx;
            int dy;
            uint mouseData;
            uint dwFlags;
            uint time;
            IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            uint uMsg;
            ushort wParamL;
            ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)] //*
            public MOUSEINPUT mi;
            [FieldOffset(4)] //*
            public KEYBDINPUT ki;
            [FieldOffset(4)] //*
            public HARDWAREINPUT hi;
        }

        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_UNICODE = 0x0004;
        public const uint KEYEVENTF_SCANCODE = 0x0008;
        public const uint XBUTTON1 = 0x0001;
        public const uint XBUTTON2 = 0x0002;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const uint MOUSEEVENTF_XDOWN = 0x0080;
        public const uint MOUSEEVENTF_XUP = 0x0100;
        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte vk, byte scan, int flags, ref int extrainfo);
        [DllImport("User32.dll")]
        public static extern uint SendInput(uint numberOfInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] input, int structSize);
        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        private ArrayList prsKey = new ArrayList();

        private const int KEYDOWN = 0x0000;
        private const int KEYUP = 0x0002;

        public static Thread KeyThread;
        int Rate = 50;
        bool Running = false;

        int info = 0;

        public void SetKeyPressRate(int milisec)
        {
            Rate = milisec;
        }

        public void KeyDown_S(ushort key)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].ki.dwFlags = 0;
            inputs[0].ki.wScan = (ushort)(key & 0xff);

            uint intReturn = SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }

        public void KeyUp_S(ushort key)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].ki.dwFlags = KEYEVENTF_KEYUP;
            inputs[0].ki.wScan = (ushort)(key & 0xff);

            uint intReturn = SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }

        public void KeyDown(byte key)
        {
            keybd_event(key, (byte)(MapVirtualKey(key, 0) & 0xff), KEYDOWN, ref info);
        }

        public void KeyUp(byte key)
        {
            keybd_event(key, (byte)(MapVirtualKey(key, 0) & 0xff), KEYUP, ref info);
        }

        public void PressKey(byte key)
        {
            keybd_event(key, (byte)(MapVirtualKey(key, 0) & 0xff), KEYDOWN, ref info);
            keybd_event(key, (byte)(MapVirtualKey(key, 0) & 0xff), KEYUP, ref info);
        }

        public void AddKeyDown(byte key)
        {
            for (int i = 0; i < prsKey.Count; i++)
            {
                if (prsKey[i].Equals(key))
                {
                    return;
                }
            }

            prsKey.Add(key);
        }

        public void AddKeyUp(byte key)
        {
            for (int i = 0; i < prsKey.Count; i++)
            {
                if (prsKey[i].Equals(key))
                {
                    prsKey.RemoveAt(i);
                    KeyUp(key);
                    return;
                }
            }
        }

        public void releaseAllKey()
        {
            for (int i = 0; i < prsKey.Count; i++)
            {
                KeyUp((byte)prsKey[0]);
                prsKey.RemoveAt(0);
            }
        }

        private void KeyThreadFunc()
        {
            while (Running)
            {
                try
                {
                    for (int i = 0; i < prsKey.Count; i++)
                        KeyDown((byte)prsKey[i]);
                    Thread.Sleep(Rate);
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }

        public void StartThread()
        {
            if (Running) return;
            Running = true;
            KeyThread = new Thread(KeyThreadFunc);
            KeyThread.Start();
        }

        public void StopThread()
        {
            Running = false;
            KeyThread.Abort();
        }
    }
}
