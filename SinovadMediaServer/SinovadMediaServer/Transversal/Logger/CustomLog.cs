using System;
using System.IO;

namespace SinovadMediaServer.Transversal.Logger
{
    public sealed class CustomLog
    {
        private static CustomLog _instance = null;
        private string _path;
        private static object _protect = new object();
        public static CustomLog GetInstance(string path)
        {
            lock (_protect)
            {
                if (_instance == null)
                {
                    _instance = new CustomLog(path);
                }
            }
            return _instance;
        }
        private CustomLog(string path)
        {
            _path = path;
        }

        public void Save(string message)
        {
            var date = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss ");
            var finalMessage = date + " - " + message;
            try
            {
                File.AppendAllText(_path, finalMessage + Environment.NewLine);
            }
            catch (Exception)
            {

            }
        }
    }
}
