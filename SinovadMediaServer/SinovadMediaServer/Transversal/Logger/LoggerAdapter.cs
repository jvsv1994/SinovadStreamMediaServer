using Microsoft.Extensions.Logging;
using SinovadMediaServer.Transversal.Interface;
using System.Diagnostics;
using System.Reflection;

namespace SinovadMediaServer.Transversal.Logger
{
    public class LoggerAdapter<T> : IAppLogger<T>
    {

        public readonly ILogger<T> _logger;

        private string _pathLog;

        public LoggerAdapter( ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<T>();
            var rootPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sinovad Media Server");
            var logsPath = Path.Combine(rootPath, "Logs");
            var pathLog = Path.Combine(logsPath, "log.txt");
            _pathLog = pathLog;
        }

        public void LogInformation(string message, params object[] args)
        {
            try
            {
                message = GetFinalMessage(new StackTrace().GetFrame(1).GetMethod(), message);
                CustomLog.GetInstance(_pathLog).Save(message);
                _logger.LogInformation(message, args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
        }

        public void LogWarning(string message, params object[] args)
        {
            try
            {
                message = GetFinalMessage(new StackTrace().GetFrame(1).GetMethod(), message);
                CustomLog.GetInstance(_pathLog).Save(message);
                _logger.LogWarning(message, args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
        }

        public void LogError(string message, params object[] args)
        {
            try
            {
                message = GetFinalMessage(new StackTrace().GetFrame(1).GetMethod(), message);
                CustomLog.GetInstance(_pathLog).Save(GetFinalMessage(new StackTrace().GetFrame(1).GetMethod(), message));
                _logger.LogError(message, args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
        }

        private string GetFinalMessage(MethodBase methodBase, string message)
        {
            try
            {
                var methodName = methodBase.DeclaringType.Name;
                var className = methodBase.DeclaringType.DeclaringType != null ? methodBase.DeclaringType.DeclaringType.Name : methodBase.DeclaringType.Name;
                var nameSpace = methodBase.DeclaringType.DeclaringType != null ? methodBase.DeclaringType.DeclaringType.Namespace : methodBase.DeclaringType.Namespace;
                message = nameSpace + " - " + className + " - " + methodName + " - " + message;
            }
            catch (Exception ex)
            {
                CustomLog.GetInstance(_pathLog).Save(ex.StackTrace);
                _logger.LogError(ex.StackTrace);
            }
            return message;
        }
    }
}
