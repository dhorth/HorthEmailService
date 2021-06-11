using System;

namespace Irc.Infrastructure.Model
{
    public interface IUnitOfWork: IDisposable
    {
        int Save();
    }
}
