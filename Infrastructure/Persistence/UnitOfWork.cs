using System.Data;
using AdaByron.Application.Ports.Out;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AdaByron.Infrastructure.Persistence;

public sealed class UnitOfWork(AplicacionDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.RepeatableRead)
    {
        _transaction = await context.Database.BeginTransactionAsync(level);
    }

    public async Task CommitAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction.");
            
        await context.SaveChangesAsync();
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync();
    }

    public async Task AcquireEspacioLockAsync(string espacioId)
    {
        var lockKey = Math.Abs(string.GetHashCode(espacioId, StringComparison.OrdinalIgnoreCase));
        await context.Database.ExecuteSqlRawAsync($"SELECT pg_advisory_xact_lock({lockKey})");
    }

    public void Dispose() => _transaction?.Dispose();
}
