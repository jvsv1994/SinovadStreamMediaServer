using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SinovadMediaServer.Application.Configuration;
using SinovadMediaServer.Transversal.Interface;
using System.Diagnostics;
using System.Reflection;

namespace SinovadMediaServer.Transversal.Logger
{
    public class LoggerAdapter<T> : IAppLogger<T>
    {
        public static IOptions<MyConfig> _config { get; set; }

        public readonly ILogger<T> _logger;

        public LoggerAdapter(IOptions<MyConfig> config, ILoggerFactory loggerFactory)
        {
            _config = config;
            _logger = loggerFactory.CreateLogger<T>();
        }

        public void LogInformation(string message, params object[] args)
        {
            try
            {
                message = GetFinalMessage(new StackTrace().GetFrame(1).GetMethod(), message);
                CustomLog.GetInstance(_config.Value.PathLog).Save(message);
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
                CustomLog.GetInstance(_config.Value.PathLog).Save(message);
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
                CustomLog.GetInstance(_config.Value.PathLog).Save(GetFinalMessage(new StackTrace().GetFrame(1).GetMethod(), message));
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
                CustomLog.GetInstance(_config.Value.PathLog).Save(ex.StackTrace);
                _logger.LogError(ex.StackTrace);
            }
            return message;
        }
    }
}
