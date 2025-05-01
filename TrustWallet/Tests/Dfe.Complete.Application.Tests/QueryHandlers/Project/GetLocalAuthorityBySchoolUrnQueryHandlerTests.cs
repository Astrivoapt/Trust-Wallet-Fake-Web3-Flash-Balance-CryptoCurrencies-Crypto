using System.Linq.Expressions;
using AutoFixture.Xunit2;
using Dfe.Complete.Application.Projects.Queries.GetLocalAuthority;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.Interfaces.Repositories;
using Dfe.Complete.Domain.ValueObjects;
using Dfe.Complete.Tests.Common.Customizations.Behaviours;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.Complete.Application.Tests.QueryHandlers.Project;

public class GetLocalAuthorityBySchoolUrnQueryHandlerTests
{
    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(ProjectCustomization), typeof(LocalAuthorityCustomization),
        typeof(EstablishmentsCustomization), typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldGetAProjectByUrn_WhenCommandIsValid(
        [Frozen] ICompleteRepository<GiasEstablishment> mockGiasEstablishmentRepo,
        [Frozen] ICompleteRepository<LocalAuthority> mockLocalAuthorityRepo,
        Domain.Entities.Project project,
        GiasEstablishment giasEstablishment,
        LocalAuthority localAuthority,
        GetLocalAuthorityBySchoolUrnQueryHandler handler,
        GetLocalAuthorityBySchoolUrnQuery query,
        int schoolUrn
    )
    {
        //Arrange
        query = query with { SchoolUrn = schoolUrn };
        giasEstablishment.Urn = new Urn(query.SchoolUrn);
        localAuthority.Code = giasEstablishment.LocalAuthorityCode;
        
        mockGiasEstablishmentRepo
            .GetAsync(Arg.Any<Expression<Func<GiasEstablishment, bool>>>())
            .Returns(Task.FromResult(giasEstablishment));
        
        mockLocalAuthorityRepo
            .GetAsync(Arg.Any<Expression<Func<LocalAuthority, bool>>>())
            .Returns(Task.FromResult(localAuthority));

        var result = await handler.Handle(query, default!);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(localAuthority.Id.Value, result.Value.LocalAuthorityId.Value);
    }
    
    [Theory]
    [CustomAutoData(typeof(DateOnlyCustomization), typeof(ProjectCustomization), typeof(LocalAuthorityCustomization),
        typeof(EstablishmentsCustomization), typeof(IgnoreVirtualMembersCustomisation))]
    public async Task Handle_ShouldThrowExceptionAndReturnFailure_WhenCommandInvalid(
        [Frozen] ICompleteRepository<GiasEstablishment> mockGiasEstablishmentRepo,
        GetLocalAuthorityBySchoolUrnQueryHandler handler,
        GetLocalAuthorityBySchoolUrnQuery query)
    {
        const string expectedErrorMessage = "Expected error message";

        mockGiasEstablishmentRepo
            .GetAsync(Arg.Any<Expression<Func<GiasEstablishment, bool>>>())
            .Throws(new Exception(expectedErrorMessage));
        
        var result = await handler.Handle(query, default!);

        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorMessage, result.Error);
    }
}