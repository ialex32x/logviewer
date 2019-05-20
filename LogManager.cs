using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logviewer
{
    class LogManager
    {
        private static LogManager _instance = new LogManager();

        public Dictionary<string, LogFolder> folders = new Dictionary<string, LogFolder>();

        public static LogManager GetInstance()
        {
            return _instance;
        }

        public LogFolder AddFolder(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                var fp = System.IO.Path.GetFullPath(path);
                LogFolder logFolder;
                if (!folders.TryGetValue(fp, out logFolder))
                {
                    logFolder = new LogFolder(fp);
                    folders[fp] = logFolder;
                }
                return logFolder;
            }
            return null;
        }
    }
}
