using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ConsoleApplication1
{
    public partial class NetFiddler : Form
    {
        MyFiddler fiddler;
        bool bStart = false;
        public NetFiddler()
        {
            InitializeComponent();
            button1.Enabled = bStart;
            if (!qinglong.GetInstance().Init(this))
            {
                addLog("初始化青龙环境失败, 请检查配置文件");
                启动.Enabled = false;
                comboBox1.Enabled = false;
                return;
            }
            comboBox1.Items.AddRange(qinglong.GetInstance().remarkList.ToArray());
            comboBox1.SelectedIndex = 0;
            启动_Click(null, null);
        }

        private void 启动_Click(object sender, EventArgs e)
        {
            if (!bStart)
            {
                addLog("开始启动");
                MyFiddler.GetInstance().InitFiddler(comboBox1.Text, this);
                qinglong.GetInstance().addRemark(comboBox1.Text);
                comboBox1.Enabled = false;
                bStart = true;
                
                启动.Text = "停止";
                addLog("启动成功");
            }
            else
            {
                addLog("开始停止");
                MyFiddler.GetInstance().Shutdown();
                comboBox1.Enabled = true;
                bStart = false;
                启动.Text = "启动";
                addLog("停止成功");
            }
            button1.Enabled = bStart;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (bStart)
            {
                MyFiddler.GetInstance().ReloadFiddler();
            }
        }

        private void 清屏_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        public void addLog(string log)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                this.richTextBox1.BeginInvoke(new Action<string>(addLog), new object[] { log });
            }
            else
            {
                richTextBox1.AppendText("\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss: ") + log);
                richTextBox1.ScrollToCaret();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            启动_Click(sender, e);
            Form_UserAdd add = new Form_UserAdd();
            add.ShowDialog();
            启动_Click(sender, e);
        }

        private void NetFiddler_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyFiddler.GetInstance().Shutdown();
        }
    }
}
