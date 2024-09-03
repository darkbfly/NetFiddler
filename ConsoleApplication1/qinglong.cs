using ConsoleApp1;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static log4net.Appender.FileAppender;

namespace ConsoleApplication1
{
    class qinglong
    {
        // 静态变量用于存储唯一的实例
        private Dictionary<string, string> qinglongDic = new Dictionary<string, string>();
        private static qinglong instance;

        private static readonly object _lock = new object();
        private qinglong() { }

        public static qinglong GetInstance()
        {
            // 如果实例不存在，则创建一个新实例
            if (instance == null)
            {
                instance = new qinglong();
            }
            // 返回实例
            return instance;
        }
        static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string ip, client_id, client_secret, iniPath, token;
        private int port;
        public string[] remarkList;
        NetFiddler form;
        public bool Init(NetFiddler form)
        {
            this.form = form;
            bool bRet = false;
            iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "qinglong.ini");
            ip = IniFunc.getString("ql", "ip", "", iniPath);
            port = IniFunc.GetInt("ql", "port", -1, iniPath);
            client_id = IniFunc.getString("ql", "client_id", "", iniPath);
            client_secret = IniFunc.getString("ql", "client_secret", "", iniPath);
            token = IniFunc.getString("ql", "token", "", iniPath);
            remarkList = IniFunc.getString("ql", "remark", "", iniPath).Split('|');
            if (port <= 0)
            {
                log.Error(iniPath + "配置文件中, port 不正确");
                if (form != null) { form.addLog(iniPath + "配置文件中, port 不正确"); }
                goto cleanUP;
            }
            if (ip == null || ip.Length == 0)
            {
                log.Error(iniPath + "配置文件中, ip 为空");
                if (form != null) { form.addLog(iniPath + "配置文件中, ip 为空"); }
                goto cleanUP;
            }
            if (client_id == null || client_id.Length == 0)
            {
                log.Error(iniPath + "配置文件中, client_id 为空");
                if (form != null) { form.addLog(iniPath + "配置文件中, client_id 为空"); }
                goto cleanUP;
            }
            if (client_secret == null || client_secret.Length == 0)
            {
                log.Error(iniPath + "配置文件中, client_secret 为空");
                if (form != null) { form.addLog(iniPath + "配置文件中, client_secret 为空"); }
                goto cleanUP;
            }
            bRet = true;
        cleanUP:
            return bRet;
        }
    
        public async Task<bool> Login()
        {
            token = "";
            bool bRet = false;
            string url = $"http://{ip}:{port}/open/auth/token?client_id={client_id}&client_secret={client_secret}";
            JObject obj = await httpGet(url);
            if (obj != null && (int)obj["code"] == 200)
            {
                bRet = true;
                token = (string)obj["data"]["token"];
                IniFunc.writeString("ql", "token", token, iniPath);
            }
            return bRet;
        }
        
        public bool checkLogin()
        {
            return false;
        }

        public async Task<JObject> httpGet(string url)
        {
            log.Debug($"url: {url}");
            JObject obj = null;
            using (HttpClient client = new HttpClient())
            {
                if (token.Length > 0)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                // 创建HttpContent对象，并设置内容类型
                HttpContent content = new StringContent("", Encoding.UTF8, "application/json");

                try
                {
                    // 发送POST请求
                    HttpResponseMessage response = await client.GetAsync(url);

                    // 检查响应是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        // 读取响应内容
                        string responseContent = await response.Content.ReadAsStringAsync();
                        obj = JObject.Parse(responseContent);
                    }
                    else
                    {
                        Console.WriteLine($"HTTP Error: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
            return obj;
        }

        public async Task<JObject> httpPost(string url, string data)
        {
            log.Debug($"url: {url}, data: {data}");
            JObject obj = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // 创建HttpContent对象，并设置内容类型
                HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");

                try
                {
                    // 发送POST请求
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // 检查响应是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        // 读取响应内容
                        string responseContent = await response.Content.ReadAsStringAsync();
                        obj = JObject.Parse(responseContent);
                    }
                    else
                    {
                        Console.WriteLine($"HTTP Error: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
            return obj;
        }

        public async Task<JObject> httpPut(string url, string data)
        {
            log.Debug($"url: {url}, data: {data}");
            JObject obj = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // 创建HttpContent对象，并设置内容类型
                HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");

                try
                {
                    // 发送POST请求
                    HttpResponseMessage response = await client.PutAsync(url, content);

                    // 检查响应是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        // 读取响应内容
                        string responseContent = await response.Content.ReadAsStringAsync();
                        obj = JObject.Parse(responseContent);
                    }
                    else
                    {
                        Console.WriteLine($"HTTP Error: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
            return obj;
        }
    
        public async Task<JObject> httpDelete(string url, string data)
        {
            log.Debug($"url: {url}, data: {data}");
            JObject obj = null;
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = new StringContent(data, Encoding.UTF8, "application/json");

                try
                {
                    // 发送POST请求
                    HttpResponseMessage response = await client.SendAsync(request);

                    // 检查响应是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        // 读取响应内容
                        string responseContent = await response.Content.ReadAsStringAsync();
                        obj = JObject.Parse(responseContent);
                    }
                    else
                    {
                        Console.WriteLine($"HTTP Error: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
            return obj; 
        }
        public async Task<JArray> searchEnvs(string name)
        {
            JArray data = new JArray();
            string url = $"http://{ip}:{port}/open/envs";
            JObject obj = await httpGet(url);
            if (obj != null && (int)obj["code"] == 200)
            {
                foreach (JToken jToken in obj["data"])
                {
                    if (jToken["name"].ToString() == name)
                    {
                        data.Add(jToken);
                    }
                }
            }
            return data;
        }

        public async Task<bool> deleteEnv(int id)
        {
            bool bRet = false;
            string url = $"http://{ip}:{port}/open/envs";
            JObject obj = await httpDelete(url, $"[{id}]");
            if (obj != null && (int)obj["code"] == 200)
            {
                bRet = true;
            }
            return bRet;
        }

        public async Task<bool> createNewEnv(string name, string value, string remark = "")
        {
            bool bRet = false;
            string url = $"http://{ip}:{port}/open/envs";

            JObject data = new JObject();
            JArray array = new JArray();
            data["name"] = name;
            data["value"] = value;
            data["remarks"] = remark;
            array.Add(data);

            JObject obj = await httpPost(url, array.ToString());
            if (obj != null && (int)obj["code"] == 200)
            {
                bRet = true;
            }
            return bRet;
        }

        public async Task<int> searchTask(string name)
        {
            string url = $"http://{ip}:{port}/open/crons";
            JObject obj =  await httpGet(url);
            if (obj != null && (int)obj["code"] == 200)
            {
                foreach (JToken jToken in obj["data"]["data"])
                {
                    if ((int)jToken["isDisabled"] == 0 && (string)jToken["name"] == name)
                    {
                        return (int)jToken["id"];
                    }
                }
            }
            return -1;
        }
    
        public async Task<bool> runTask(int id)
        {
            bool bRet = false;
            string url = $"http://{ip}:{port}/open/crons/run";
            JObject obj =  await httpPut(url, $"[{id}]");
            if (obj != null && (int)obj["code"] == 200)
            {
                bRet = true;
            }
            return bRet;
        }

        public void addRemark(string data)
        {
            if (data != null && !remarkList.Contains(data))
            {
                string remark = IniFunc.getString("ql", "remark", "", iniPath);
                remark += data;
                remark += "|";
                IniFunc.writeString("ql", "remark", remark, iniPath);
            }
        }

        public void startListenThread()
        {
            new Thread(new ThreadStart(uploadThread)).Start();
        }

        public void addInfo(string key, string jsonData)
        {
            lock (_lock)
            {
                Console.WriteLine("加入数据: " + key);
                qinglongDic[key] = jsonData;
            }
        }

        async static void uploadThread() {
            List<string> keysToRemove = new List<string>();
            do
            {
                Thread.Sleep(2000);
                keysToRemove.Clear();
                if (MyFiddler.GetInstance().getRunningData() <= 0 && qinglong.GetInstance().qinglongDic.Count() > 0)
                {
                    foreach (string jsonMsg in qinglong.GetInstance().qinglongDic.Values)
                    {
                        JObject obj = JObject.Parse(jsonMsg);
                        var jsonText = new
                        {
                            name = obj["name"].ToString(),
                            value = obj["value"].ToString(),
                            remark = obj["remark"].ToString(),
                            run = obj["run"].Value<bool>(),
                            taskName = obj["taskName"].ToString(),
                        };
                        Program.ShowNotification(jsonText.name, "青龙上传变量");
                        if (qinglong.GetInstance().form != null) { qinglong.GetInstance().form.addLog("青龙上传变量 " + jsonText.name); }
                       
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

                        if (jsonText.name != null)
                        {
                            keysToRemove.Add(jsonText.name);
                        }
                    }


                    lock (_lock)
                    {
                        foreach (string key in keysToRemove)
                        {
                            qinglong.GetInstance().qinglongDic.Remove(key);
                        }
                    }
                }
            } while (MyFiddler.GetInstance().isRunning);
        }
    }
}
