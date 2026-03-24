using System.Diagnostics.CodeAnalysis;

namespace Hammer.Collector.Application.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class ConflictException : Exception
{
    public ConflictException()
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
