using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logviewer
{
    enum LogLevel
    {
        DEBUG,
        INFO,
        WARN,
        ERROR,
        DPANIC,
        PANIC,
        FATAL,
    }

    // 日志项
    class LogEntry
    {
        public DateTime time;
        public LogLevel level;
        public string name = ""; // stub
        public string caller = "";
        public string message = "";
        public string stacktrace = "";

        public Dictionary<string, object> objects;

        private bool GetValue<T>(Dictionary<string, object> dict, string key, out T value)
            where T : class
        {
            object obj = null;
            value = null;
            if (dict.TryGetValue(key, out obj))
            {
                value = obj as T;
            }
            return value != null;
        }

        public void Parse(string fulltext)
        {
            try
            {
                var js = Newtonsoft.Json.JsonSerializer.CreateDefault();
                objects = js.Deserialize<Dictionary<string, object>>(new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(fulltext)));
                if (objects != null)
                {
                    object tmTime;
                    string strLevel = null;
                    string strCaller = null;
                    string strMessage = null;
                    string strStacktrace = null;
                    if (GetValue(objects, "T", out tmTime) && tmTime is DateTime)
                    {
                        time = (DateTime)tmTime;
                    }
                    if (GetValue(objects, "L", out strLevel))
                    {
                        Enum.TryParse<LogLevel>(strLevel, out level);
                    }
                    if (GetValue(objects, "C", out strCaller))
                    {
                        caller = strCaller;
                    }
                    if (GetValue(objects, "M", out strMessage))
                    {
                        message = strMessage;
                    }
                    if (GetValue(objects, "S", out strStacktrace))
                    {
                        stacktrace = strStacktrace;
                    }
                }
                //Console.WriteLine("read object {0}", obj);
            }
            catch (Exception)
            {
            }
            //Console.WriteLine(fulltext);
        }
    }
}
