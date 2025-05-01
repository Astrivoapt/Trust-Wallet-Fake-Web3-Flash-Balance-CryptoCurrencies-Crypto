using Dfe.Complete.Application.LocalAuthorities.Queries.GetLocalAuthority;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.Interfaces.Repositories;
using Moq;
using FluentAssertions;
using System.Linq.Expressions;

namespace Dfe.Complete.Application.Tests.LocalAuthorities.QueriesTests;

public class LocalAuthoritiesQueriesTests
{
    private readonly Mock<ICompleteRepository<LocalAuthority?>> _repositoryMock;
    private readonly GetLocalAuthorityByCodeQueryHandler _handler;

    public LocalAuthoritiesQueriesTests()
    {
        _repositoryMock = new Mock<ICompleteRepository<LocalAuthority>>();
        _handler = new GetLocalAuthorityByCodeQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturn_SuccessResult_WhenLocalAuthorityFound()
    {
        var query = new GetLocalAuthorityByCodeQuery("123");
        var localAuthority = new LocalAuthority { Code = "123", Name = "Test LA" };
        _repositoryMock
            .Setup(repo => repo.GetAsync(x => x.Code == query.Code))
            .ReturnsAsync(localAuthority);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be(localAuthority.Code);
        result.Value.Name.Should().Be(localAuthority.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturn_SuccessWithNull_WhenLocalAuthorityNotFound()
    {
        var query = new GetLocalAuthorityByCodeQuery("999");
        _repositoryMock
            .Setup(repo => repo.GetAsync(x => x.Code == query.Code))
            .ReturnsAsync((LocalAuthority?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturn_Failure_WhenExceptionThrown()
    {
        var query = new GetLocalAuthorityByCodeQuery("123");
        _repositoryMock
            .Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<LocalAuthority?, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Database error");
    }
}