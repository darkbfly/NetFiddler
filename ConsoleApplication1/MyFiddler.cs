using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fiddler;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using log4net;
using log4net.Config;
using System.Reflection;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyFiddler
    {
        public bool isRunning = false;
        public static int runningData = 0;
        // 静态变量用于存储唯一的实例
        private static MyFiddler instance;
        NetFiddler form = null;
        private static readonly object _lock = new object();
        static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //https代理
        string remark;
        static Proxy oSecureEndpoint;
        //主机名
        string sSecureEndpointHostname = "localhost";
        //伪装https服务器（别人这么说，我也没搞明白这个技术细节）
        int iSecureEndpointPort = 8877;
        //代理端口
        int iStartPort = 8888;
        List<MessageObject> MessageObjects = new List<MessageObject>();
        Dictionary<string, JObject> envs = new Dictionary<string, JObject>();
        // 循环文件夹里面的ini文件

        private MyFiddler()
        {

        }
        public static MyFiddler GetInstance()
        {
            // 如果实例不存在，则创建一个新实例
            if (instance == null)
            {
                instance = new MyFiddler();
            }
            // 返回实例
            return instance;
        }
        public async void InitFiddler(string remark, NetFiddler form = null)
        {
            this.form = form;
            this.remark = remark;
            //这个名字随便
            FiddlerApplication.SetAppDisplayName("test");
            //绑定事件处理————当发起请求之前
            FiddlerApplication.BeforeRequest += On_BeforeRequest;
            //绑定事件处理————当会话结束之后
            //             FiddlerApplication.AfterSessionComplete += On_AfterSessionComplete;

            //-----------处理证书-----------
            //伪造的证书
            X509Certificate2 oRootCert;
            //如果没有伪造过证书并把伪造的证书加入本机证书库中
            if (null == CertMaker.GetRootCertificate())
            {
                //创建伪造证书
                CertMaker.createRootCert();

                //重新获取
                oRootCert = CertMaker.GetRootCertificate();

                //打开本地证书库
                X509Store certStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                certStore.Open(OpenFlags.ReadWrite);
                try
                {
                    //将伪造的证书加入到本地的证书库
                    certStore.Add(oRootCert);
                }
                finally
                {
                    certStore.Close();
                }
            }
            else
            {
                //以前伪造过证书，并且本地证书库中保存过伪造的证书
                oRootCert = CertMaker.GetRootCertificate();
            }

            //-----------------------------

            //指定伪造证书
            FiddlerApplication.oDefaultClientCertificate = oRootCert;
            //忽略服务器证书错误
            CONFIG.IgnoreServerCertErrors = true;
            //信任证书
            CertMaker.trustRootCert();
            //看字面意思知道是啥，但实际起到啥作用。。。鬼才知道，官方例程里有这句，加上吧，管它呢。
            FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);
            //启动代理服务————启动参数1：捕捉https；启动参数2：允许局域网其他终端连入本代理
            FiddlerApplication.Startup(iStartPort, FiddlerCoreStartupFlags.DecryptSSL | FiddlerCoreStartupFlags.AllowRemoteClients | FiddlerCoreStartupFlags.Default, null);
            //创建https代理
            oSecureEndpoint = FiddlerApplication.CreateProxyEndpoint(iSecureEndpointPort, true, oRootCert);

            if (await InitQL() == true)
            {
                ReloadFiddler();
            }


            Debug();
        }

        public async Task<bool> InitQL()
        {
            if (!qinglong.GetInstance().checkLogin())
            {
                if (!await qinglong.GetInstance().Login())
                {
                    Shutdown();
                    return false;
                }
            }
            return true;
        }

        //封包发送之前事件————基于这个Session oS可以做很多很多事
        public async void On_BeforeRequest(Session oS)
        {
            foreach (MessageObject obj in MessageObjects)
            {
                if (obj.host == oS.host)
                {
                    MessageObject item = new MessageObject(form, remark);
                    item.copy(obj);
                    item.analyze(oS.RequestHeaders, oS.GetRequestBodyAsString(), oS.PathAndQuery, oS.fullUrl);
                    if (!item.verify())
                    {
                        continue;
                    }
                    log.Debug($"抓到请求报文 host: {oS.host}");
                    // if (form != null) { form.addLog($"抓到请求报文 host: {oS.host}"); }
                    // 保存文件并根据内容判断是否上传
                    if (item.getEnv().Length > 0)
                    {
                        runningData++;
                        var jsonText = new
                        {
                            name = item.envName,
                            value = item.getEnv(),
                            remark = remark,
                            run = item.iRun == 1 ? true : false,
                            taskName = item.taskName,
                        };

                        string json = JsonConvert.SerializeObject(jsonText);

                        // 将 JSON 字符串写入文件
                        bool bUpdateQL = false;
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"envs_{remark}", $"{item.taskName}.json");
                        // log.Debug($"写入文件路径: {filePath}");
                        lock (_lock) 
                        {
                            if (envs.ContainsKey(item.taskName))
                            {
                                try
                                {
                                    if ((string)envs[item.taskName]["value"] != item.getEnv())
                                    {
                                        envs[item.taskName]["value"] = item.getEnv();
                                        bUpdateQL = true;
                                        File.WriteAllText(filePath, json);
                                    }
                                }
                                catch (Exception e)
                                {
                                    bUpdateQL = false;
                                    log.Error("文件读取异常", e);
                                }
                            }
                            else
                            {
                                bUpdateQL = true;
                                envs.Add(item.taskName, JObject.Parse(json));
                                File.WriteAllText(filePath, json);
                            }
                        }
                        if (bUpdateQL) 
                        {
                            qinglong.GetInstance().addInfo(item.envName, json);
                        }
                        runningData--; 
                    }
                }
            }
        }

        private void Debug() 
        {
        }

        public void Shutdown()
        {
            FiddlerApplication.Shutdown();
            isRunning = false;
        }

        public void ReloadFiddler()
        {
            MessageObjects.Clear(); 
            envs.Clear();

            string fiddlerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fiddler");
            Directory.CreateDirectory(fiddlerPath);
            // 循环处理ini文件
            string[] iniFiles = Directory.GetFiles(fiddlerPath, "*.ini");

            // 遍历INI文件路径并打印
            foreach (string filePath in iniFiles)
            {
                MessageObject data = new MessageObject(form, remark);
                if (data.init(Path.GetFileNameWithoutExtension(filePath), filePath))
                {
                    MessageObjects.Add(data);
                }
            }

            string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"envs_{remark}");
            Directory.CreateDirectory(dirPath);
            string[] jsonFiles = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"envs_{remark}"), "*.json");
            foreach (string filePath in jsonFiles)
            {
                string json = File.ReadAllText(filePath);
                JObject obj = JObject.Parse(json);
                envs.Add(Path.GetFileNameWithoutExtension(filePath), obj);
            }


            log.Info("共加载" + MessageObjects.Count + "条数据");
            if (form != null) { form.addLog("共加载" + MessageObjects.Count + "条数据"); }
            isRunning = true;
            
            qinglong.GetInstance().startListenThread();
        }
    
        public int getRunningData(){ if (runningData <= 0) { runningData = 0; } return runningData; }
    }
}
