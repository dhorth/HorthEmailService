using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Exceptions;
using Irc.Infrastructure.Services.Queue;
using Serilog;

namespace Horth.Service.Email.Shared.MsgQueue
{
    public abstract class MessageNotificationSubscriber<T> : IObserver<T> where T : IrcMessageQueueMessage
    {
        private IDisposable _unsubscriber;
        private bool _failOnError;
        public string SubscriberName { get; private set; }

        public MessageNotificationSubscriber(string sub, bool failOnError)
        {
            SubscriberName = sub;
            _failOnError = failOnError;
        }

        public virtual void OnCompleted()
        {
            Log.Information($"OnCompleted...");
            Unsubscribe();
        }

        public virtual void OnError(Exception ex)
        {
            Log.Error($"OnError...", ex);
        }

        public void OnNext(T value)
        {
            var rc=HandleMessage(value);
            if(!rc && _failOnError)
                throw new IrcMessageQueueDeliveryException($"Failed to process event {value.Id}");

        }

        public abstract bool HandleMessage(T value);

        public virtual void Subscribe(IObservable<T> provider)
        {
            // Subscribe to the Observable
            if (provider != null)
            {
                _unsubscriber = provider.Subscribe(this);
                Log.Information($"Subscribe...");
            }
            else
            {
                throw new Exception();
            }
        }

        public virtual void Unsubscribe()
        {
            Log.Information($"Unsubscribe...");
            _unsubscriber.Dispose();
        }
    }

}
