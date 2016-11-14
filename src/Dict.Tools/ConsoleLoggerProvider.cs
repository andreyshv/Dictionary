using System;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Logging
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger(categoryName);
        }

        public static void RegisterLogger(DbContext context)
        {
            var serviceProvider = context.GetInfrastructure<IServiceProvider>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new ConsoleLoggerProvider());
        }

        public void Dispose()
        { }
    }

    public static class LoggerHelper
    {
        private static ILoggerFactory _factory;
        
        public static ILogger<T> CreateLogger<T>()
        {
            if (_factory == null)
            {
                _factory = new LoggerFactory();
                _factory.AddProvider(new ConsoleLoggerProvider());
            }

            return new Logger<T>(_factory);
        }
    }

    public class ConsoleLogger : ILogger
    {
        public ConsoleLogger(string categoryName)
        {

        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(formatter(state, exception));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}