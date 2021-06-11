using Horth.Service.Email.Shared.Model;
using Irc.Infrastructure.Services.Queue;
using Microsoft.EntityFrameworkCore;

namespace Horth.Service.Email.Queue.Model
{
    public partial class MessageQueueDbContext : OneOfficeDbContext
    {
        public MessageQueueDbContext()
        {
        }

        public MessageQueueDbContext(DbContextOptions<MessageQueueDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<IrcMessageQueueMessage> MessageQueueMessage { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var appSettingEntityTypeBuilder = modelBuilder.Entity<IrcMessageQueueMessage>();
            appSettingEntityTypeBuilder
                .HasKey(e => e.Id);

            base.OnModelCreating(modelBuilder);
        }

    }
}
