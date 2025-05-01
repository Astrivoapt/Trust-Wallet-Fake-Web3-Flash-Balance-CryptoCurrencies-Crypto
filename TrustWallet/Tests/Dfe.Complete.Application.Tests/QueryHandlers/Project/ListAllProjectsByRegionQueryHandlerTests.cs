using System.Xml.XPath;
using AutoFixture;
using AutoFixture.Xunit2;
using Dfe.Complete.Application.Projects.Interfaces;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Application.Projects.Queries.ListAllProjects;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using MockQueryable;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.Complete.Application.Tests.QueryHandlers.Project;

public class ListAllProjectsByRegionQueryHandlerTests
{
    [Theory]
    [CustomAutoData(
        typeof(OmitCircularReferenceCustomization),
        typeof(ListAllProjectsQueryModelCustomization),
        typeof(DateOnlyCustomization))]
    public async Task Handle_ShouldReturnProjectsByRegion(
        [Frozen] IListAllProjectsQueryService mockListAllProjectsQueryService,
        ListAllProjectsByRegionQueryHandler handler,
        IFixture fixture)
    {
        // Arrange
        var listAllProjectsQueryModels = fixture.CreateMany<ListAllProjectsQueryModel>(50).ToList();

        var query = new ListAllProjectsQuery(null, null);

        var mock = listAllProjectsQueryModels.BuildMock();

        mockListAllProjectsQueryService.ListAllProjects(query.ProjectStatus, query.Type)
            .Returns(mock);

        var listAllProjectsByRegionQuery = new ListAllProjectsByRegionQuery(null, null);

        var result = await handler.Handle(listAllProjectsByRegionQuery, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Distinct(result.Value.Select(x => x.Region));
        Assert.Contains(result.Value, x => x.TransfersCount > 1 || x.ConversionsCount > 1);
    }
    
    [Theory]
    [CustomAutoData(
        typeof(OmitCircularReferenceCustomization),
        typeof(ListAllProjectsQueryModelCustomization),
        typeof(DateOnlyCustomization))]
    public async Task Handle_ThrowsException_Returns_FailureResult(
        [Frozen] IListAllProjectsQueryService mockListAllProjectsQueryService,
        ListAllProjectsByRegionQueryHandler handler,
        IFixture fixture)
    {
        // Arrange
        const string errorMessage = "test error message";

        var query = new ListAllProjectsQuery(null, null);

        mockListAllProjectsQueryService.ListAllProjects(query.ProjectStatus, query.Type)
            .Throws(new Exception(errorMessage));

        var listAllProjectsByRegionQuery = new ListAllProjectsByRegionQuery(null, null);

        var result = await handler.Handle(listAllProjectsByRegionQuery, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.Error);
    }
}