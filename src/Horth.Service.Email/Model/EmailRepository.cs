using Horth.Service.Email.Shared.Model;
using Irc.Infrastructure.Model;
using Microsoft.EntityFrameworkCore;

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


    public interface IEmailRepository : IMessageQueueRepository<EmailStat> { }
    public class EmailRepository : MessageQueueRepository<EmailStat>, IEmailRepository
    {
        public EmailRepository(DbContext context) : base(context)
        {
        }
    }

}
