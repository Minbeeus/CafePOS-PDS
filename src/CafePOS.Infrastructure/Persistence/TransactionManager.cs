using System;
using System.Threading.Tasks;
using CafePOS.Application.Interfaces;
using CafePOS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace CafePOS.Infrastructure.Persistence;

public class TransactionManager : ITransactionManager, IDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public TransactionManager(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _transaction.CommitAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            return; // No transaction to rollback
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
    }
}
