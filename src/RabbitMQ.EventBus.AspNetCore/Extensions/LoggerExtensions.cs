using System;
using Microsoft.Extensions.Logging;

namespace RabbitMQ.EventBus.Extension.AspNetCore.Extensions
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> InformationAction;
        private static readonly Action<ILogger, string, Exception> WarningAction;
        private static readonly Action<ILogger, string, Exception> ErrorAction;
        static LoggerExtensions()
        {
            InformationAction = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(WriteLog)), "RabbitMQEventBus {LogContent}");
            WarningAction = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1, nameof(WriteLog)), "RabbitMQEventBus {LogContent}");
            ErrorAction = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, nameof(WriteLog)), "RabbitMQEventBus {LogContent}");
        }

        public static void WriteLog(this ILogger logger, LogLevel level, string LogContent)
        {
            switch (level)
            {
                case LogLevel.Error:
                    {
                        ErrorAction(logger, LogContent, null);
                        break;
                    }
                case LogLevel.Warning:
                    {
                        WarningAction(logger, LogContent, null);
                        break;
                    }
                case LogLevel.Trace:
                    break;
                case LogLevel.Debug:
                    break;
                case LogLevel.Information:
                    break;
                case LogLevel.Critical:
                    break;
                case LogLevel.None:
                    break;
                default:
                    {
                        InformationAction(logger, LogContent, null);
                        break;
                    }
            }
        }
    }
}
