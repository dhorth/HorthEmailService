using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irc.Infrastructure.Services.Queue;
using Serilog;

namespace Horth.Service.Email.Shared.MsgQueue
{
    public abstract class MessageNotificationSubscriber<T> : IObserver<T> where T : IrcMessageQueueMessage
    {
        private IDisposable _unsubscriber;
        public string SubscriberName { get; private set; }

        public MessageNotificationSubscriber(string sub)
        {
            SubscriberName = sub;
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
            HandleMessage(value);
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
