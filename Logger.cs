using System;
using System.IO;

namespace BallBotGui
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
        private static readonly object LockObj = new object();

        public static void Log(string message, Exception? ex = null)
        {
            try
            {
                lock (LockObj)
                {
                    string content = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                    if (ex != null)
                    {
                        content += $"Exception: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}{Environment.NewLine}";
                        if (ex.InnerException != null)
                        {
                            content += $"Inner Exception: {ex.InnerException.Message}{Environment.NewLine}";
                        }
                    }
                    content += "--------------------------------------------------" + Environment.NewLine;
                    File.AppendAllText(LogPath, content);
                }
            }
            catch
            {
                // Если даже логгер упал, ничего не поделаешь
            }
        }
    }
}
