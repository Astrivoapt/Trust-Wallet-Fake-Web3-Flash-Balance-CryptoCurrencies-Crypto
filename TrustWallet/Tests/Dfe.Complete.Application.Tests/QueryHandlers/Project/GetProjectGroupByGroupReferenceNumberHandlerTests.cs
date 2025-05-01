using AutoFixture.Xunit2;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using Dfe.Complete.Domain.Interfaces.Repositories;
using NSubstitute;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using Dfe.Complete.Application.Projects.Queries.GetProject;
using System.Linq.Expressions;
using AutoMapper;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Domain.Entities;
using NSubstitute.ExceptionExtensions;

namespace Dfe.Complete.Application.Tests.QueryHandlers.Project
{
    public class GetProjectGroupByGroupReferenceNumberHandlerTests
    {
        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldGetAProjectGroupByGroupReference_WhenCommandIsValid(
            [Frozen] ICompleteRepository<Domain.Entities.ProjectGroup> mockProjectGroupRepository,
            [Frozen] IMapper mockMapper,
            GetProjectGroupByGroupReferenceNumberQueryHandler handler,
            GetProjectGroupByGroupReferenceNumberQuery command
            )
        {
            var projectGroup = new ProjectGroup();

            // Arrange
            mockProjectGroupRepository.GetAsync(Arg.Any<Expression<Func<Domain.Entities.ProjectGroup, bool>>>())
                .Returns(projectGroup);

            mockMapper.Map<ProjectGroupDto>(projectGroup).Returns(new ProjectGroupDto());
            
            // Act
            var result = await handler.Handle(command, default);

            // Assert
            await mockProjectGroupRepository.Received(1).GetAsync(Arg.Any<Expression<Func<Domain.Entities.ProjectGroup, bool>>>());
            Assert.True(result.IsSuccess == true);
        }


        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldSucceedAndReturnNullWhenUnfoundProjectByUrn_WhenCommandIsValid(
            [Frozen] ICompleteRepository<Domain.Entities.ProjectGroup> mockProjectGroupRepository,
            GetProjectGroupByGroupReferenceNumberQueryHandler handler,
            GetProjectGroupByGroupReferenceNumberQuery command
            )
        {
            // Arrange
            mockProjectGroupRepository.GetAsync(Arg.Any<Expression<Func<Domain.Entities.ProjectGroup?, bool>>>())
                .Returns((Domain.Entities.ProjectGroup?)null);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            await mockProjectGroupRepository.Received(1).GetAsync(Arg.Any<Expression<Func<Domain.Entities.ProjectGroup, bool>>>());
            Assert.True(result.IsSuccess == true);
            Assert.True(result.Value == null);
        }

        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldFailAndReturnError_WhenRepoCallFails(
        [Frozen] ICompleteRepository<Domain.Entities.ProjectGroup> mockProjectGroupRepository,
        GetProjectGroupByGroupReferenceNumberQueryHandler handler,
        GetProjectGroupByGroupReferenceNumberQuery command
        )
        {
            // Arrange
            mockProjectGroupRepository.GetAsync(Arg.Any<Expression<Func<Domain.Entities.ProjectGroup?, bool>>>())
                .Throws(new Exception());

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            await mockProjectGroupRepository.Received(1).GetAsync(Arg.Any<Expression<Func<Domain.Entities.ProjectGroup, bool>>>());
            Assert.True(result.IsSuccess == false);
        }
    }
}
