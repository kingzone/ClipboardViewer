using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ClipboardViewer
{
    public partial class FormMain : Form
    {
        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        IntPtr nextClipboardViewer;

        public FormMain()
        {
            InitializeComponent();

            // 若函数调用成功，返回剪贴板监听器链中下一个窗口的句柄
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // 参见winuser.h中的定义
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:

                    //Console.WriteLine(Process.GetCurrentProcess().ProcessName + "+++++++" + GetForegroundWindow().ToString());
                    //IntPtr p_Handle = FindWindow("", "Windows任务管理器");
                    //StringBuilder _TitleString = new StringBuilder(256);
                    //GetWindowText(p_Handle, _TitleString, 256);
                    //Console.WriteLine(p_Handle + "" + _TitleString);

                    foreach (Process thisproc in Process.GetProcessesByName("ClipboardViewer.vshost"))   //进程名字
                    {
                        Console.WriteLine(thisproc.MainWindowHandle.ToInt32());
                        Console.WriteLine(GetForegroundWindow().ToInt32());
                        if( thisproc.MainWindowHandle.ToInt32() == GetForegroundWindow().ToInt32() )	
                        {
                            //你的代码
                            MessageBox.Show("ClipboardViewer.vshost");
                        }
                    }

                    DisplayClipboardData();
                    
                    if (nextClipboardViewer != null)
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else if (nextClipboardViewer != null)
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void DisplayClipboardData()
        {
            IDataObject iData = new DataObject();
            iData = Clipboard.GetDataObject();

            if (iData.GetDataPresent(DataFormats.Rtf))
                this.richTextBox1.Rtf = (string)iData.GetData(DataFormats.Rtf);
            else if (iData.GetDataPresent(DataFormats.Text))
                this.richTextBox1.Text = (string)iData.GetData(DataFormats.Text);
            else
                this.richTextBox1.Text = "[Clipboard data is not RTF or ASCII Text]";
        }
    }
}