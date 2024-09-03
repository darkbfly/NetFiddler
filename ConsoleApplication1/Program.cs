

using System;
using Newtonsoft.Json.Linq;
using log4net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ConsoleApplication1;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Web.WebSockets;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace ConsoleApp1
{
    public class Program
    {
        static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;
        static private NotifyIcon notifyIcon;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                default:
                    shutdown();
                    return false;
            }
        }
        public class ConsoleExitEventArgs : EventArgs
        {
            public long countip { get; set; }
            //public testamp_ampip db { get; set; }
        }
        static void onProcessExit(object sender, EventArgs e)
        {
            if (e != null)
            {
                Console.WriteLine("ProcessExit");
                Console.WriteLine(((ConsoleExitEventArgs)e).countip);
            }
            else
                Console.WriteLine("Exit");
            shutdown();
        }

        static void shutdown()
        {
            log.Info("关闭程序中");
            Fiddler.FiddlerApplication.Shutdown();
            log.Info("关闭程序完成");
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new System.EventHandler(onProcessExit);
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // Debug();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 初始化NotifyIcon控件
            notifyIcon = new NotifyIcon
            {
                Icon = LoadIconFromResource("favicon.ico"),
                Text = "MyFiddler",
                Visible = true,
            };
            Application.Run(new NetFiddler()); // 这里的 MainForm 是你的 WinForms 应用程序的主窗体类名
        }

        static private Icon LoadIconFromResource(string resourceName)
        {
            using (var stream = File.OpenRead(resourceName))
            {
                return new Icon(stream);
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogException(e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject as Exception);
        }

        static void LogException(Exception ex)
        {
            log.Error(ex);
            shutdown();
        }

        static public void ShowNotification(string text, string title)
        {
            // 显示带有标题和文本的通知
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = text;
            notifyIcon.ShowBalloonTip(5000); // 显示5秒
        }

        static public void Debug()
        {
            Dictionary<string, string> headerMap = new Dictionary<string, string>();

            headerMap.Add("1111", "111111");
            headerMap.Add("2222", "321321");
        }
    }
}