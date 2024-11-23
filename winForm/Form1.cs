using ScreenShot;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static winForm.Form1;

namespace winForm
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();

         }

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        IntPtr TaskHwnd = IntPtr.Zero;
        IntPtr DeskHwnd = IntPtr.Zero;

        public static IntPtr WallDesk = IntPtr.Zero;


        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChild, string lpclassName, string lpWindowName);

        //定义常量
        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;

        private bool HotKeyStus;




        private bool TaskShow = true;
        private bool DeskShow = true;

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowText(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        //键盘钩子
        public const int WM_LBUTTONDOWN = 0x201;




        //安装钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetWindowsHookEx(int idHook, KeyboardHook lpfn, IntPtr hMod, int dwThreadId);


        //卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(IntPtr idHook);

        //传递钩子
        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string name);



        //委托
        private delegate int KeyboardHook(int nCode, IntPtr wParam, IntPtr lParam);



        private void UpdateButton()
        {
            //bool isTaskVisible = IsWindowVisible(TaskHwnd);
            //bool isDeskVisible = IsWindowVisible(DeskHwnd);
           
            this.TaskBtn.Text = this.TaskShow ? "隐藏任务栏" : "显示任务栏";
            this.DeskBtn.Text = this.DeskShow ? "隐藏桌面图标" : "显示桌面图标";

        }

        private void GetTaskHwnd()
        {
            TaskHwnd = FindWindow("Shell_TrayWnd", null);
            if(TaskHwnd == IntPtr.Zero)
            {
                MessageBox.Show("未找到该窗口");
            }
        }
        private void GetDeskHwnd()
        {

            IntPtr temp1 = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);
            IntPtr temp2 = FindWindowEx(temp1, IntPtr.Zero, null, null);
            //该处判断是否开启壁纸软件
            if(temp2 == IntPtr.Zero)
            {
                //MessageBox.Show("壁纸软件运行中");
                EnumWindows(ForEachWindow,IntPtr.Zero);
                DeskHwnd = FindWindowEx(Form1.WallDesk, IntPtr.Zero, "SysListView32", null);
                //MessageBox.Show($"dsadasd{DeskHwnd}");
            }
            else
            {
                DeskHwnd = FindWindowEx(temp2, IntPtr.Zero, null, null);
            }
        }
        //遍历所有窗口
        private static bool ForEachWindow(IntPtr hWnd, IntPtr lParam)
        {
            StringBuilder CLASS_NAME = new StringBuilder(256);
            int net = GetClassName(hWnd, CLASS_NAME, CLASS_NAME.Capacity);
            if(net > 0)
            {
                if(CLASS_NAME.ToString() == "WorkerW")
                {
                    IntPtr temp1 = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);

                    if (temp1 != IntPtr.Zero)
                    {
                        Form1.WallDesk = temp1;
                    }

                }
            }
            return true;
        }
        private void TaskBtn_Click(object sender, EventArgs e)
        {
            GetTaskHwnd();
            this.TaskShow = !this.TaskShow;
            if (this.TaskShow)
            {
                ShowWindow(TaskHwnd, SW_SHOW);
            }
            else
            {
                ShowWindow(TaskHwnd, SW_HIDE);
            }
            UpdateButton();
        }
        private void DeskBtn_Click(object sender, EventArgs e)
        {
            GetDeskHwnd();
            this.DeskShow = !this.DeskShow;
            if (this.DeskShow)
            {
                ShowWindow(DeskHwnd, SW_SHOW);
            }
            else
            {
                ShowWindow(DeskHwnd, SW_HIDE);
            }
            UpdateButton();

    
            
        }


        public bool IsVisible;





        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            if (m.Msg == WM_HOTKEY && !HotKeyStus)
            {
                switch (m.WParam.ToInt32())
                {
                    case 100:
                        //HotKey.UnregisterHotKey(Handle, 100);
                        this.Visible = true;
                        break;
                    
                }
            }
            base.WndProc(ref m);
        }





        private void Form1_Load(object sender, EventArgs e)
        {
            HotKey.RegisterHotKey(this.Handle,100,2, 0x51);

            HotKeyStus = HotKeyToolStripMenuItem.Checked;

            Console.WriteLine(HotKeyStus);
        }



        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ShowWindow(TaskHwnd, SW_SHOW);
            ShowWindow(DeskHwnd, SW_SHOW);

            Application.Exit();
        }


        //阻止默认关闭事件
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing)
            {
                    this.Hide();
                    this.notifyIcon1.Visible = true;
                    e.Cancel = true;
            }
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void HotKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HotKeyStus = !HotKeyStus;
        }
    }
}





