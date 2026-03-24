using System.Diagnostics.CodeAnalysis;

namespace Hammer.Collector.Application.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException()
    {
    }

    public ServiceUnavailableException(string message)
        : base(message)
    {
    }

    public ServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
