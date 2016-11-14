using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// https://ef.readthedocs.io/en/latest/miscellaneous/logging.html

namespace Models
{
    public class LoggerProvider : ILoggerProvider
    {

        public ILogger CreateLogger(string categoryName)
        {
            // if (categoryName != "Microsoft.EntityFrameworkCore.Storage.Internal.SqliteRelationalConnection" &&
            //     (categoryName.IndexOf("Relational", StringComparison.CurrentCultureIgnoreCase) >= 0 ||
            //     categoryName.IndexOf("Sqlite", StringComparison.CurrentCultureIgnoreCase) >= 0))
            // {
                return new DbLogger { Category = categoryName };
            // }

            // return new NullLogger();
        }

        public static void RegisterLogger(DbContext context)
        {
            var serviceProvider = context.GetInfrastructure<IServiceProvider>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new LoggerProvider());
        }

        public void Dispose()
        { }

        private class DbLogger : ILogger
        {
            public string Category { get; set; }

            public string LogFile { get; private set; } 
 
            public DbLogger()
            {
                LogFile = @"c:\temp\dblog.txt";

                try 
                {
                    System.IO.File.Delete(LogFile);
                }
                catch 
                {
                }
            }
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                System.IO.File.AppendAllText(LogFile, "[" + Category + "]\n" + formatter(state, exception) + "\n");

                // var col = Console.ForegroundColor;
                // Console.ForegroundColor = ConsoleColor.DarkMagenta;
                // Console.WriteLine(formatter(state, exception));
                // Console.ForegroundColor = col;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }

        private class NullLogger : ILogger
        {
            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            { }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }

}
