using System;
using System.Threading.Tasks;

namespace CafePOS.Application.Interfaces;

public interface ITransactionManager
{
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
