using System;
using System.Collections.Generic;
using System.Net.Cache;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Exceptions;
using Irc.Infrastructure.Services.Queue;
using Serilog;

namespace Horth.Service.Email.Shared.MsgQueue
{
    public class MessageQueueNotificationProviderBase<T> : IObservable<T> where T : IrcMessageQueueMessage
    {

        public string ProviderName { get; private set; }
        // Maintain a list of observers
        private List<IObserver<T>> _observers;

        public MessageQueueNotificationProviderBase(string providerName)
        {
            ProviderName = providerName;
            Log.Information($"Created notification provider {providerName}");
            _observers = new List<IObserver<T>>();
        }

        // Define Unsubscriber class
        private class Unsubscriber : IDisposable
        {

            private readonly List<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                Dispose(true);
            }
            private bool _disposed = false;
            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }
                if (disposing)
                {
                    if (_observer != null && _observers.Contains(_observer))
                    {
                        _observers.Remove(_observer);
                    }
                }
                _disposed = true;
            }
        }

        // Define Subscribe method
        public async Task<IDisposable> SubscribeAsync(IObserver<T> observer)
        {
            IDisposable ret = null;
            await Task.Run(() =>
            {
                ret = Subscribe(observer);
            });
            return ret;

        }


        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                Log.Information($"Subscription added to {ProviderName}, count = {_observers.Count}");
            }
            return new Unsubscriber(_observers, observer);
        }

        public async Task UnSubscribeAsync(IObserver<T> observer)
        {
            await Task.Run(() =>
            {
                UnSubscribe(observer);
            });
        }

        public void UnSubscribe(IObserver<T> observer)
        {
            if (_observers.Contains(observer))
            {
                var idx = _observers.IndexOf(observer);
                var obj = _observers[idx];
                obj.OnCompleted();
                _observers.Remove(observer);
                Log.Information($"Subscription removed from {ProviderName}, count = {_observers.Count}");
            }
        }



        internal bool HasSubscribers()
        {
            return _observers.Count > 0;

        }

        internal bool EventNotification(IrcMessageQueueMessage msg, bool failOnDeliveryException = false)
        {
            var rc = false;
            foreach (var observer in _observers)
            {
                try
                {
                    observer.OnNext((T)msg);
                    Log.Debug($"EventNotification...");
                    rc = true;
                }
                catch (Exception ex)
                {
                    Log.Error($"EventNotification...", ex);
                    if (failOnDeliveryException)
                        throw new IrcMessageQueueDeliveryException($"Failed to process message {msg.Id}", ex);
                }
            }

            return rc;
        }

        internal void End()
        {
            foreach (var observer in _observers)
            {
                try
                {
                    observer.OnCompleted();
                    Log.Debug($"End...");
                }
                catch (Exception ex)
                {
                    Log.Error($"End...", ex);
                }
            }
            _observers.Clear();
        }


    }
}
