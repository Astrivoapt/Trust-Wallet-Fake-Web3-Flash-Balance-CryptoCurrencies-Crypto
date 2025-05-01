using System.Collections.ObjectModel;
using AutoFixture;
using AutoFixture.Xunit2;
using Dfe.AcademiesApi.Client.Contracts;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using NSubstitute;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using Dfe.Complete.Application.Projects.Interfaces;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Application.Projects.Queries.ListAllProjects;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Domain.ValueObjects;
using Dfe.Complete.Tests.Common.Customizations.Models;
using MockQueryable;
using NSubstitute.ExceptionExtensions;

namespace Dfe.Complete.Application.Tests.QueryHandlers.Project
{
    public class ListAllProjectsInTrustTests
    {
        [Theory]
        [CustomAutoData(
            typeof(OmitCircularReferenceCustomization),
            typeof(ListAllProjectsQueryModelCustomization),
            typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldReturnCorrectResult_WhenProjectsFound(
            [Frozen] IListAllProjectsQueryService listAllProjectsQueryService,
            [Frozen] ITrustsV4Client trustsClient,
            ListAllProjectsInTrustQueryHandler handler,
            IFixture fixture)
        {
            // Arrange
            var expectedTrusts = 20;
            var expectedProjects = 20;
            var ukprn = 2;

            var trustDto = fixture
                .Build<TrustDto>().With(t => t.Ukprn, ukprn.ToString())
                .Create();
            
            var listAllProjectsQueryModels = fixture
                .Build<ListAllProjectsQueryModel>()
                .CreateMany(expectedProjects)
                .ToList();
            
            var random = new Random();
            
            foreach (var model in listAllProjectsQueryModels)
            {
                var project = model.Project;
                bool isMatProject = random.NextDouble() < 0.5; // 50% chance
            
                project.IncomingTrustUkprn = isMatProject ? null : new Ukprn(ukprn);
                project.Type = isMatProject ? ProjectType.Conversion : ProjectType.Transfer;
            
                if (isMatProject)
                {
                    project.NewTrustReferenceNumber = "TR0001";
                    project.NewTrustName = "Test Trust";
                }
            }

            var query = new ListAllProjectsInTrustQuery("", false) { Count = expectedTrusts };

            var mockProjects = listAllProjectsQueryModels.BuildMock();
            
            listAllProjectsQueryService.ListAllProjects(ProjectState.Active, null)
                .Returns(mockProjects);

            trustsClient.GetTrustByUkprn2Async(Arg.Any<string>())
                .Returns(Task.FromResult(trustDto));

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Value.trustName, trustDto.Name);
        }
        
        

        
        [Theory]
        [CustomAutoData(
            typeof(OmitCircularReferenceCustomization),
            typeof(ListAllProjectsQueryModelCustomization),
            typeof(DateOnlyCustomization))]
        public async Task Handle_ExceptionIsCaught_WhenTrustClientFails(
            [Frozen] ITrustsV4Client trustsClient,
            ListAllProjectsInTrustQueryHandler handler)
        {
            // Arrange
            var expectedtrusts = 20;
            var errorMessage = "This is an error";
            
            var query = new ListAllProjectsInTrustQuery("", false) { Count = expectedtrusts };
            
            trustsClient.GetTrustByUkprn2Async(Arg.Any<string>())
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