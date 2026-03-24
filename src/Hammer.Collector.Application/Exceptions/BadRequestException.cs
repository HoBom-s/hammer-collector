using System.Diagnostics.CodeAnalysis;

namespace Hammer.Collector.Application.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class BadRequestException : Exception
{
    public BadRequestException()
    {
    }

    public BadRequestException(string message)
        : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
