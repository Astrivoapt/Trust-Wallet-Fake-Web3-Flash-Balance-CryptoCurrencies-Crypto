using AutoFixture;
using AutoFixture.Xunit2;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Application.Users.Models;
using Dfe.Complete.Application.Users.Queries.GetUser;
using Dfe.Complete.Application.Users.Queries.ListAllUsers;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Domain.Interfaces.Repositories;
using Dfe.Complete.Tests.Common.Customizations.Behaviours;
using Dfe.Complete.Tests.Common.Customizations.Models;
using Dfe.Complete.Utils;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using MockQueryable;
using NSubstitute;

namespace Dfe.Complete.Application.Tests.QueryHandlers.User;

public class ListAllUsersWithProjectsHandlerTest
{
    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(UserCustomization),
        typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldReturnSuccess_WhenUsersWithProjectIsFound(
        [Frozen] ICompleteRepository<Domain.Entities.User> mockUserRepository,
        ListAllUsersWithProjectsHandler handler,
        IFixture fixture)
    {
        // Arrange
        var users = fixture.CreateMany<Domain.Entities.User>(20)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();

        for (int i = 0; i < users.Count; i++)
        {
            users[i].ProjectAssignedTos = fixture.CreateMany<Domain.Entities.Project>(i + 1).ToList();
        }

        var expected = users.Select(user => new UserWithProjectsDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Team.FromDescriptionValue<ProjectTeam>(),
            user.ProjectAssignedTos.Select(project => new ListAllProjectsResultModel(
                null,
                project.Id,
                project.Urn,
                project.SignificantDate,
                project.State,
                project.Type,
                project.FormAMat,
                null
            )).ToList(),
            user.ProjectAssignedTos.Count(project => project.Type == ProjectType.Conversion),
            user.ProjectAssignedTos.Count(project => project.Type == ProjectType.Transfer)
        )).ToList();
        var userQueryable = users.BuildMock();

        mockUserRepository
            .Query()
            .Returns(userQueryable);

        var query = new ListAllUsersWithProjectsQuery(null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        for (int i = 0; i < result.Value!.Count; i++)
        {
            Assert.Equivalent(expected[i], result.Value![i]);
        }
    }

    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(UserCustomization),
        typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldReturnSuccess_AnNoUsersWhenThereAreNoAssignedProjects(
        [Frozen] ICompleteRepository<Domain.Entities.User> mockUserRepository,
        ListAllUsersWithProjectsHandler handler,
        IFixture fixture)
    {
        // Arrange
        var users = fixture.CreateMany<Domain.Entities.User>(20)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();

        var userQueryable = users.BuildMock();

        mockUserRepository
            .Query()
            .Returns(userQueryable);

        var query = new ListAllUsersWithProjectsQuery(null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }


    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(UserCustomization),
        typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldReturnSuccess_WhenUsersWithProjectIsFoundAndPaginationWorks(
        [Frozen] ICompleteRepository<Domain.Entities.User> mockUserRepository,
        ListAllUsersWithProjectsHandler handler,
        IFixture fixture)
    {
        // Arrange
        var users = fixture.CreateMany<Domain.Entities.User>(100)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();

        for (int i = 0; i < users.Count; i++)
        {
            users[i].ProjectAssignedTos = fixture.CreateMany<Domain.Entities.Project>(i + 1).ToList();
        }

        var expected = users.Select(user => new UserWithProjectsDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Team.FromDescriptionValue<ProjectTeam>(),
            user.ProjectAssignedTos.Select(project => new ListAllProjectsResultModel(
                null,
                project.Id,
                project.Urn,
                project.SignificantDate,
                project.State,
                project.Type,
                project.FormAMat,
                null
            )).ToList(),
            user.ProjectAssignedTos.Count(project => project.Type == ProjectType.Conversion),
            user.ProjectAssignedTos.Count(project => project.Type == ProjectType.Transfer)
        )).Skip(10).Take(5).ToList();
        var userQueryable = users.BuildMock();

        mockUserRepository
            .Query()
            .Returns(userQueryable);

        var query = new ListAllUsersWithProjectsQuery(null) { Page = 2, Count = 5 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        for (int i = 0; i < result.Value!.Count; i++)
        {
            Assert.Equivalent(expected[i], result.Value![i]);
        }
    }

    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(UserCustomization),
        typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldReturnSuccess_WhenFilterWorksSuccessfully(
        [Frozen] ICompleteRepository<Domain.Entities.User> mockUserRepository,
        ListAllUsersWithProjectsHandler handler,
        IFixture fixture)
    {
        // Arrange
        var users = fixture.CreateMany<Domain.Entities.User>(10)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();

        for (int i = 0; i < users.Count; i++)
        {
            users[i].ProjectAssignedTos = fixture.CreateMany<Domain.Entities.Project>(i + 1).ToList();
        }

        var expected = users.Where(user => user.ProjectAssignedTos.Any(project => project.State == ProjectState.Active)).Select(user => new UserWithProjectsDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Team.FromDescriptionValue<ProjectTeam>(),
            user.ProjectAssignedTos.Where(project => project.State == ProjectState.Active).Select(project => new ListAllProjectsResultModel(
                null,
                project.Id,
                project.Urn,
                project.SignificantDate,
                project.State,
                project.Type,
                project.FormAMat,
                null
            )).ToList(),
            user.ProjectAssignedTos.Where(project => project.State == ProjectState.Active).Count(project => project.Type == ProjectType.Conversion),
            user.ProjectAssignedTos.Where(project => project.State == ProjectState.Active).Count(project => project.Type == ProjectType.Transfer)
        )).ToList();
        var userQueryable = users.BuildMock();

        mockUserRepository
            .Query()
            .Returns(userQueryable);

        var query = new ListAllUsersWithProjectsQuery(ProjectState.Active);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        for (int i = 0; i < result.Value!.Count; i++)
        {
            Assert.Equivalent(expected[i], result.Value![i]);
        }
    }
}