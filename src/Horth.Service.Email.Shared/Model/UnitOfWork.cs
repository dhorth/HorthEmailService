using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Linq;
using System.Text;

namespace Irc.Infrastructure.Model
{
    public class UnitOfWork : IUnitOfWork
    {
        protected readonly DbContext _context;

        public UnitOfWork(DbContext context)
        {
            _context = context;
        }

        public int Save()
        {
            int ret = -1;
            try
            {
                ret = _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                var builder = new StringBuilder("A DbUpdateException was caught while saving changes. ");
                var dbu = ex as DbUpdateException;
                try
                {
                    var entries= _context.ChangeTracker.Entries();
                    foreach (var entry in entries)
                    {
                        if(entry.CurrentValues==null || entry.CurrentValues.Properties==null)
                            continue;

                        foreach(var property in entry.CurrentValues.Properties)
                        {
                            var name=property.Name;
                            var length=property.GetMaxLength();
                            var current=entry.Properties.FirstOrDefault(a=>a.Metadata.Name==name);
                            if (current != null && current.CurrentValue !=null)
                            {
                                if(current.CurrentValue.ToString().Length>length)
                                {
                                    builder.Append($"{name} exceeds maximum field length of {length} while trying to save {current.CurrentValue.ToString()}.");
                                }
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    builder.Append("Error parsing DbUpdateException: " + e.ToString());
                }

                string message = builder.ToString();
                throw new Exception(message, dbu);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "DB Save Gerenic Error");
                throw;
            }
            return ret;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
