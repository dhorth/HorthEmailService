using Microsoft.EntityFrameworkCore;

namespace Horth.Service.Email.Shared.Model
{
    public partial class OneOfficeDbContext : DbContext
    {
        public OneOfficeDbContext()
        {
        }

        public OneOfficeDbContext(DbContextOptions options)
            : base(options)
        {
            //Database.EnsureCreated();
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("DataSource=oneoffice.sqlitedb");
            }

            base.OnConfiguring(optionsBuilder);
        }
    }
}
