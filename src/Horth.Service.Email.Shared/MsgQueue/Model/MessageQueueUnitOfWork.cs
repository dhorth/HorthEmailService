using Horth.Service.Email.Queue.Model;
using Horth.Service.Email.Shared.Model;
using Irc.Infrastructure.Model;
using Irc.Infrastructure.Services.Queue;
using Microsoft.EntityFrameworkCore;

namespace Horth.Service.Email.Shared.MsgQueue
{

    public interface IMessageQueueMessageUnitOfWork : IUnitOfWork
    {
        IMessageQueueMessageRepository MessageQueueMessage { get; }
    }

    public class MessageQueueMessageUnitOfWork : UnitOfWork, IMessageQueueMessageUnitOfWork
    {
        public MessageQueueMessageUnitOfWork(MessageQueueDbContext context) : base(context)
        {
            MessageQueueMessage = new MessageQueueMessageRepository(_context);
        }

        public IMessageQueueMessageRepository MessageQueueMessage { get; set; }

    }


    public interface IMessageQueueMessageRepository : IMessageQueueRepository<IrcMessageQueueMessage> { }
    public class MessageQueueMessageRepository : MessageQueueRepository<IrcMessageQueueMessage>, IMessageQueueMessageRepository
    {
        public MessageQueueMessageRepository(DbContext context) : base(context)
        {
        }
    }
}
