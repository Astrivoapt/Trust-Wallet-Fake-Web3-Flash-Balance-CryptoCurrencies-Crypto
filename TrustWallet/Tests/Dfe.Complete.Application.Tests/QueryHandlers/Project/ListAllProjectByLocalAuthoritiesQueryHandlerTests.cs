using System.Linq.Expressions;
using AutoFixture;
using AutoFixture.Xunit2;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Application.Projects.Queries.ListAllProjects;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.Interfaces.Repositories;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.Complete.Application.Tests.QueryHandlers.Project;

public class ListAllProjectByLocalAuthoritiesQueryHandlerTests
{
    [Theory]
    [CustomAutoData(
        typeof(OmitCircularReferenceCustomization),
        typeof(ListAllProjectsQueryModelCustomization),
        typeof(DateOnlyCustomization),
        typeof(ListAllProjectLocalAuthoritiesArrangement))]
    public async Task Handle_ShouldReturnListProjectsLocalAuthorities(
        ListAllProjectByLocalAuthorities handler,
        IFixture fixture)
    {
        //Arrange
        var expectedLocalAuthorities = fixture.Create<List<ListAllProjectLocalAuthoritiesResultModel>>().OrderBy(la => la.LocalAuthority.Name).ToList();

        //Act
        var query = new ListAllProjectsByLocalAuthoritiesQuery();

        var handlerResult = await handler.Handle(query, default);

        //Assert 
        Assert.NotNull(handlerResult);
        Assert.Equal(expectedLocalAuthorities.Count, handlerResult.ItemCount);

        for (var i = 0; i < handlerResult.ItemCount; i++)
        {
            Assert.Equivalent(expectedLocalAuthorities[i], handlerResult.Value![i]);
        }
    }

    [Theory]
    [CustomAutoData(
        typeof(OmitCircularReferenceCustomization),
        typeof(ListAllProjectsQueryModelCustomization),
        typeof(DateOnlyCustomization),
        typeof(ListAllProjectLocalAuthoritiesArrangement))]
    public async Task Handle_ShouldReturnCorrectList_WhenAllPagesAreSkipped(
        ListAllProjectByLocalAuthorities handler)
    {
        //Act
        var query = new ListAllProjectsByLocalAuthoritiesQuery { Page = 10 };

        var handlerResult = await handler.Handle(query, default);

        //Assert 

        // Assert
        Assert.NotNull(handlerResult);
        Assert.True(handlerResult.IsSuccess);
        Assert.Equal(0, handlerResult.Value?.Count);
    }


    [Theory]
    [CustomAutoData(
        typeof(OmitCircularReferenceCustomization),
        typeof(ListAllProjectsQueryModelCustomization),
        typeof(DateOnlyCustomization),
        typeof(ListAllProjectLocalAuthoritiesArrangement))]
    public async Task Handle_ShouldReturnUnsuccessful_WhenAnErrorOccurs(
        [Frozen] ICompleteRepository<LocalAuthority> localAuthoritiesRepo,
        ListAllProjectByLocalAuthorities handler,
        IFixture fixture)
    {
        const string errorMessage = "This is a test error message";

        var query = new ListAllProjectsByLocalAuthoritiesQuery { Page = 10 };

        localAuthoritiesRepo.FetchAsync(Arg.Any<Expression<Func<LocalAuthority, bool>>>(), default)
            .ThrowsAsync(new Exception(errorMessage));

        var handlerResult = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(handlerResult);
        Assert.False(handlerResult.IsSuccess);
        Assert.Equal(errorMessage, handlerResult.Error);
    }
}