using AdaByron.Domain.Exceptions;

namespace AdaByron.Domain.Exceptions;

public class ConcurrencyException : ExcepcionDominio
{
    public ConcurrencyException(string message) : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
