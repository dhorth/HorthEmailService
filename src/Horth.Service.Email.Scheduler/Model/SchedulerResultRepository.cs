using Horth.Service.Email.Shared.Model;
using Irc.Infrastructure.Model;
using Microsoft.EntityFrameworkCore;
using OneOffice.Scheduler.Job.Shared;

namespace Horth.Service.Email.Scheduler.Model
{

    public interface ISchedulerResultUnitOfWork : IUnitOfWork
    {
        ISchedulerResultRepository SchedulerResult { get; }
    }

    public class SchedulerResultUnitOfWork : UnitOfWork, ISchedulerResultUnitOfWork
    {
        public SchedulerResultUnitOfWork(SchedulerServiceDbContext context) : base(context)
        {
            SchedulerResult = new SchedulerResultRepository(_context);
        }

        public ISchedulerResultRepository SchedulerResult { get; set; }

    }


    public interface ISchedulerResultRepository : IMessageQueueRepository<SchedulerResult> { }
    public class SchedulerResultRepository : MessageQueueRepository<SchedulerResult>, ISchedulerResultRepository
    {
        public SchedulerResultRepository(DbContext context) : base(context)
        {
        }
    }


}