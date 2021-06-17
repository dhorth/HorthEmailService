using Horth.Service.Email.Shared.Model;
using Irc.Infrastructure.Model;
using Microsoft.EntityFrameworkCore;
using System;
using Serilog;

namespace Horth.Service.Email.Model
{

    public interface IEmailUnitOfWork : IUnitOfWork
    {
        IEmailRepository Email { get; }
    }

    public class EmailUnitOfWork : UnitOfWork, IEmailUnitOfWork
    {
        public EmailUnitOfWork(EmailServiceDbContext context) : base(context)
        {
            Email = new EmailRepository(_context);
        }

        public IEmailRepository Email { get; set; }

    }


    public interface IEmailRepository : IMessageQueueRepository<EmailStat>
    {
        void Log(string key, string to, string from, string subject, int result);
        void Log(OneOfficeMailMessage msg, int success);
    }
    public class EmailRepository : MessageQueueRepository<EmailStat>, IEmailRepository
    {
        public EmailRepository(DbContext context) : base(context)
        {
        }
        public void Log(string key, string to, string from, string subject, int result)
        {
            try
            {
                Add(new EmailStat { Key = key, To = to, From = from, Subject = subject, Result = result, LastUpdate = DateTime.Now });
                Context.SaveChanges();

            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, "Email Stats Log");
            }        
        }

        public void Log(OneOfficeMailMessage msg, int success)
        {
            Log(msg.Id, msg.Addresses, msg.From, msg.Subject, success);
        }
    }

}
