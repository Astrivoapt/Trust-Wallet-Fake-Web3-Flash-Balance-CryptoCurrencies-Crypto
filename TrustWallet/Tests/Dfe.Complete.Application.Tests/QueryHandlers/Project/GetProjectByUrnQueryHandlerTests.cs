using AutoFixture.Xunit2;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using Dfe.Complete.Domain.Interfaces.Repositories;
using NSubstitute;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using Dfe.Complete.Application.Projects.Queries.GetProject;
using System.Linq.Expressions;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Domain.ValueObjects;
using AutoMapper;
using Dfe.Complete.Application.Projects.Models;
using NSubstitute.ExceptionExtensions;

namespace Dfe.Complete.Application.Tests.QueryHandlers.Project
{
    public class GetProjectByUrnQueryHandlerTests
    {
        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldGetAProjectByUrn_WhenCommandIsValid(
            [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
            [Frozen] IMapper mockMapper,
            GetProjectByUrnQueryHandler handler,
            GetProjectByUrnQuery command
            )
        {
            var now = DateTime.UtcNow;

            var project = Domain.Entities.Project.CreateConversionProject(
                new ProjectId(Guid.NewGuid()),
                command.Urn,
                now,
                now,
                Domain.Enums.TaskType.Conversion,
                Domain.Enums.ProjectType.Conversion,
                Guid.NewGuid(),
                DateOnly.MinValue,
                true,
                new Domain.ValueObjects.Ukprn(2),
                Region.London,
                true,
                true,
                DateOnly.MinValue,
                "",
                "",
                "",
                null,
                default,
                null,
                null, 
                null, 
                null, 
                Guid.NewGuid());

            // Arrange
            mockProjectRepository.GetAsync(Arg.Any<Expression<Func<Domain.Entities.Project, bool>>>())
                .Returns(project);

            mockMapper.Map<ProjectDto>(project).Returns(new ProjectDto() { Urn = command.Urn });

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            await mockProjectRepository.Received(1).GetAsync(Arg.Any<Expression<Func<Domain.Entities.Project, bool>>>());
            Assert.True(result.IsSuccess == true);
            Assert.True(result.Value?.Urn == command.Urn);
        }


        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldSucceedAndReturnNullWhenUnfoundProjectByUrn_WhenCommandIsValid(
            [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
            GetProjectByUrnQueryHandler handler,
            GetProjectByUrnQuery command
            )
        {
            // Arrange
            mockProjectRepository.GetAsync(Arg.Any<Expression<Func<Domain.Entities.Project?, bool>>>())
                .Returns((Domain.Entities.Project?)null);

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            await mockProjectRepository.Received(1).GetAsync(Arg.Any<Expression<Func<Domain.Entities.Project, bool>>>());
            Assert.True(result.IsSuccess == true);
            Assert.True(result.Value == null);
        }

        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldFailAndReturnErrorMessage_WhenExceptionIsThrown(
        [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
        GetProjectByUrnQueryHandler handler,
        GetProjectByUrnQuery command
        )
            {
                // Arrange
                var expectedErrorMessage = "Expected Error Message";

                mockProjectRepository.GetAsync(Arg.Any<Expression<Func<Domain.Entities.Project?, bool>>>())
                    .Throws(new Exception(expectedErrorMessage));

                // Act
                var result = await handler.Handle(command, default);

                // Assert
                await mockProjectRepository.Received(1).GetAsync(Arg.Any<Expression<Func<Domain.Entities.Project, bool>>>());
                Assert.True(result.IsSuccess == false);
                Assert.Equal(result.Error, expectedErrorMessage);
        }
        }
}
