using Dfe.Complete.Application.LocalAuthorities.Models;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.ValueObjects;
using FluentAssertions;

namespace Dfe.Complete.Application.Tests.LocalAuthorities.ModelsTests;

public class LocalAuthoritiesModelsTests {
    [Fact]
    public void MapLAEntityToDto_Should_Map_Properties_Correctly()
    {
        var model = new LocalAuthority
        {
            Id = new LocalAuthorityId(new Guid()),
            Name = "Test Name",
            Address2 = "64 Zoo Lane"
        };

        var dto = LocalAuthorityDto.MapLAEntityToDto(model);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(model.Id);
        dto.Name.Should().Be(model.Name);
        dto.Address2.Should().Be(model.Address2);
    }

    [Fact]
    public void MapLAEntityToDto_Should_Handle_Nullable_Property()
    {
        var model = new LocalAuthority
        {
            Id = new LocalAuthorityId(new Guid()),
            Name = "Another Test",
            Address2 = null
        };

        var dto = LocalAuthorityDto.MapLAEntityToDto(model);

        dto.Address2.Should().BeNull();
    }
}
