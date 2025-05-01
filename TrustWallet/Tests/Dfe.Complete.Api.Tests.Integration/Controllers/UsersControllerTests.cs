using AutoFixture;
using Dfe.Complete.Api.Tests.Integration.Customizations;
using Dfe.Complete.Client.Contracts;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Infrastructure.Database;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.Mocks.WebApplicationFactory;
using Microsoft.EntityFrameworkCore;
using Project = Dfe.Complete.Domain.Entities.Project;

namespace Dfe.Complete.Api.Tests.Integration.Controllers;

public class UsersControllerTests
{
    [Theory]
    [CustomAutoData(typeof(CustomWebApplicationDbContextFactoryCustomization), typeof(EstablishmentsCustomization))]
    public async Task ListUsersWithProjectsAsync_ShouldReturnList(
        CustomWebApplicationDbContextFactory<Program> factory,
        IUsersClient usersClient,
        IFixture fixture)
    {
        //todo: when auth is done, add this back in
        // Arrange
        var dbContext = factory.GetDbContext<CompleteContext>();
        var testUser = await dbContext.Users.FirstAsync();
        testUser.FirstName = "Nick";
        testUser.LastName = "Warms";
        dbContext.Users.Update(testUser);
        
        var establishments = fixture.CreateMany<GiasEstablishment>(50).ToList();
        await dbContext.GiasEstablishments.AddRangeAsync(establishments);
        var projects = establishments.Select(establishment =>
        {
            var project = fixture.Customize(new ProjectCustomization
                {
                    RegionalDeliveryOfficerId = testUser.Id,
                    CaseworkerId = testUser.Id,
                    AssignedToId = testUser.Id,
                })
                .Create<Project>();
            project.Urn = establishment.Urn ?? project.Urn;
            return project;
        }).ToList();
        
        projects.ForEach(p => p.LocalAuthorityId = dbContext.LocalAuthorities.FirstOrDefault().Id);
        
        await dbContext.Projects.AddRangeAsync(projects);

        await dbContext.SaveChangesAsync();

        // Act
        var results = await usersClient.ListAllUsersWithProjectsAsync(
            null, 0, 1000);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        var result = results[0];
        Assert.Equal(testUser.Email, result.Email);
        Assert.Equal(testUser.Id.Value, result.Id.Value);
        Assert.Equal($"{testUser.FirstName} {testUser.LastName}", result.FullName);
        
        Assert.NotNull(result.ProjectsAssigned);
        Assert.Equal(50, result.ProjectsAssigned!.Count);
        foreach (var assignedProject in result.ProjectsAssigned!)
        {
            var expectedProject = projects.Find(p => p.Id.Value == assignedProject.ProjectId?.Value);
            var expectedEstablishment = establishments.Find(e => e.Urn?.Value == assignedProject.Urn?.Value);

            Assert.Null(assignedProject.EstablishmentName);
            // Assert.Equal(establishment?.Name, result.EstablishmentName);

            Assert.NotNull(assignedProject.ProjectId);
            Assert.Equal(expectedProject?.Id.Value, assignedProject.ProjectId.Value);

            Assert.NotNull(assignedProject.Urn);
            Assert.Equal(expectedProject?.Urn.Value, assignedProject.Urn.Value);
            Assert.Equal(expectedEstablishment?.Urn?.Value, assignedProject.Urn.Value);

            Assert.NotNull(assignedProject.ConversionOrTransferDate);
            Assert.Equal(expectedProject?.SignificantDate, new DateOnly(assignedProject.ConversionOrTransferDate.Value.Year,
                assignedProject.ConversionOrTransferDate.Value.Month, assignedProject.ConversionOrTransferDate.Value.Day));

            Assert.NotNull(assignedProject.State);
            Assert.Equal(expectedProject?.State.ToString(), assignedProject.State.ToString());

            Assert.NotNull(assignedProject.ProjectType);
            Assert.Equal(expectedProject?.Type?.ToString(), assignedProject.ProjectType.Value.ToString());

            Assert.Equal(expectedProject?.FormAMat, assignedProject.IsFormAMAT);

            Assert.Null(assignedProject.AssignedToFullName);
        }
    }
    
    [Theory]
    [CustomAutoData(typeof(CustomWebApplicationDbContextFactoryCustomization), typeof(EstablishmentsCustomization))]
    public async Task GetUserWithProjectsAsync_ShouldReturnList(
        CustomWebApplicationDbContextFactory<Program> factory,
        IUsersClient usersClient,
        IFixture fixture)
    {
        //todo: when auth is done, add this back in
        // Arrange
        var dbContext = factory.GetDbContext<CompleteContext>();
        var testUser = await dbContext.Users.FirstAsync();
        testUser.FirstName = "Nick";
        testUser.LastName = "Warms";
        dbContext.Users.Update(testUser);

        var localAuthorityId = dbContext.LocalAuthorities.FirstOrDefault().Id;
        
        var establishments = fixture.CreateMany<GiasEstablishment>(50).ToList();
        await dbContext.GiasEstablishments.AddRangeAsync(establishments);
        var projects = establishments.Select(establishment =>
        {
            var project = fixture.Customize(new ProjectCustomization
                {
                    RegionalDeliveryOfficerId = testUser.Id,
                    CaseworkerId = testUser.Id,
                    AssignedToId = testUser.Id
                })
                .Create<Project>();
            project.Urn = establishment.Urn ?? project.Urn;
            return project;
        }).ToList();
        
        projects.ForEach(p => p.LocalAuthorityId = localAuthorityId);
        
        await dbContext.Projects.AddRangeAsync(projects);

        await dbContext.SaveChangesAsync();

        // Act
        var results = await usersClient.GetUserWithProjectsAsync( testUser.Id.Value,
            null, 0, 1000);

        // Assert
        Assert.NotNull(results);
        Assert.NotNull(results.ProjectsAssigned);
        Assert.Equal(50, results.ProjectsAssigned!.Count);
        foreach (var result in results.ProjectsAssigned!)
        {
            var project = projects.Find(p => p.Id.Value == result.ProjectId?.Value);
            var establishment = establishments.Find(e => e.Urn?.Value == result.Urn?.Value);

            Assert.NotNull(result.EstablishmentName);
            Assert.Equal(establishment?.Name, result.EstablishmentName);

            Assert.NotNull(result.ProjectId);
            Assert.Equal(project?.Id.Value, result.ProjectId.Value);

            Assert.NotNull(result.Urn);
            Assert.Equal(project?.Urn.Value, result.Urn.Value);
            Assert.Equal(establishment?.Urn?.Value, result.Urn.Value);

            Assert.NotNull(result.ConversionOrTransferDate);
            Assert.Equal(project?.SignificantDate, new DateOnly(result.ConversionOrTransferDate.Value.Year,
                result.ConversionOrTransferDate.Value.Month, result.ConversionOrTransferDate.Value.Day));

            Assert.NotNull(result.State);
            Assert.Equal(project?.State.ToString(), result.State.ToString());

            Assert.NotNull(result.ProjectType);
            Assert.Equal(project?.Type?.ToString(), result.ProjectType.Value.ToString());

            Assert.Equal(project?.FormAMat, result.IsFormAMAT);

            Assert.NotNull(result.AssignedToFullName);
            Assert.Equal($"{project?.AssignedTo?.FirstName} {project?.AssignedTo?.LastName}",
                result.AssignedToFullName);
        }
    }
}
