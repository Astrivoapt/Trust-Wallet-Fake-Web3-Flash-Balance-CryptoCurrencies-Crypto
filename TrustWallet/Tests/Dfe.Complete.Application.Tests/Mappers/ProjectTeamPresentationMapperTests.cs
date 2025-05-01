using Dfe.Complete.Application.Mappers;
using Dfe.Complete.Domain.Enums;

namespace Dfe.Complete.Application.Tests.Mappers;

public class ProjectTeamPresentationMapperTests
{
    [Theory]
    [InlineData(ProjectTeam.London, "London")]
    [InlineData(ProjectTeam.SouthEast, "South East")]
    [InlineData(ProjectTeam.YorkshireAndTheHumber, "Yorkshire and the Humber")]
    [InlineData(ProjectTeam.NorthWest, "North West")]
    [InlineData(ProjectTeam.EastOfEngland, "East of England")]
    [InlineData(ProjectTeam.WestMidlands, "West Midlands")]
    [InlineData(ProjectTeam.NorthEast, "North East")]
    [InlineData(ProjectTeam.SouthWest, "South West")]
    [InlineData(ProjectTeam.EastMidlands, "East Midlands")]
    [InlineData(ProjectTeam.RegionalCaseWorkerServices, "Regional casework services")]
    [InlineData(ProjectTeam.ServiceSupport, "Service support")]
    [InlineData(ProjectTeam.BusinessSupport, "Business support")]
    [InlineData(ProjectTeam.DataConsumers, "Data consumers")]
    public void Map_ShouldReturnCorrectMapping_ForValidProjectTeam(ProjectTeam projectTeam, string expected)
    {
        // Act
        var result = ProjectTeamPresentationMapper.Map(projectTeam);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Map_ShouldReturnNull_WhenProjectTeamIsNull()
    {
        var result = ProjectTeamPresentationMapper.Map(null);
        Assert.Null(result);
    }

    [Fact]
    public void Map_ShouldThrowArgumentOutOfRangeException_WhenProjectTeamIsInvalid()
    {
        var invalidProjectTeam = (ProjectTeam)999;

        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectTeamPresentationMapper.Map(invalidProjectTeam));
    }
}
