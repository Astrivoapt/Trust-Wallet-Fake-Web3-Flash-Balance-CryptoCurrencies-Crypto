using System.Collections.ObjectModel;
using AutoFixture;
using AutoFixture.Xunit2;
using Dfe.Complete.Application.Users.Queries.GetUser;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Domain.Interfaces.Repositories;
using Dfe.Complete.Tests.Common.Customizations.Behaviours;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using MockQueryable;
using NSubstitute;

namespace Dfe.Complete.Application.Tests.QueryHandlers.User;

public class GetUserWithProjectsQueryHandlerTests
{
    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(UserCustomization), typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldReturnSuccess_WhenProjectIsFound(
        [Frozen] ICompleteRepository<Domain.Entities.User> mockUserRepository,
        [Frozen] ICompleteRepository<GiasEstablishment> mockEstablishmentRepository,
        GetUserWithProjectsHandler handler,
        Domain.Entities.User user,
        GiasEstablishment establishment,
        IFixture fixture)
    {
        // Arrange
        var conversionProject = fixture.Customize(new ProjectCustomization
        {
            Urn = establishment.Urn,
            Type = ProjectType.Conversion,
            AssignedToId = user.Id
        }).Create<Domain.Entities.Project>();
        
        var transferProject = fixture.Customize(new ProjectCustomization
        {
            Urn = establishment.Urn,
            Type = ProjectType.Transfer,
            AssignedToId = user.Id
        }).Create<Domain.Entities.Project>();
        

        var projectList = new List<Domain.Entities.Project>{ conversionProject, transferProject };
        user.ProjectAssignedTos = projectList;
        var userList = new List<Domain.Entities.User>{user};
        var establishmentList = new List<GiasEstablishment>{establishment};
        
        var establishmemtQueryable = establishmentList.BuildMock();
        var userQueryable = userList.BuildMock();
        
        mockUserRepository
            .Query()
            .Returns(userQueryable);
        
        mockEstablishmentRepository
            .Query()
            .Returns(establishmemtQueryable);

        var query = new GetUserWithProjectsQuery(user.Id, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(user.FullName, result.Value.FullName);
        Assert.Equal(user.Email, result.Value.Email);
        Assert.Equal(1, result.Value.ConversionProjectsAssigned);
        Assert.Equal(1, result.Value.TransferProjectsAssigned);
    }
}