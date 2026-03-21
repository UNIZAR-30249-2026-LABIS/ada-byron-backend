using System.Data;

namespace AdaByron.Application.Ports.Out;

/// <summary>
/// Puerto de salida para control de transacciones (Application no conoce EF Core).
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Inicia una transacción con el nivel de aislamiento indicado.</summary>
    Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.RepeatableRead);

    /// <summary>Guarda cambios y hace commit de la transacción activa.</summary>
    Task CommitAsync();

    /// <summary>Revierte la transacción activa.</summary>
    Task RollbackAsync();

    /// <summary>Adquiere un bloqueo consultivo de PostgreSQL sobre el código de espacio.</summary>
    Task AcquireEspacioLockAsync(string espacioId);
}
