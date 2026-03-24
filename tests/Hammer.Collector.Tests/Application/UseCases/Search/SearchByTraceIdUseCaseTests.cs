using FluentAssertions;
using Hammer.Collector.Application.Exceptions;
using Hammer.Collector.Application.UseCases.Search.SearchByTraceId;
using Hammer.Collector.Domain.Ports;
using Hammer.Collector.Domain.Search;
using NSubstitute;

namespace Hammer.Collector.Tests.Application.UseCases.Search;

public sealed class SearchByTraceIdUseCaseTests
{
    private readonly ISearchRepository _searchRepository = Substitute.For<ISearchRepository>();
    private readonly SearchByTraceIdUseCase _sut;

    public SearchByTraceIdUseCaseTests()
    {
        _sut = new SearchByTraceIdUseCase(_searchRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResults_WhenValidTraceIdAsync()
    {
        // Arrange
        var expected = new TraceSearchResult([], []);
        _searchRepository
            .SearchByTraceIdAsync("trace-123", Arg.Any<CancellationToken>())
            .Returns(expected);

        var request = new SearchByTraceIdRequest("trace-123");

        // Act
        TraceSearchResult result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenRequestIsNullAsync()
    {
        Func<Task> act = () => _sut.ExecuteAsync(null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenTraceIdIsEmptyAsync()
    {
        var request = new SearchByTraceIdRequest(string.Empty);
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowBadRequest_WhenTraceIdIsWhitespaceAsync()
    {
        var request = new SearchByTraceIdRequest("   ");
        Func<Task> act = () => _sut.ExecuteAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
