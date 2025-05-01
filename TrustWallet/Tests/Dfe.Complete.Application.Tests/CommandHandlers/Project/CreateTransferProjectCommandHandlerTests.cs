using System.Linq.Expressions;
using AutoFixture.Xunit2;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using Dfe.Complete.Domain.Interfaces.Repositories;
using NSubstitute;
using Dfe.Complete.Application.Projects.Commands.CreateProject;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Domain.ValueObjects;
using Dfe.Complete.Utils;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using MediatR;
using Moq;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Application.Common.Models;
using Dfe.Complete.Application.Projects.Queries.GetLocalAuthority;
using Dfe.Complete.Application.Projects.Queries.GetUser;
using Dfe.Complete.Application.Projects.Queries.GetProject;
using Dfe.Complete.Tests.Common.Customizations.Behaviours;
using Dfe.Complete.Tests.Common.Customizations.Models;
using Guid = System.Guid;

namespace Dfe.Complete.Application.Tests.CommandHandlers.Project;

public class CreateTransferProjectCommandHandlerTests
{
    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization))]
    public async Task Handle_ShouldCreateAndReturnProjectId_WhenCommandIsValid(
        [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
        [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
        [Frozen] Mock<ISender> mockSender,
        CreateTransferProjectCommand command)
    {
        // Arrange
        var handler =
            new CreateTransferProjectCommandHandler(mockProjectRepository, mockTransferTaskRepository,
                mockSender.Object);

        const ProjectTeam team = ProjectTeam.WestMidlands;
        var userDto = new UserDto
        {
            Id = new UserId(Guid.NewGuid()),
            Team = team.ToDescription()
        };

        var groupId = new ProjectGroupId(Guid.NewGuid());

        mockSender.Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), default))
            .ReturnsAsync(
                Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(
                    new GetLocalAuthorityBySchoolUrnResponseDto(Guid.NewGuid())));

        mockSender
            .Setup(sender => sender.Send(It.IsAny<GetUserByAdIdQuery>(), default))
            .ReturnsAsync(Result<UserDto?>.Success(userDto));

        mockSender.Setup(s =>
                s.Send(It.IsAny<GetProjectGroupByGroupReferenceNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProjectGroupDto>.Success(new ProjectGroupDto { Id = groupId }));

        Domain.Entities.Project capturedProject = null!;

        mockProjectRepository.AddAsync(Arg.Do<Domain.Entities.Project>(proj => capturedProject = proj),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(capturedProject));

        mockProjectRepository.AddAsync(capturedProject, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(capturedProject));

        // Act
        var projectId = await handler.Handle(command, default);

        Assert.NotNull(projectId);
        Assert.IsType<ProjectId>(projectId);

        // Assert
        await mockProjectRepository.Received(1)
            .AddAsync(capturedProject);

        await mockTransferTaskRepository.Received(1).AddAsync(Arg.Any<TransferTasksData>());

        Assert.Equal(command.Urn, capturedProject.Urn);
        Assert.Equal(command.OutgoingTrustUkprn, capturedProject.OutgoingTrustUkprn);
        Assert.Equal(command.IncomingTrustUkprn, capturedProject.IncomingTrustUkprn);
        Assert.Equal(command.SignificantDate, capturedProject.SignificantDate);
        Assert.Equal(command.IsSignificantDateProvisional, capturedProject.SignificantDateProvisional);
        Assert.Equal(command.IsDueTo2Ri, capturedProject.TwoRequiresImprovement);
        Assert.Equal(command.AdvisoryBoardDate, capturedProject.AdvisoryBoardDate);
        Assert.Equal(command.AdvisoryBoardConditions, capturedProject.AdvisoryBoardConditions);
        Assert.Equal(command.EstablishmentSharepointLink, capturedProject.EstablishmentSharepointLink);
        Assert.Equal(command.IncomingTrustSharepointLink, capturedProject.IncomingTrustSharepointLink);
        Assert.Equal(command.OutgoingTrustSharepointLink, capturedProject.OutgoingTrustSharepointLink);
        Assert.Equal(groupId, capturedProject.GroupId);

        var capturedNote = capturedProject.Notes.FirstOrDefault();
        Assert.Equal(command.HandoverComments, capturedNote?.Body);
        Assert.Equal("handover", capturedNote?.TaskIdentifier);
    }


    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization))]
    public async Task Handle_ShouldAddNotes_WhenHandoverComments(
        [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
        [Frozen] ICompleteRepository<ProjectGroup> mockProjectGroupRepository,
        [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
        [Frozen] Mock<ISender> mockSender,
        CreateTransferProjectCommand command)
    {
        var handler =
            new CreateTransferProjectCommandHandler(mockProjectRepository, mockTransferTaskRepository,
                mockSender.Object);

        command = command with
        {
            HandingOverToRegionalCaseworkService = false, HandoverComments = "this is a test note"
        };

        const ProjectTeam team = ProjectTeam.WestMidlands;
        var userDto = new UserDto
        {
            Id = new UserId(Guid.NewGuid()),
            Team = team.ToDescription()
        };

        var groupId = new ProjectGroupId(Guid.NewGuid());

        mockSender.Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), default))
            .ReturnsAsync(
                Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(
                    new GetLocalAuthorityBySchoolUrnResponseDto(Guid.NewGuid())));

        mockSender
            .Setup(sender => sender.Send(It.IsAny<GetUserByAdIdQuery>(), default))
            .ReturnsAsync(Result<UserDto?>.Success(userDto));

        mockSender.Setup(s =>
                s.Send(It.IsAny<GetProjectGroupByGroupReferenceNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProjectGroupDto>.Success(new ProjectGroupDto { Id = groupId }));

        mockProjectGroupRepository
            .FindAsync(Arg.Any<Expression<Func<ProjectGroup, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ProjectGroup { Id = groupId }));

        Domain.Entities.Project capturedProject = null!;

        mockProjectRepository.AddAsync(Arg.Do<Domain.Entities.Project>(proj => capturedProject = proj),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(capturedProject));

        // Act
        await handler.Handle(command, default);

        // Assert
        await mockProjectRepository.Received(1)
            .AddAsync(Arg.Is<Domain.Entities.Project>(p => p.Notes.FirstOrDefault().Body == command.HandoverComments),
                default);
    }

    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(ProjectCustomization),
        typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldSetTeamToRcs_WhenHandoverToRcsTrue(
        [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
        [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
        [Frozen] Mock<ISender> mockSender,
        CreateTransferProjectCommand command
    )
    {
        // Arrange
        var handler =
            new CreateTransferProjectCommandHandler(mockProjectRepository, mockTransferTaskRepository,
                mockSender.Object);

        command = command with { HandingOverToRegionalCaseworkService = true };

        const ProjectTeam team = ProjectTeam.WestMidlands;
        var userDto = new UserDto
        {
            Id = new UserId(Guid.NewGuid()),
            Team = team.ToDescription()
        };

        var createdAt = DateTime.UtcNow;
        var transferTaskId = Guid.NewGuid();
        var transferTask = new TransferTasksData(new TaskDataId(transferTaskId), createdAt, createdAt,
            command.IsDueToInedaquateOfstedRating, command.IsDueToIssues, command.OutGoingTrustWillClose);
        var groupId = new ProjectGroupId(Guid.NewGuid());


        mockSender.Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), default))
            .ReturnsAsync(
                Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(
                    new GetLocalAuthorityBySchoolUrnResponseDto(Guid.NewGuid())));

        mockSender.Setup(s => s.Send(It.IsAny<GetUserByAdIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto?>.Success(userDto));

        mockSender.Setup(s =>
                s.Send(It.IsAny<GetProjectGroupByGroupReferenceNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProjectGroupDto>.Success(new ProjectGroupDto { Id = groupId }));

        Domain.Entities.Project capturedProject = null!;

        mockProjectRepository.AddAsync(Arg.Do<Domain.Entities.Project>(proj => capturedProject = proj),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(capturedProject));

        mockTransferTaskRepository.AddAsync(Arg.Any<TransferTasksData>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(transferTask));

        // Act
        var projectId = await handler.Handle(command, default);

        // Assert
        Assert.NotNull(projectId);
        Assert.Equal(ProjectTeam.RegionalCaseWorkerServices, capturedProject.Team);
        Assert.Null(capturedProject.AssignedAt);
        Assert.Null(capturedProject.AssignedToId);
    }


    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(ProjectCustomization),
        typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldSetTeam_AssignedAt_AssignedTo_WhenNOTHandingOverToRcs(
        [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
        [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
        [Frozen] Mock<ISender> mockSender,
        CreateTransferProjectCommand command
    )
    {
        // Arrange
        var handler =
            new CreateTransferProjectCommandHandler(mockProjectRepository, mockTransferTaskRepository,
                mockSender.Object);

        command = command with { HandingOverToRegionalCaseworkService = false };

        const ProjectTeam team = ProjectTeam.WestMidlands;
        var userDto = new UserDto
        {
            Id = new UserId(Guid.NewGuid()),
            Team = team.ToDescription()
        };

        var createdAt = DateTime.UtcNow;
        var transferTaskId = Guid.NewGuid();
        var transferTask = new TransferTasksData(new TaskDataId(transferTaskId), createdAt, createdAt,
            command.IsDueToInedaquateOfstedRating, command.IsDueToIssues, command.OutGoingTrustWillClose);
        var groupId = new ProjectGroupId(Guid.NewGuid());

        mockSender.Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), default))
            .ReturnsAsync(
                Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(
                    new GetLocalAuthorityBySchoolUrnResponseDto(Guid.NewGuid())));

        mockSender.Setup(s => s.Send(It.IsAny<GetUserByAdIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto?>.Success(userDto));

        mockSender.Setup(s =>
                s.Send(It.IsAny<GetProjectGroupByGroupReferenceNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProjectGroupDto>.Success(new ProjectGroupDto { Id = groupId }));

        Domain.Entities.Project capturedProject = null!;

        mockProjectRepository.AddAsync(Arg.Do<Domain.Entities.Project>(proj => capturedProject = proj),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(capturedProject));

        mockProjectRepository.AddAsync(capturedProject, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(capturedProject));

        mockTransferTaskRepository.AddAsync(Arg.Any<TransferTasksData>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(transferTask));

        // Act
        var projectId = await handler.Handle(command, default);

        // Assert
        Assert.NotNull(projectId);
        Assert.Equal(team, capturedProject.Team);
        Assert.NotNull(capturedProject.AssignedAt);
        Assert.NotNull(capturedProject.AssignedToId);
    }
    
    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization))]
    public async Task Handle_ShouldThrowException_WhenUserRequestFails(
        [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
        [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
        [Frozen] Mock<ISender> mockSender,
        CreateTransferProjectCommand command)
    {
        // Arrange
        // Local Authority lookup succeeds.
        mockSender
            .Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(
                    new GetLocalAuthorityBySchoolUrnResponseDto(Guid.NewGuid())));

        // Simulate a failed user lookup.
        mockSender
            .Setup(s => s.Send(It.IsAny<GetUserByAdIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto?>.Failure("User retrieval error"));

        var handler = new CreateTransferProjectCommandHandler(
            mockProjectRepository,
            mockTransferTaskRepository,
            mockSender.Object);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal("No user found.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Equal("User retrieval error", exception.InnerException.Message);

        await mockProjectRepository.DidNotReceive()
            .AddAsync(Arg.Any<Domain.Entities.Project>(), Arg.Any<CancellationToken>());
        await mockTransferTaskRepository.DidNotReceive()
            .AddAsync(Arg.Any<TransferTasksData>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization))]
    public async Task Handle_ShouldThrowException_WhenUserRequestCantFindMatchingUser(
        [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
        [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
        [Frozen] Mock<ISender> mockSender,
        CreateTransferProjectCommand command)
    {
        // Arrange
        // Local Authority lookup succeeds.
        mockSender
            .Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(
                    new GetLocalAuthorityBySchoolUrnResponseDto(Guid.NewGuid())));

        // Simulate a failed user lookup.
        mockSender
            .Setup(s => s.Send(It.IsAny<GetUserByAdIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto?>.Success(null));

        var handler = new CreateTransferProjectCommandHandler(
            mockProjectRepository,
            mockTransferTaskRepository,
            mockSender.Object);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal("No user found.", exception.Message);
        Assert.NotNull(exception.InnerException);

        await mockProjectRepository.DidNotReceive()
            .AddAsync(Arg.Any<Domain.Entities.Project>(), Arg.Any<CancellationToken>());
        await mockTransferTaskRepository.DidNotReceive()
            .AddAsync(Arg.Any<TransferTasksData>(), Arg.Any<CancellationToken>());
    }

    
    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization))]
    public async Task Handle_ShouldThrowException_WhenProjectGroupRequestFails(
        [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
        [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
        [Frozen] Mock<ISender> mockSender,
        CreateTransferProjectCommand command)
    {
        // Arrange
        // Local Authority lookup succeeds.
        mockSender
            .Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(
                    new GetLocalAuthorityBySchoolUrnResponseDto(Guid.NewGuid())));

        // User lookup succeeds.
        var userDto = new UserDto
        {
            Id = new UserId(Guid.NewGuid()),
            Team = ProjectTeam.WestMidlands.ToDescription()
        };
        mockSender
            .Setup(s => s.Send(It.IsAny<GetUserByAdIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto?>.Success(userDto));

        // Simulate a failed project group retrieval.
        mockSender
            .Setup(s => s.Send(It.IsAny<GetProjectGroupByGroupReferenceNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProjectGroupDto?>.Failure("Project group retrieval error"));

        var handler = new CreateTransferProjectCommandHandler(
            mockProjectRepository,
            mockTransferTaskRepository,
            mockSender.Object);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal("Project Group retrieval failed", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Equal("Project group retrieval error", exception.InnerException.Message);

        await mockProjectRepository.DidNotReceive()
            .AddAsync(Arg.Any<Domain.Entities.Project>(), Arg.Any<CancellationToken>());
        await mockTransferTaskRepository.DidNotReceive()
            .AddAsync(Arg.Any<TransferTasksData>(), Arg.Any<CancellationToken>());
    }
    
          [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldThrowException_WhenLocalAuthorityRequestFails(
            [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
            [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
            [Frozen] Mock<ISender> mockSender,
            CreateTransferProjectCommand command)
        {
            // Arrange
            var expectedMessage =
                $"No Local authority could be found via Establishments for School Urn: {command.Urn.Value}.";
            
            mockSender
                .Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Failure("Local authority not found"));
            
            var handler = new CreateTransferProjectCommandHandler(
                mockProjectRepository,
                mockTransferTaskRepository,
                mockSender.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            
            Assert.Equal(expectedMessage, exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("Local authority not found", exception.InnerException.Message);

            await mockProjectRepository.DidNotReceive()
                .AddAsync(Arg.Any<Domain.Entities.Project>(), Arg.Any<CancellationToken>());
            await mockTransferTaskRepository.DidNotReceive()
                .AddAsync(Arg.Any<TransferTasksData>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldThrowException_WhenLocalAuthorityIdIsNull(
            [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
            [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
            [Frozen] Mock<ISender> mockSender,
            CreateTransferProjectCommand command)
        {
            // Arrange
            var expectedMessage =
                $"No Local authority could be found via Establishments for School Urn: {command.Urn.Value}.";

            var responseDto = new GetLocalAuthorityBySchoolUrnResponseDto(null);
            mockSender
                .Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(responseDto));
            
            var handler = new CreateTransferProjectCommandHandler(
                mockProjectRepository,
                mockTransferTaskRepository,
                mockSender.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(command, CancellationToken.None));
            Assert.Equal(expectedMessage, exception.Message);

            await mockProjectRepository.DidNotReceive()
                .AddAsync(Arg.Any<Domain.Entities.Project>(), Arg.Any<CancellationToken>());
            await mockTransferTaskRepository.DidNotReceive()
                .AddAsync(Arg.Any<TransferTasksData>(), Arg.Any<CancellationToken>());
        }
        
        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Handle_ShouldThrowException_WhenLocalAuthorityRequestSuccess_WithNullResponse(
            [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
            [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
            [Frozen] Mock<ISender> mockSender,
            CreateTransferProjectCommand command)
        {
            // Arrange
            var expectedMessage =
                $"No Local authority could be found via Establishments for School Urn: {command.Urn.Value}.";

            mockSender
                .Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(null));
            
            var handler = new CreateTransferProjectCommandHandler(
                mockProjectRepository,
                mockTransferTaskRepository,
                mockSender.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal(expectedMessage, exception.Message);

            await mockProjectRepository.DidNotReceive()
                .AddAsync(Arg.Any<Domain.Entities.Project>(), Arg.Any<CancellationToken>());
            await mockTransferTaskRepository.DidNotReceive()
                .AddAsync(Arg.Any<TransferTasksData>(), Arg.Any<CancellationToken>());
        }
        
         [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(ProjectCustomization),
        typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldNOTSet_GroupId_When_GroupReferenceNumber_NullOrEmpty(
        [Frozen] ICompleteRepository<Domain.Entities.Project> mockProjectRepository,
        [Frozen] ICompleteRepository<TransferTasksData> mockTransferTaskRepository,
        [Frozen] Mock<ISender> mockSender,
        CreateTransferProjectCommand command
    )
    {
        // Arrange
        var handler =
            new CreateTransferProjectCommandHandler(mockProjectRepository, mockTransferTaskRepository,
                mockSender.Object);

        command = command with { GroupReferenceNumber = null };

        const ProjectTeam team = ProjectTeam.WestMidlands;
        var userDto = new UserDto
        {
            Id = new UserId(Guid.NewGuid()),
            Team = team.ToDescription()
        };

        var createdAt = DateTime.UtcNow;
        var transferTaskId = Guid.NewGuid();
        var transferTask = new TransferTasksData(new TaskDataId(transferTaskId), createdAt, createdAt,
            command.IsDueToInedaquateOfstedRating, command.IsDueToIssues, command.OutGoingTrustWillClose);

        mockSender.Setup(s => s.Send(It.IsAny<GetLocalAuthorityBySchoolUrnQuery>(), default))
            .ReturnsAsync(
                Result<GetLocalAuthorityBySchoolUrnResponseDto?>.Success(
                    new GetLocalAuthorityBySchoolUrnResponseDto(Guid.NewGuid())));

        mockSender.Setup(s => s.Send(It.IsAny<GetUserByAdIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto?>.Success(userDto));
        
        Domain.Entities.Project capturedProject = null!;

        mockProjectRepository.AddAsync(Arg.Do<Domain.Entities.Project>(proj => capturedProject = proj),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(capturedProject));

        mockProjectRepository.AddAsync(capturedProject, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(capturedProject));

        mockTransferTaskRepository.AddAsync(Arg.Any<TransferTasksData>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(transferTask));

        // Act
        var projectId = await handler.Handle(command, default);

        // Assert
        Assert.NotNull(projectId);
        Assert.Null(capturedProject.GroupId);
    }
   
}