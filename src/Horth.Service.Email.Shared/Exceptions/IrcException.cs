using System;
using System.Net;
using System.Net.Http;
using Horth.Service.Email.Shared.Service;
using Serilog;

namespace Horth.Service.Email.Shared.Exceptions
{
    /// <summary>
    /// Exception type for app exceptions
    /// </summary>
    public class IrcException : Exception
    {
        public IrcException()
        {
        }

        public IrcException(string msg, params object[] args)
            : base(string.Format(msg, args))
        {
        }

        public IrcException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class IrcDataException : IrcException
    {
        public IrcDataException(string msg, params object[] args)
            : base(string.Format(msg, args))
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class IrcWebException : IrcException
    {
        public IrcWebException()
        {
        }

        public IrcWebException(HttpResponseMessage response, ServiceName serviceName, string url)
            : base()
        {
            if (response != null)
            {
                StatusCode = response.StatusCode;
                ReasonPhrase = response.ReasonPhrase;
                Method = response.RequestMessage.Method;
                Version = response.Version;
            }

            Url = url;
            ServiceName = serviceName;
        }

        public ServiceName ServiceName { get; private set; }
        public string Url { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }
        public HttpMethod Method { get; private set; }
        public Version Version { get; private set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class IrcMissingException : IrcException
    {
        public IrcMissingException(string msg, params object[] args)
            : base(string.Format(msg, args))
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string StackTrace => Environment.StackTrace;
    }

    /// <summary>
    /// 
    /// </summary>
    public class IrcEmailException : IrcException
    {
        public IrcEmailException()
            : base()
        {
        }

        public IrcEmailException(string msg, Exception innerException=null)
            : base(msg,innerException)
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override string StackTrace => Environment.StackTrace;
    }


}
