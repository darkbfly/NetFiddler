using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static log4net.Appender.FileAppender;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using log4net;
using System.Reflection;

namespace ConsoleApplication1
{
    public partial class Form_UserAdd : Form
    {
        List<MessageObject> MessageObjects = new List<MessageObject>();
        MessageObject item;
        static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object _lock = new object();
        public Form_UserAdd()
        {
            InitializeComponent();
            comboBox1.Items.AddRange(qinglong.GetInstance().remarkList.ToArray());

            string fiddlerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fiddler");
            Directory.CreateDirectory(fiddlerPath);
            // 循环处理ini文件
            string[] iniFiles = Directory.GetFiles(fiddlerPath, "*.ini");

            // 遍历INI文件路径并打印
            foreach (string filePath in iniFiles)
            {
                MessageObject data = new MessageObject(null, "");
                if (data.init(Path.GetFileNameWithoutExtension(filePath), filePath))
                {
                    MessageObjects.Add(data);
                }
            }

            comboBox2.Items.AddRange(MessageObjects.Where(x => x.iCanManual == 1).Select(x => x.taskName).ToArray());

            comboBox2.SelectedIndexChanged += (object sender, EventArgs e) =>
            {
                foreach (MessageObject item in MessageObjects)
                {
                    if (item.taskName == comboBox2.Text)
                    {
                        richTextBox1.Text = item.memo.Replace("[ENTER]", "\n");
                        this.item = item;
                    }
                }
            };
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string remark = comboBox1.Text;
            var jsonText = new
            {
                name = item.envName,
                value = richTextBox2.Text,
                remark = remark,
                run = item.iRun == 1 ? true : false,
                taskName = item.taskName,
            };

            string json = JsonConvert.SerializeObject(jsonText);

            // 将 JSON 字符串写入文件
            bool bUpdateQL = false;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"envs_{remark}", $"{item.taskName}.json");
            log.Debug($"写入文件路径: {filePath}");
            lock (_lock)
            {
                bUpdateQL = true;
                File.WriteAllText(filePath, json);
            }

            if (bUpdateQL)
            {
                log.Info("确认更新");
                JArray array = await qinglong.GetInstance().searchEnvs(jsonText.name);
                foreach (JToken jToken in array)
                {
                    if (jsonText.remark == (string)jToken["remarks"] && jsonText.value != (string)jToken["value"])
                    {
                        await qinglong.GetInstance().deleteEnv((int)jToken["id"]);
                    }
                }
                await qinglong.GetInstance().createNewEnv(jsonText.name, jsonText.value, jsonText.remark);
                if (jsonText.run)
                {
                    int id = await qinglong.GetInstance().searchTask(jsonText.taskName);
                    if (id > 0)
                    {
                        await qinglong.GetInstance().runTask(id);
                    }
                }
            }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
