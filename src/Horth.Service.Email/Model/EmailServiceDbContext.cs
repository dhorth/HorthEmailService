using Horth.Service.Email.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace Horth.Service.Email.Model
{
    public partial class EmailServiceDbContext : OneOfficeDbContext
    {
        public EmailServiceDbContext()
        {
        }

        public EmailServiceDbContext(DbContextOptions<EmailServiceDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EmailStat> EmailStat { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var statEntityTypeBuilder = modelBuilder.Entity<EmailStat>();
            statEntityTypeBuilder.HasKey(e => e.Id);

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
