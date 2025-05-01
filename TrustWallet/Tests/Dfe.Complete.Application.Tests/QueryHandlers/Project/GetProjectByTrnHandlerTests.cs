using System.Linq.Expressions;
using AutoFixture.Xunit2;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;        
using DfE.CoreLibs.Testing.AutoFixture.Customizations;   
using Dfe.Complete.Application.Projects.Queries.GetProject;
using Dfe.Complete.Domain.Interfaces.Repositories;
using Dfe.Complete.Tests.Common.Customizations.Behaviours;
using Dfe.Complete.Tests.Common.Customizations.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.Complete.Application.Tests.QueryHandlers.Project
{
    public class GetProjectByTrnQueryHandlerTests
    {
        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization), typeof(ProjectCustomization), typeof(IgnoreVirtualMembersCustomisation))]
        public async Task Handle_ShouldReturnSuccess_WhenProjectIsFound(
            [Frozen] ICompleteRepository<Domain.Entities.Project> mockRepository,
            GetProjectByTrn handler,
            Domain.Entities.Project project)
        {
            // Arrange
            mockRepository
                .FindAsync(Arg.Any<Expression<Func<Domain.Entities.Project, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(project);

            var query = new GetProjectByTrnQuery(project.NewTrustReferenceNumber);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(project.Id.Value, result.Value.ProjectId);
            Assert.Equal(project.NewTrustName, result.Value.NewTrustName);
        }

        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization), typeof(ProjectCustomization), typeof(IgnoreVirtualMembersCustomisation))]
        public async Task Handle_ShouldReturnFailure_WhenRepositoryThrowsException(
            [Frozen] ICompleteRepository<Domain.Entities.Project> mockRepository,
            GetProjectByTrn handler,
            Domain.Entities.Project project)
        {
            // Arrange
            var expectedMessage = "Repository error";
            mockRepository
                .FindAsync(Arg.Any<Expression<Func<Domain.Entities.Project, bool>>>(), Arg.Any<CancellationToken>())
                .Throws(new Exception(expectedMessage));

            var query = new GetProjectByTrnQuery(project.NewTrustReferenceNumber);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Error);
        }
    }
}
