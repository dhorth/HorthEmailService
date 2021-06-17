using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Horth.Service.Email.Shared.Exceptions
{
    public class IrcMessageQueueDeliveryException:IrcException
    {

        public IrcMessageQueueDeliveryException(string message, Exception innerException=null)
            : base(message, innerException)
        {
            Log.Logger.Error(innerException, message);
        }
    }

    public class IrcMessageQueueException : IrcException
    {
        public IrcMessageQueueException(string message, Exception innerException)
            : base(message, innerException)
        {
            Log.Logger.Error(innerException, message);
        }
    }
}
