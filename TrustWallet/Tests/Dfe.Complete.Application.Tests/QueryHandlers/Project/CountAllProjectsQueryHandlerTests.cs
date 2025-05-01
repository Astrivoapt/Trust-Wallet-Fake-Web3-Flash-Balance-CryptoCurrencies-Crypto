using AutoFixture.Xunit2;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using NSubstitute;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using Dfe.Complete.Application.Projects.Interfaces;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Application.Projects.Queries.CountAllProjects;
using Dfe.Complete.Tests.Common.Customizations.Models;
using MockQueryable;
using NSubstitute.ExceptionExtensions;

namespace Dfe.Complete.Application.Tests.QueryHandlers.Project
{
    public class CountAllProjectsQueryHandlerTests
    {
        
        [Theory]
        [CustomAutoData(
            typeof(OmitCircularReferenceCustomization),
            typeof(ListAllProjectsQueryModelCustomization),
            typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldReturnCorrectCount(
            [Frozen] IListAllProjectsQueryService mockEstablishmentQueryService,
            CountAllProjectsQueryHandler handler,
            CountAllProjectsQuery query,
            List<ListAllProjectsQueryModel> listAllProjectsQueryModels)
        {
            // Arrange
            var expected = listAllProjectsQueryModels.Count;

            var mock = listAllProjectsQueryModels.BuildMock();

            mockEstablishmentQueryService.ListAllProjects(query.ProjectStatus, query.Type)
                .Returns(mock);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.Equal(expected, result.Value);
        }
        
        [Theory]
        [CustomAutoData(
            typeof(OmitCircularReferenceCustomization),
            typeof(ListAllProjectsQueryModelCustomization),
            typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldReturnUnsuccessful_WhenAnErrorOccurs(
            [Frozen] IListAllProjectsQueryService mockEstablishmentQueryService,
            CountAllProjectsQueryHandler handler)
        {
            // Arrange
            var errorMessage = "This is a test";
            
            var query = new CountAllProjectsQuery(null, null);

            mockEstablishmentQueryService.ListAllProjects(query.ProjectStatus, query.Type)
                .Throws(new Exception(errorMessage));

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMessage, result.Error);
        }
    }
}
