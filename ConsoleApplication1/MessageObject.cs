using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication1
{
    class MessageObject
    {
        NetFiddler form;
        static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string name, url, method, host, path, body;
        Dictionary<string, string> headerMap = new Dictionary<string, string>();
        Dictionary<string, string> queriesMap = new Dictionary<string, string>();
        Dictionary<string, string> parseBodyMap = new Dictionary<string, string>();
        Dictionary<string, string> cookieMap = new Dictionary<string, string>();
        string fullUrl;
        JObject jsonBody;
        public int iCheckEnable, iRun, iCanManual;
        public string checkInfo, env, envName, taskName, regexReplace, regexReplaceValue, checkValue, memo;
        public string remark;

        public MessageObject(NetFiddler form, string remark) { this.form = form; this.remark = remark; }

        public bool init(string name, string iniPath) {
            bool bRet = false;
            host = IniFunc.getString(name, "host", "", iniPath);
            env = IniFunc.getString(name, "env", "", iniPath);
            iCheckEnable = IniFunc.GetInt(name, "checkEnable", 0, iniPath);
            iRun = IniFunc.GetInt(name, "runEnable", 0, iniPath);
            taskName = IniFunc.getString(name, "taskName", "", iniPath);
            envName = IniFunc.getString(name, "envName", "", iniPath);
            regexReplace = IniFunc.getString(name, "regexReplace", "", iniPath);
            regexReplaceValue = IniFunc.getString(name, "regexReplaceValue", "", iniPath);
            checkValue = IniFunc.getString(name, "checkValue", "", iniPath);
            memo = IniFunc.getString(name, "memo", "", iniPath);
            iCanManual = IniFunc.GetInt(name, "canManual", 0, iniPath);

            if (host == null || host.Length == 0)
            {
                log.Error(iniPath + "配置文件中, host 为空");
                if (form != null) { form.addLog(iniPath + "配置文件中, host 为空"); }
                goto cleanUP;
            }
            if (env == null || env.Length == 0)
            {
                log.Error(iniPath + "配置文件中, env 为空");
                if (form != null) { form.addLog(iniPath + "配置文件中, env 为空"); }
                goto cleanUP;
            }
            if (envName == null || envName.Length == 0)
            {
                log.Error(iniPath + "配置文件中, envName 为空");
                if (form != null) { form.addLog(iniPath + "配置文件中, envName 为空"); }
                goto cleanUP;
            }
            bRet = true;
        cleanUP:
            return bRet;
        }

        public void copy(MessageObject obj) {
            host = obj.host;
            env = obj.env;
            iCheckEnable = obj.iCheckEnable;
            iRun = obj.iRun;
            iCanManual = obj.iCanManual;
            taskName = obj.taskName;
            envName = obj.envName;
            regexReplace = obj.regexReplace;
            regexReplaceValue = obj.regexReplaceValue;
            checkValue = obj.checkValue;
            memo = obj.memo;
        }

        public bool verify() {
            bool bRet = false;
            if (iCheckEnable != 1)
            {
                bRet = true;
                goto cleanUP;
            }
            List<string> strings = checkValue.Split('|').ToList();
            strings.ForEach(x =>
            {
                string []list = x.Split('=');
                
                switch (list[0])
                {
                    case "headers":
                    case "header":
                        if (headerMap.Keys.Where(y => y.Contains(list[1])).Count() > 0 || headerMap.Values.Where(y => y.Contains(list[1])).Count() > 0)
                        { 
                            bRet = true;
                        }
                        break;
                    case "cookies":
                    case "cookie":
                        if (cookieMap.Keys.Where(y => y.Contains(list[1])).Count() > 0 || cookieMap.Values.Where(y => y.Contains(list[1])).Count() > 0)
                        {
                            bRet = true;
                        }
                        break;
                    case "body":
                        if (body.Contains(list[1])) 
                        { 
                            bRet = true;
                        }
                        break;
                    case "queries":
                    case "query":
                        if (queriesMap.Keys.Where(y => y.Contains(list[1])).Count() > 0 || queriesMap.Values.Where(y => y.Contains(list[1])).Count() > 0)
                        {
                            bRet = true;
                        }
                        break;
                }
            });

        cleanUP:
            return bRet;
        }

        public void analyze(Fiddler.HTTPRequestHeaders headers, string body, string query, string url) {
            fullUrl = url;
            foreach (Fiddler.HTTPHeaderItem header in headers)
            {
                headerMap[header.Name.ToLower()] = header.Value;
            }
            try
            {
                if (headers["Cookie"] != null)
                {
                    string[] strings = headers["Cookie"].Split(new char[] { ';' });
                    foreach (string s in strings)
                    {
                        if (s.Split('=').Length == 2)
                        {
                            string key = s.Split('=')[0];
                            string value = s.Split('=')[1];
                            cookieMap[key.ToLower()] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("报文里的Cookie解析失败", ex);
                if (form != null) { form.addLog("报文里的Cookie解析失败"); }
            }

            this.body = body;
            if (this.body.Length > 0)
            {
                try
                {
                    if (IsJson(this.body))
                    {
                        jsonBody = JObject.Parse(this.body);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("报文json解析失败", ex);
                    if (form != null) { form.addLog("报文json解析失败"); }
                }

                try
                {
                    if (!IsJson(this.body))
                    {
                        string[] strings = body.Split(new char[] { '&' });
                        foreach (string s in strings)
                        {
                            if (s.Split('=').Length == 2)
                            {
                                string key = s.Split('=')[0];
                                string value = s.Split('=')[1];
                                parseBodyMap[key.ToLower()] = value;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"报文分割解析失败\n{this.body}", ex);
                    if (form != null) { form.addLog("报文分割解析失败"); }
                }

            }

            if (query.Length > 0)
            {
                query = query.Substring(query.IndexOf('?') + 1);
                try
                {
                    string[] strings = query.Split(new char[] { '&' });
                    foreach (string s in strings)
                    {
                        if (s.Split('=').Length == 2)
                        {
                            string key = s.Split('=')[0];
                            string value = s.Split('=')[1];
                            queriesMap[key.ToLower()] = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"报文分割解析失败\n {query}", ex);
                    if (form != null) { form.addLog("报文分割解析失败"); }
                }
            }
        }
        public string getEnv()
        {
            JObject obj;
            string result = "";
            string pattern = @"\^([^\\^]+)\^";
            for (int i = 0; i < env.Length; i++)
            {
                // 将字符转换为整数表示并输出其值
                if (env[i] == '^')
                {
                    MatchCollection matches = Regex.Matches(env.Substring(i), pattern);
                    Match match = matches[0];
                    string pattern1 = @"\[(.*?)\]";
                    MatchCollection matches1 = new Regex(pattern1).Matches(match.Groups[1].Value);
                    string data = null;
                    if (matches1.Count > 0) 
                    {
                        string outerContent = match.Groups[1].Value.Substring(0, matches1[0].Index).ToLower();
                        string innerContent = matches1[0].Value.ToLower().Replace("[", "").Replace("]", "").ToLower(); 
                        switch (outerContent)
                        {
                            case "remark":
                            case "remarks":
                                data = remark;
                                break;
                            case "url":
                                if (fullUrl.Contains(innerContent))
                                {
                                    data = fullUrl;
                                }
                                break;
                            case "headers":
                            case "header":
                                data = (headerMap.ContainsKey(innerContent)) ? headerMap[innerContent] : null;
                                break;
                            case "jsonheaders":
                            case "jsonheader":
                                List<Match> remainingMatches = new List<Match>();
                                for (int j = 1; j < matches1.Count; j++)
                                {
                                    remainingMatches.Add(matches1[j]);
                                }
                                string key = matches1[0].Groups[1].Value.ToLower();
                                string header = (headerMap.ContainsKey(key)) ? headerMap[key] : null;
                                if (header != null)
                                {
                                    try
                                    {
                                        obj = JObject.Parse(header);
                                        foreach (Match matchlist in remainingMatches)
                                        {
                                            string msg = matchlist.Groups[1].Value.Replace("[", "").Replace("]", "");
                                            if (!obj.ContainsKey(msg))
                                            {
                                                data = null;
                                                break;
                                            }
                                            else
                                            {
                                                if (obj[msg] is JObject)
                                                {
                                                    Console.WriteLine(obj[msg].ToString());
                                                    obj = (JObject)obj[msg];
                                                }
                                                else
                                                {
                                                    data = obj[msg].ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(ex.ToString());
                                        data = null;
                                    }
                                }
                                else { data = null; }
                                break;
                            case "cookies":
                            case "cookie":
                                data = (cookieMap.ContainsKey(innerContent)) ? cookieMap[innerContent] : null;
                                break;
                            case "parsebody":
                                data = (parseBodyMap.ContainsKey(innerContent)) ? parseBodyMap[innerContent] : null;
                                break;
                            case "jsonbody":
                                obj = jsonBody;
                                foreach (Match matchlist in matches1)
                                {
                                    string msg = matchlist.Groups[1].Value.Replace("[", "").Replace("]", "");
                                    if (!obj.ContainsKey(msg))
                                    {
                                        data = null;
                                        break;
                                    }
                                    else
                                    {
                                        if (obj[msg] is JObject)
                                        {
                                            obj = (JObject)obj[msg];
                                        }
                                        else 
                                        {
                                            data = obj[msg].ToString();
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "queries":
                            case "query":
                                data = (queriesMap.ContainsKey(innerContent)) ? queriesMap[innerContent] : null;
                                break;
                            default:
                                data = null;
                                break;
                        }
                        if (data != null)
                        {
                            result += data;
                        }
                        else
                        {
                            log.Error($"环境变量解析错误: {outerContent}[{innerContent}]");
                            result = "";
                            break;
                        }
                    }
                    
                    i += match.Groups[1].Value.Length + 1;
                }
                else
                {
                    result += env[i];
                }
            }
            if (regexReplace.Length > 0 && result.Length > 0)
            {
                result = Regex.Replace(result, regexReplace, regexReplaceValue);
            }
            return result;
        }

        public string getEnv_old()
        {
            string result = "";
            string pattern = @"\^([^\\^]+)\^";
            for (int i = 0; i < env.Length; i++)
            {
                // 将字符转换为整数表示并输出其值
                if (env[i] == '^')
                {
                    MatchCollection matches = Regex.Matches(env.Substring(i), pattern);
                    Match match = matches[0];
                    string pattern1 = @"\[(.*?)\]";
                    Match match1 = Regex.Match(match.Groups[1].Value, pattern1);
                    i += match.Groups[1].Value.Length + 1;
                    if (match1.Success)
                    {
                        string outerContent = match.Groups[1].Value.Substring(0, match1.Index).ToLower();
                        string innerContent = match1.Groups[1].Value.ToLower();
                        string data = "";
                        switch (outerContent)
                        {
                            case "headers":
                            case "header":
                                data = (headerMap.ContainsKey(innerContent)) ? headerMap[innerContent] : null;
                                break;
                            case "cookies":
                            case "cookie":
                                data = (cookieMap.ContainsKey(innerContent)) ? cookieMap[innerContent] : null;
                                break;
                            case "parsebody":
                                data = (parseBodyMap.ContainsKey(innerContent)) ? parseBodyMap[innerContent] : null;
                                break;
                            case "jsonbody":
                                data = (jsonBody != null && jsonBody.ContainsKey(innerContent)) ? jsonBody[innerContent].ToString() : null;
                                break;
                            case "queries":
                            case "query":
                                data = (queriesMap.ContainsKey(innerContent)) ? queriesMap[innerContent] : null;
                                break;
                            default:
                                data = null;
                                break;
                        }
                        if (data != null)
                        {
                            result += data;
                        }
                        else
                        {
                            result = "";
                            break;
                        }
                    }
                    else
                    {
                        result = "";
                        break;
                    }
                }
                else
                {
                    result += env[i];
                }
            }
            if (regexReplace.Length > 0 && result.Length > 0)
            {
                result = Regex.Replace(result, regexReplace, regexReplaceValue);
            }
            return result;
        }

        static private bool IsJson(string str)
        {
            try
            {
                JToken.Parse(str);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }
    }
}
