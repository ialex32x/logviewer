using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logviewer
{
    // 一个目录包含的所有日志
    class LogFolder
    {
        private string _name;
        private string _foldername;
        private List<LogFile> _files = new List<LogFile>();

        public string Name {
            get { return _name; }
        }

        public string FullName {
            get { return _foldername; }
        }

        public int Count {
            get { return _files.Count; }
        }

        public LogFile GetFile(int index)
        {
            return _files[index];
        }

        public LogFolder(string fp)
        {
            _name = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(fp));
            _foldername = fp;
            var files = System.IO.Directory.GetFiles(fp);
            foreach (var file in files)
            {
                _files.Add(new LogFile(file));
            }
        }
    }
}
