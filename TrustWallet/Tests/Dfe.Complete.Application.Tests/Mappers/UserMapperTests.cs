using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using AutoMapper;
using Dfe.Complete.Application.Common.Mappers;
using Dfe.Complete.Application.Projects.Models;
using Dfe.Complete.Tests.Common.Customizations.Behaviours;

namespace Dfe.Complete.Application.Tests.Mappers
{
    public class UserMapperTests
    {
        private readonly IMapper _mapper;

        public UserMapperTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapping>();
            });

            _mapper = config.CreateMapper();
        }

        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization), typeof(IgnoreVirtualMembersCustomisation))]
        public async Task Map_UserToUserDto_ShouldMapAllPropertiesCorrectly(Domain.Entities.User user)
        {
            // Act
            var userDto = _mapper.Map<UserDto>(user);

            // Assert
            Assert.NotNull(userDto);
            Assert.Equal(user.Id, userDto.Id);
            Assert.Equal(user.Email, userDto.Email);
            Assert.Equal(user.CreatedAt, userDto.CreatedAt);
            Assert.Equal(user.UpdatedAt, userDto.UpdatedAt);
            Assert.Equal(user.ManageTeam, userDto.ManageTeam);
            Assert.Equal(user.AddNewProject, userDto.AddNewProject);
            Assert.Equal(user.FirstName, userDto.FirstName);
            Assert.Equal(user.LastName, userDto.LastName);
            Assert.Equal(user.ActiveDirectoryUserId, userDto.ActiveDirectoryUserId);
            Assert.Equal(user.AssignToProject, userDto.AssignToProject);
            Assert.Equal(user.ManageUserAccounts, userDto.ManageUserAccounts);
            Assert.Equal(user.ActiveDirectoryUserGroupIds, userDto.ActiveDirectoryUserGroupIds);
            Assert.Equal(user.Team, userDto.Team);
            Assert.Equal(user.DeactivatedAt, userDto.DeactivatedAt);
            Assert.Equal(user.ManageConversionUrns, userDto.ManageConversionUrns);
            Assert.Equal(user.ManageLocalAuthorities, userDto.ManageLocalAuthorities);
            Assert.Equal(user.LatestSession, userDto.LatestSession);
        }
    }
}
