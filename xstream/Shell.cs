using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Xstream
{
    public static class Shell
    {
        public const ConsoleColor NOTE_COLOR = ConsoleColor.Green;
        public const ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
        public const ConsoleColor ERROR_COLOR = ConsoleColor.Red;

        public static readonly ConsoleColor DEFAULT_COLOR;

        static Shell()
        {
            // 绝不能在属性后面写，因“.Net 按需加载机制”使属性在第一次使用时才会被赋值，此时原值已发生改变
            DEFAULT_COLOR = Console.ForegroundColor;
        }

        public static Exception Abort(string extra, Exception e, params object[] args)
        {
            e = new XStreamException(extra, e, args);
            if (Trace.Listeners[0] is DefaultTraceListener)
            {
                Error(e.Message);
                PressAnyKeyToContinue();
            }
            else
            {
                MessageBox.Show(e.ToString(), e.Message
                    , MessageBoxButtons.AbortRetryIgnore
                    , MessageBoxIcon.Error);
                Trace.WriteLine(e);// 写日志
            }
            return e;
        }

        public static void PressAnyKeyToContinue()
        {
            Console.Write("请按任意键继续. . .");
            Console.ReadKey();
        }

        public static void Note(string output, params object[] args)
            => WriteLine(output, NOTE_COLOR, true, args);

        public static void Warning(string output, params object[] args)
            => WriteLine(output, WARNING_COLOR, true, args);

        public static void Error(string output, params object[] args)
            => WriteLine(output, ERROR_COLOR, true, args);

        public static void WriteLine(object value) => WriteLine(value.ToString());

        public static void WriteLine(string output, params object[] args)
            => WriteLine(output, DEFAULT_COLOR, false, args);

        public static void WriteLine(string output, ConsoleColor color, bool restore, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(output, args);
            if (restore)
            {
                Console.ForegroundColor = DEFAULT_COLOR;
            }
        }

        public static string WriteReadLine(string output = null, params object[] args)
        {
            if (output != null)
            {
                Console.Write(output, args);
            }
            return Console.ReadLine();
        }
    }

    public class Logger : TraceListener
    {
        string _path = @"debug\{0}.{1}-{2}.log";

        public Logger(string _tokenFilePath)
        {
            _path = string.Format(_path
                , Path.GetFileName(_tokenFilePath)
                , DateTimeOffset.Now.ToString("yyyyMMddHHmmss")
                , Process.GetCurrentProcess().Id);

            FileInfo info = new FileInfo(_path);
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }
        }

        public override void Write(string message) => File.AppendAllText(_path, message);

        public override void WriteLine(string message)
            => File.AppendAllText(_path
                , DateTimeOffset.Now.ToString("[yyyy-MM-dd HH:mm:ss] ")
                + message
                + Environment.NewLine);
    }

    public class XStreamException : Exception
    {
        public XStreamException(string msg) : base(msg) { }
        public XStreamException(string msg, Exception e, params object[] args)
            : base(string.Format(msg, args), e) { }
    }
}
