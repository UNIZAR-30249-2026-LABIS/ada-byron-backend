using System.Data;
using AdaByron.Application.Ports.Out;
using AdaByron.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AdaByron.Infrastructure.Persistence;

/// <summary>
/// Implementación de IUnitOfWork usando la transacción del DbContext de EF Core.
/// Permite usar serializable isolation para prevenir solapamientos concurrentes.
/// </summary>
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
            throw new InvalidOperationException("No hay ninguna transacción activa.");
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
        // Bloqueo consultivo a nivel de transacción en PostgreSQL (advisory lock)
        // Garantiza acceso exclusivo al espacio durante la transacción activa.
        var lockKey = Math.Abs(string.GetHashCode(espacioId, StringComparison.OrdinalIgnoreCase));
        await context.Database.ExecuteSqlRawAsync(
            $"SELECT pg_advisory_xact_lock({lockKey})");
    }

    public void Dispose() => _transaction?.Dispose();
}
