using Horth.Service.Email.Queue.Model;
using Horth.Service.Email.Shared.MsgQueue;
using Irc.Infrastructure.Model;
using Irc.Infrastructure.Services.Queue;
using Microsoft.EntityFrameworkCore;

namespace Horth.Service.Email.Shared.Model
{
    public interface IMessageQueueRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
    }

    public class MessageQueueRepository<TEntity> : Repository<TEntity>, IMessageQueueRepository<TEntity> where TEntity : class
    {
        public MessageQueueRepository(DbContext context) : base(context)
        {
        }

        public MessageQueueDbContext OneOfficeContext => (MessageQueueDbContext)Context;


    }
}
