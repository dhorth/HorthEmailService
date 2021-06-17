using Horth.Service.Email.Shared.Configuration;
using MimeKit;

namespace Horth.Service.Email.Service
{
    public interface ISendMessageService
    {
        void SendMail(MimeMessage msg);
    }
    
    
    public abstract class OneOfficeSendMailBase : ISendMessageService
    {
        protected AppSettings AppSettings;

        protected OneOfficeSendMailBase(AppSettings appSettings)
        {
            AppSettings = appSettings;
        }

        public abstract void SendMail(MimeMessage msg);
    }
}
