﻿using System;
using System.Threading.Tasks;
using NLog;

namespace NzbDrone.Common.Instrumentation
{
    public static class GlobalExceptionHandlers
    {
        private static readonly Logger Logger = LogManager.GetLogger("Global");

        public static void Register()
        {
            ExceptronTarget.Register();

            AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e.ExceptionObject as Exception));
            TaskScheduler.UnobservedTaskException += ((s, e) => TaskException(e.Exception));
        }

        private static void TaskException(Exception exception)
        {
            Console.WriteLine("Task Error: {0}", exception);
            Logger.Error("Task Error: " + exception.Message, exception);
        }

        private static void AppDomainException(Exception exception)
        {
            Console.WriteLine("EPIC FAIL: {0}", exception);
            Logger.FatalException("EPIC FAIL: " + exception.Message, exception);
        }
    }
}