using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using AutoMapper;
using Dfe.Complete.Application.Common.Mappers;
using Dfe.Complete.Domain.Entities;

namespace Dfe.Complete.Application.Tests.Mappers
{
    public class ProjectGroupMapperTests
    {
        private readonly IMapper _mapper;

        public ProjectGroupMapperTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapping>();
            });

            config.AssertConfigurationIsValid(); // Ensures all mappings are valid

            _mapper = config.CreateMapper();
        }

        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization))]
        public async Task Map_ProjectGroupToProjectGroupDto_ShouldMapAllPropertiesCorrectly(Domain.Entities.ProjectGroup projectGroup)
        {
            // Act
            var projectGroupDto = _mapper.Map<ProjectGroup>(projectGroup);

            // Assert
            Assert.NotNull(projectGroupDto);
            Assert.Equal(projectGroup.Id, projectGroupDto.Id);
            Assert.Equal(projectGroup.GroupIdentifier, projectGroupDto.GroupIdentifier);
            Assert.Equal(projectGroup.TrustUkprn, projectGroupDto.TrustUkprn);
            Assert.Equal(projectGroup.CreatedAt, projectGroupDto.CreatedAt);
            Assert.Equal(projectGroup.UpdatedAt, projectGroupDto.UpdatedAt);

        }
    }
}
