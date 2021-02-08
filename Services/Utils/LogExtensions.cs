using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebRTCServer
{
    public static class LogExtensions
    {
        
        //public static async Task Debug(this ILogger logger,Func<Task<String>> callback,[CallerFilePath]string callerFilePath = null, [CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
        public static  void Debug(this ILogger logger,Func<String> callback,[CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
        {

            if (logger.IsEnabled(LogLevel.Debug))
            {
                var message= callback();
                logger.LogDebug($"[ac] {callerMemberName}:{callerLineNumber} | {message}");
            }
        }
        public static  void  Trace(this ILogger logger,Func<String> callback,[CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                var message= callback();
                logger.LogTrace($"[ac] {callerMemberName}:{callerLineNumber} | {message}");
            }
        }
        public static void  Info(this ILogger logger,Func<String> callback,[CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var message= callback();
                logger.LogInformation($"[ac] {callerMemberName}:{callerLineNumber} | {message}");
            }
        }
        public static  void  Warn(this ILogger logger,Func<String> callback,[CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                var message= callback();
                logger.LogWarning(message);
            }
        }
        public static void  Error(this ILogger logger,Func<String> callback,[CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                var message=callback();
                logger.LogError($"[ac] {callerMemberName}:{callerLineNumber} | {message}");
            }
        }
        public static  void  Critical(this ILogger logger,Func<String> callback,[CallerMemberName]string callerMemberName = null, [CallerLineNumber]int callerLineNumber = 0)
        {
            if (logger.IsEnabled(LogLevel.Critical))
            {
                var message= callback();
                logger.LogCritical($"[ac] {callerMemberName}:{callerLineNumber} | {message}");
            }
        }
    }
}