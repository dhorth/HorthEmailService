using Horth.Service.Email.Shared.Model;
using Microsoft.EntityFrameworkCore;
using OneOffice.Scheduler.Job.Shared;

namespace Horth.Service.Email.Scheduler.Model
{
    public partial class SchedulerServiceDbContext : OneOfficeDbContext
    {
        public SchedulerServiceDbContext()
        {
        }

        public SchedulerServiceDbContext(DbContextOptions<SchedulerServiceDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<SchedulerResult> SchedulerResult { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityTypeBuilder = modelBuilder.Entity<SchedulerResult>();
            entityTypeBuilder.HasKey(e => e.Id);

            //modelBuilder.Entity<OneOfficeMailAttachment>()
            //    .HasOne<OneOfficeMailMessage>(g => g.Message)
            //    .WithMany(a=>a.Attachments)
            //    .HasForeignKey(s => s.MessageKey)
            //    .OnDelete(DeleteBehavior.Cascade);

            //var messageEntityTypeBuilder = modelBuilder.Entity<OneOfficeMailMessage>();
            //messageEntityTypeBuilder.HasKey(e => e.Id);

            //var attachmentEntityTypeBuilder = modelBuilder.Entity<OneOfficeMailAttachment>();
            //attachmentEntityTypeBuilder.HasKey(e => e.Id);

            base.OnModelCreating(modelBuilder);
        }

    }
}