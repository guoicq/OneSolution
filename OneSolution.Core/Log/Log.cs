using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OneSolution.Core.Log
{
    public static class Log
    {
        private static ILogger logger = NullLogger.Instance;

        public static void Init(ILoggerFactory factory, string appName)
        {
            if (logger == NullLogger.Instance)
                logger = factory.CreateLogger(appName);
        }

        public static ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public static void Trace(string message, params object[] args)
        {
            logger.LogTrace(message, args);
        }
        public static void Trace(Exception exception, string message, params object[] args)
        {
            logger.LogTrace(exception, message, args);
        }
        public static void Debug(string message, params object[] args)
        {
            logger.LogDebug(message, args);
        }
        public static void Debug(Exception exception, string message, params object[] args)
        {
            logger.LogDebug(exception, message, args);
        }
        public static void Information(string message, params object[] args)
        {
            logger.LogInformation(message, args);
        }
        public static void Information(Exception exception, string message, params object[] args)
        {
            logger.LogInformation(exception, message, args);
        }
        public static void Warning(string message, params object[] args)
        {
            logger.LogWarning(message, args);
        }
        public static void Warning(Exception exception, string message, params object[] args)
        {
            logger.LogWarning(exception, message, args);
        }
        public static void Error(string message, params object[] args)
        {
            logger.LogError(message, args);
        }
        public static void Error(Exception exception, string message, params object[] args)
        {
            logger.LogError(exception, message, args);
        }
        public static void Critical(string message, params object[] args)
        {
            logger.LogCritical(message, args);
        }
        public static void Critical(Exception exception, string message, params object[] args)
        {
            logger.LogCritical(exception, message, args);
        }
    }
}
