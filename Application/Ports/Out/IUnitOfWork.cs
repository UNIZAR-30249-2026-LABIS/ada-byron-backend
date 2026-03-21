using System.Data;

namespace AdaByron.Application.Ports.Out;

/// <summary>
/// Puerto de salida para control de transacciones ACID.
/// Application lo usa; Infrastructure lo implementa con EF Core.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.RepeatableRead);
    Task CommitAsync();
    Task RollbackAsync();
    /// <summary>Bloqueo consultivo de PostgreSQL sobre el código de espacio (anti-double-booking).</summary>
    Task AcquireEspacioLockAsync(string espacioId);
}
