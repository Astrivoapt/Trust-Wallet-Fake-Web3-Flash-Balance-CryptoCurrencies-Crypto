using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using Dfe.Complete.Tests.Common.Customizations.Behaviours;
using Dfe.Complete.Tests.Common.Customizations.Models;
using Dfe.Complete.Application.Projects.Models;
using AutoMapper;
using Dfe.Complete.Application.Common.Mappers;

namespace Dfe.Complete.Application.Tests.Mappers
{
    public class ProjectMapperTests
    {
        private readonly IMapper _mapper;

        public ProjectMapperTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapping>();
            });

            config.AssertConfigurationIsValid(); // Ensures all mappings are valid

            _mapper = config.CreateMapper();
        }

        [Theory]
        [CustomAutoData(typeof(DateOnlyCustomization), typeof(ProjectCustomization), typeof(IgnoreVirtualMembersCustomisation))]
        public async Task Map_ProjectToProjectDto_ShouldMapAllPropertiesCorrectly(Domain.Entities.Project project)
        {
            // Act
            var projectDto = _mapper.Map<ProjectDto>(project);

            // Assert
            Assert.NotNull(projectDto);
            Assert.Equal(project.Id, projectDto.Id);
            Assert.Equal(project.Urn, projectDto.Urn);
            Assert.Equal(project.CreatedAt, projectDto.CreatedAt);
            Assert.Equal(project.UpdatedAt, projectDto.UpdatedAt);
            Assert.Equal(project.IncomingTrustUkprn, projectDto.IncomingTrustUkprn);
            Assert.Equal(project.RegionalDeliveryOfficerId, projectDto.RegionalDeliveryOfficerId);
            Assert.Equal(project.CaseworkerId, projectDto.CaseworkerId);
            Assert.Equal(project.AssignedAt, projectDto.AssignedAt);
            Assert.Equal(project.AdvisoryBoardDate, projectDto.AdvisoryBoardDate);
            Assert.Equal(project.AdvisoryBoardConditions, projectDto.AdvisoryBoardConditions);
            Assert.Equal(project.EstablishmentSharepointLink, projectDto.EstablishmentSharepointLink);
            Assert.Equal(project.CompletedAt, projectDto.CompletedAt);
            Assert.Equal(project.IncomingTrustSharepointLink, projectDto.IncomingTrustSharepointLink);
            Assert.Equal(project.Type, projectDto.Type);
            Assert.Equal(project.AssignedToId, projectDto.AssignedToId);
            Assert.Equal(project.SignificantDate, projectDto.SignificantDate);
            Assert.Equal(project.SignificantDateProvisional, projectDto.SignificantDateProvisional);
            Assert.Equal(project.DirectiveAcademyOrder, projectDto.DirectiveAcademyOrder);
            Assert.Equal(project.Region, projectDto.Region);
            Assert.Equal(project.AcademyUrn, projectDto.AcademyUrn);
            Assert.Equal(project.TasksDataId, projectDto.TasksDataId);
            Assert.Equal(project.TasksDataType, projectDto.TasksDataType);
            Assert.Equal(project.OutgoingTrustUkprn, projectDto.OutgoingTrustUkprn);
            Assert.Equal(project.Team, projectDto.Team);
            Assert.Equal(project.TwoRequiresImprovement, projectDto.TwoRequiresImprovement);
            Assert.Equal(project.OutgoingTrustSharepointLink, projectDto.OutgoingTrustSharepointLink);
            Assert.Equal(project.AllConditionsMet, projectDto.AllConditionsMet);
            Assert.Equal(project.MainContactId, projectDto.MainContactId);
            Assert.Equal(project.EstablishmentMainContactId, projectDto.EstablishmentMainContactId);
            Assert.Equal(project.IncomingTrustMainContactId, projectDto.IncomingTrustMainContactId);
            Assert.Equal(project.OutgoingTrustMainContactId, projectDto.OutgoingTrustMainContactId);
            Assert.Equal(project.NewTrustReferenceNumber, projectDto.NewTrustReferenceNumber);
            Assert.Equal(project.NewTrustName, projectDto.NewTrustName);
            Assert.Equal(project.State, projectDto.State);
            Assert.Equal(project.PrepareId, projectDto.PrepareId);
            Assert.Equal(project.LocalAuthorityMainContactId, projectDto.LocalAuthorityMainContactId);
            Assert.Equal(project.GroupId, projectDto.GroupId);
            Assert.Equal(project.AssignedTo?.Id, projectDto.AssignedTo?.Id);
            Assert.Equal(project.Caseworker?.Id, projectDto.Caseworker?.Id);
            Assert.Equal(project.RegionalDeliveryOfficer?.Id, projectDto.RegionalDeliveryOfficer?.Id);

            // Validate collections
            Assert.Equal(project.Contacts.Select(c => c.Id), projectDto.Contacts.Select(c => c.Id));
            Assert.Equal(project.Notes.Select(n => n.Id), projectDto.Notes.Select(n => n.Id));
        }
    }
}
