using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logviewer
{
    // 一个日志文件
    class LogFile
    {
        class LogEntryCursor
        {
            public int offset;
            public int length;
            public int index;

            public LogEntryCursor(int offset, int length)
            {
                this.offset = offset;
                this.length = length;
                this.index = -1;
            }
        }

        private byte[] _buf;
        private readonly string _name;
        private readonly string _filename; // 日志文件名
        private System.IO.BufferedStream _fs;
        private List<LogEntryCursor> _cursors = new List<LogEntryCursor>();
        private List<LogEntry> _entries = new List<LogEntry>();

        public string Name {
            get { return _name; }
        }

        public string FullName {
            get { return _filename; }
        }

        public int Count {
            get { return _cursors.Count; }
        }

        public LogEntry GetEntry(int index)
        {
            var cursor = _cursors[index];
            if (cursor.index < 0)
            {
                _fs.Seek(cursor.offset, System.IO.SeekOrigin.Begin);
                _fs.Read(_buf, 0, cursor.length);
                var newEntry = new LogEntry();
                newEntry.Parse(Encoding.UTF8.GetString(_buf, 0, cursor.length));
                cursor.index = _entries.Count;
                _entries.Add(newEntry);
            }
            return _entries[cursor.index];
        }

        public LogFile(string fp)
        {
            _name = System.IO.Path.GetFileName(fp);
            _filename = fp;
            _fs = new System.IO.BufferedStream(System.IO.File.OpenRead(fp), 1024 * 32);
            var startPos = 0;
            var endPos = 0;
            var cap = 0;
            do
            {
                endPos++;
                var ch = _fs.ReadByte();
                if (ch >= 0)
                {
                    if (ch == '\n')
                    {
                        var ncap = endPos - startPos;
                        if (ncap > cap)
                        {
                            cap = ncap;
                        }
                        _cursors.Add(new LogEntryCursor(startPos, ncap));
                        startPos = endPos;
                    }
                    continue;
                }
                break;
            } while (true);
            _buf = new byte[cap];
        }
    }
}
