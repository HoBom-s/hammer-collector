using System.Diagnostics.CodeAnalysis;

namespace Hammer.Collector.Application.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
