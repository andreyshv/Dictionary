using System;
using System.Threading;
using System.Threading.Tasks;

//TODO: check/test code
namespace Models
{
    public sealed class SafeWrapper<T> : IDisposable where T : class
    {
        private readonly T _instance;
        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);

        public SafeWrapper() 
        {
            _instance = Activator.CreateInstance<T>();
        }

        public SafeWrapper(Func<T> constuctor) 
        {
            _instance = constuctor();
        }
        
        public void DoAction(Action<T> action)
        {
            _mutex.Wait();

            try
            {
                action(_instance);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public void DoAction(Action<T, object[]> action, params object[] args)
        {
            _mutex.Wait();

            try
            {
                action(_instance, args);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public async Task DoActionAsync(Action<T> action)
        {
            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                action(_instance);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public async Task DoActionAsync(Action<T, object[]> action, params object[] args)
        {
            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                action(_instance, args);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public U DoFunc<U>(Func<T, U> func)
        {
            _mutex.Wait();

            try
            {
                return func(_instance);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public U DoFunc<U>(Func<T, object[], U> func, params object[] args)
        {
            _mutex.Wait();

            try
            {
                return func(_instance, args);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public async Task<U> DoFuncAsync<U>(Func<T, U> func)
        {
            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                return func(_instance);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public async Task<U> DoFuncAsync<U>(Func<T, object[], U> func, params object[] args)
        {
            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                return func(_instance, args);
            }
            finally
            {
                _mutex.Release();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public void Dispose()
        {
            lock (this)
            {
                if (!disposedValue)
                {
                    // dispose managed state (managed objects).
                    _mutex.Dispose();
                    (_instance as IDisposable)?.Dispose();

                    disposedValue = true;
                }
            }
        }
        #endregion
    }
}