using System.Collections.ObjectModel;
using AutoFixture;
using AutoFixture.Xunit2;
using Dfe.AcademiesApi.Client.Contracts;
using Dfe.Complete.Application.Common.Models;
using Dfe.Complete.Application.Projects.Interfaces;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Application.Projects.Queries.GetUser;
using Dfe.Complete.Application.Projects.Queries.ListAllProjects;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Domain.ValueObjects;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using MediatR;
using MockQueryable;
using Moq;
using NSubstitute;

namespace Dfe.Complete.Application.Tests.QueryHandlers.Project;

public class ListAllProjectsForUserTests
{
    [Theory]
    [CustomAutoData(
        typeof(OmitCircularReferenceCustomization),
        typeof(ListAllProjectsQueryModelCustomization),
        typeof(DateOnlyCustomization))]
    public async Task Handle_ShouldReturnCorrectList_WhenPaginationIsCorrect(
        [Frozen] IListAllProjectsByFilterQueryService mockListAllProjectsByFilterQueryService,
        [Frozen] Mock<ISender> mockSender,
        IFixture fixture)
    {
        //Arrange 
        var mockTrustsClient = new Mock<ITrustsV4Client>();

        var handler = new ListAllProjectsForUserQueryHandler(mockListAllProjectsByFilterQueryService, 
            mockTrustsClient.Object,
            mockSender.Object);

        var userDto = fixture.Create<UserDto>();
        mockSender.Setup(sender => sender.Send(It.IsAny<GetUserByAdIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto?>.Success(userDto));

        var mockListAllProjectsForUserQueryModels = fixture.CreateMany<ListAllProjectsQueryModel>(50);

        var trustDtos = new ObservableCollection<TrustDto>(
            fixture.Build<TrustDto>().With(dto => dto.Ukprn, new Ukprn(new Random().Next()).Value.ToString)
                .CreateMany(10));

        mockTrustsClient.Setup(service => service.GetByUkprnsAllAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync(trustDtos);

        foreach (var projectsQueryModel in mockListAllProjectsForUserQueryModels.ToList())
        {
            projectsQueryModel.Project.IncomingTrustUkprn = trustDtos.OrderBy(_ => new Random().Next()).First().Ukprn;
            projectsQueryModel.Project.OutgoingTrustUkprn = trustDtos.OrderBy(_ => new Random().Next()).First().Ukprn;
        }

        var trustList = trustDtos.ToList();

        var expected = mockListAllProjectsForUserQueryModels.Select(item =>
                ListAllProjectsForUserQueryResultModel
                    .MapProjectAndEstablishmentToListAllProjectsForUserQueryResultModel(
                        item.Project,
                        item.Establishment,
                        trustList.FirstOrDefault(t => t.Ukprn == item.Project.OutgoingTrustUkprn).Name,
                        trustList.FirstOrDefault(t => t.Ukprn == item.Project.IncomingTrustUkprn).Name))
            .Skip(20).Take(20).ToList();

        mockListAllProjectsByFilterQueryService.ListAllProjectsByFilter(ProjectState.Active, null, userId: userDto.Id)
            .Returns(mockListAllProjectsForUserQueryModels.BuildMock());

        var query = new ListAllProjectForUserQuery(ProjectState.Active, userDto.ActiveDirectoryUserId) { Page = 1 };

        //Act
        var result = await handler.Handle(query, default);

        //Assert
        Assert.NotNull(result);

        Assert.Equal(expected.Count, result.Value?.Count);
        for (var i = 0; i < result.Value!.Count; i++)
        {
            Assert.Equivalent(expected[i], result.Value![i]);
        }
    }

    [Theory]
    [CustomAutoData(
        typeof(OmitCircularReferenceCustomization),
        typeof(ListAllProjectsQueryModelCustomization),
        typeof(DateOnlyCustomization))]
    public async Task Handle_ShouldReturnCorrectList_WhenAllPagesAreSkipped(
        [Frozen] IListAllProjectsByFilterQueryService mockListAllProjectsByFilterQueryService,
        [Frozen] Mock<ISender> mockSender,
        IFixture fixture)
    {
        //Arrange 
        var mockTrustsClient = new Mock<ITrustsV4Client>();

        var handler = new ListAllProjectsForUserQueryHandler(mockListAllProjectsByFilterQueryService, mockTrustsClient.Object,
            mockSender.Object);

        var userDto = fixture.Create<UserDto>();
        mockSender.Setup(sender => sender.Send(It.IsAny<GetUserByAdIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto?>.Success(userDto));

        var mockListAllProjectsForUserQueryModels = fixture.CreateMany<ListAllProjectsQueryModel>(50);

        var trustDtos = new ObservableCollection<TrustDto>(
            fixture.Build<TrustDto>().With(dto => dto.Ukprn, new Ukprn(new Random().Next()).Value.ToString)
                .CreateMany(10));

        mockTrustsClient.Setup(service => service.GetByUkprnsAllAsync(It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync(trustDtos);

        foreach (var projectsQueryModel in mockListAllProjectsForUserQueryModels.ToList())
        {
            projectsQueryModel.Project.IncomingTrustUkprn = trustDtos.OrderBy(_ => new Random().Next()).First().Ukprn;
            projectsQueryModel.Project.OutgoingTrustUkprn = trustDtos.OrderBy(_ => new Random().Next()).First().Ukprn;
        }
        
        mockListAllProjectsByFilterQueryService.ListAllProjectsByFilter(ProjectState.Active, null, userId: userDto.Id)
            .Returns(mockListAllProjectsForUserQueryModels.BuildMock());

        var query = new ListAllProjectForUserQuery(ProjectState.Active, userDto.ActiveDirectoryUserId) { Page = 50 };

        //Act
        var result = await handler.Handle(query, default);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value?.Count);
    }
    
    [Theory]
    [CustomAutoData(
        typeof(OmitCircularReferenceCustomization),
        typeof(ListAllProjectsQueryModelCustomization),
        typeof(DateOnlyCustomization))]
    public async Task Handle_ShouldReturnUnsuccessful_WhenAnErrorOccurs(
        [Frozen] IListAllProjectsByFilterQueryService mockListAllProjectsByFilterQueryService,
        [Frozen] Mock<ISender> mockSender,
        IFixture fixture)
    {
        //Arrange 
        var mockTrustsClient = new Mock<ITrustsV4Client>();

        var handler = new ListAllProjectsForUserQueryHandler(mockListAllProjectsByFilterQueryService, mockTrustsClient.Object,
            mockSender.Object);

        const string errorMessage = "this is a test";
        
        var userDto = fixture.Create<UserDto>();
        mockSender.Setup(sender => sender.Send(It.IsAny<GetUserByAdIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));
        
        var query = new ListAllProjectForUserQuery(ProjectState.Active, userDto.ActiveDirectoryUserId) { Page = 50 };

        //Act
        var result = await handler.Handle(query, default);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.Error);
    }
}