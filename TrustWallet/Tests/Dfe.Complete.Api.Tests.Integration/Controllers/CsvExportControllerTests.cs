using AutoFixture;
using Dfe.Complete.Api.Tests.Integration.Customizations;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Domain.ValueObjects;
using Dfe.Complete.Infrastructure.Database;
using Dfe.Complete.Tests.Common.Customizations.Behaviours;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using DfE.CoreLibs.Testing.AutoFixture.Customizations;
using DfE.CoreLibs.Testing.Mocks.WebApplicationFactory;

namespace Dfe.Complete.Api.Tests.Integration.Controllers
{
    public class CsvExportControllerTests
    {
        [Theory]
        [CustomAutoData(typeof(CustomWebApplicationDbContextFactoryCustomization), typeof(DateOnlyCustomization))]
        public async Task GetConversionCsvByMonthQueryHandler(
            CustomWebApplicationDbContextFactory<Program> factory,
            Complete.Client.Contracts.ICsvExportClient csvExportClient,
            IFixture fixture)
        {
            // Arrange
            var dbContext = factory.GetDbContext<CompleteContext>();

            //Establishments

            var establishment = fixture.Customize(
                new EstablishmentsCustomization()
                ).Create<GiasEstablishment>();

            establishment.Ukprn = new Ukprn(10059853);

            var academy = fixture.Customize(
                new EstablishmentsCustomization()
                ).Create<GiasEstablishment>();

            dbContext.GiasEstablishments.Add(establishment);
            dbContext.GiasEstablishments.Add(academy);

            //local authorities
            var laContact = fixture.Customize(new OmitCircularReferenceCustomization()).Create<Contact>();

            var localAuthority = fixture.Customize(
                new LocalAuthorityCustomization()
                {
                    LocalAuthorityCode = establishment.LocalAuthorityCode
                }
                ).Create<LocalAuthority>();

            laContact.Id = new ContactId(Guid.NewGuid());
            laContact.ProjectId = null;
            laContact.Project = null;
            laContact.LocalAuthorityId = localAuthority.Id;

            dbContext.LocalAuthorities.Add(localAuthority);
            dbContext.Contacts.Add(laContact);

            await dbContext.SaveChangesAsync();
            //tasks
            var conversionTasksData = fixture.Create<ConversionTasksData>();
            dbContext.ConversionTasksData.Add(conversionTasksData);

            //Users
            fixture.Customizations.Add(new IgnoreVirtualMembers());
            var createdBy = fixture.Create<User>();
            var assignedTo = fixture.Create<User>();

            createdBy.Id = new UserId(Guid.NewGuid());
            assignedTo.Id = new UserId(Guid.NewGuid());

            dbContext.Users.Add(createdBy);
            dbContext.Users.Add(assignedTo);

            var mainContact = fixture.Customize(new OmitCircularReferenceCustomization()).Create<Contact>();
            var headteacher = fixture.Customize(new OmitCircularReferenceCustomization()).Create<Contact>();
            
            var incomingContact = fixture.Customize(new OmitCircularReferenceCustomization()).Create<Contact>();
            var outgoingContact = fixture.Customize(new OmitCircularReferenceCustomization()).Create<Contact>();
            var incomingCeoContact = fixture.Customize(new OmitCircularReferenceCustomization()).Create<Contact>();
            var solicitorContact = fixture.Customize(new OmitCircularReferenceCustomization()).Create<Contact>();
            var dioceseContact = fixture.Customize(new OmitCircularReferenceCustomization()).Create<Contact>();
            var directorOfServicesContact = fixture.Customize(new OmitCircularReferenceCustomization()).Create<Contact>();
            var contacts = new List<Contact>
            {
                mainContact,
                headteacher,
                incomingContact,
                outgoingContact,
                incomingCeoContact,
            };

            contacts.ForEach(c => c.Id = new ContactId(Guid.NewGuid()));
            contacts.ForEach(c => c.ProjectId = null);
            
            directorOfServicesContact.Category = ContactCategory.LocalAuthority;
            directorOfServicesContact.LocalAuthorityId = localAuthority.Id;

            dbContext.Contacts.AddRange(contacts);

            await dbContext.SaveChangesAsync();

            var project = fixture.Customize(
                new ProjectCustomization()
                {
                    Id = new ProjectId(Guid.NewGuid()),
                    Urn = establishment.Urn,
                    IncomingTrustUkprn = establishment.Ukprn,
                    AcademyUrn = academy.Urn,
                    RegionalDeliveryOfficerId = createdBy.Id,
                    AssignedToId = assignedTo.Id,
                    MainContactId = mainContact.Id,
                    Type = ProjectType.Conversion,
                    TasksDataType = TaskType.Conversion,
                }
                ).Create<Project>();

            project.Urn = establishment.Urn;
            project.RegionalDeliveryOfficer = createdBy;
            project.AssignedTo = assignedTo;
            project.Caseworker = null;
            project.CaseworkerId = null;
            project.SignificantDate = new DateOnly(2025, 1, 1);
            project.SignificantDateProvisional = false;
            project.TasksDataId = conversionTasksData.Id;
            project.IncomingTrustMainContactId = incomingContact.Id;
            project.OutgoingTrustMainContactId = outgoingContact.Id;
            project.State = ProjectState.Active;
            project.LocalAuthorityId = localAuthority.Id;
            
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var keyContact = fixture.Create<KeyContact>();
            keyContact.HeadteacherId = headteacher.Id;
            keyContact.IncomingTrustCeoId = incomingCeoContact.Id;
            keyContact.ProjectId = project.Id;
            keyContact.Id = new KeyContactId(Guid.NewGuid());
            dbContext.KeyContacts.Add(keyContact);

            var projectContacts = new List<Contact>
            {
                solicitorContact,
                dioceseContact,
            };

            solicitorContact.Category = ContactCategory.Solicitor;
            dioceseContact.Category = ContactCategory.Diocese;

            projectContacts.ForEach(c => c.Id = new ContactId(Guid.NewGuid()));
            projectContacts.ForEach(c => c.ProjectId = project.Id);

            dbContext.Contacts.AddRange(projectContacts);

            var significantDateHistory = fixture.Customize(
                new SignificantDateHistoryCustomization()
                {
                    ProjectId = project.Id
                }
                ).Create<SignificantDateHistory>();

            dbContext.SignificantDateHistories.Add(significantDateHistory);

            await dbContext.SaveChangesAsync();

            // Act
            var results = await csvExportClient.GetConversionCsvByMonthAsync(1, 2025);

            StreamReader reader = new StreamReader(results.Stream);
            string fileContents = await reader.ReadToEndAsync();

            // Assert
            Assert.NotNull(fileContents);

            var lines = fileContents.Split("\n");

            Assert.Equal(3, lines.Length);
        }

        [Theory]
        [CustomAutoData(typeof(CustomWebApplicationDbContextFactoryCustomization), typeof(DateOnlyCustomization))]
        public async Task GetConversionCsvByMonthQueryEmptyData(
           CustomWebApplicationDbContextFactory<Program> factory,
           Complete.Client.Contracts.ICsvExportClient csvExportClient,
           IFixture fixture)
        {
            // Arrange
            var dbContext = factory.GetDbContext<CompleteContext>();

            //Establishments

            var establishment = fixture.Customize(
                new EstablishmentsCustomization()
                ).Create<GiasEstablishment>();

            establishment.Ukprn = new Ukprn(10059853);

            dbContext.GiasEstablishments.Add(establishment);

            //local authorities
            var localAuthority = fixture.Customize(
                new LocalAuthorityCustomization()
                {
                    LocalAuthorityCode = establishment.LocalAuthorityCode
                }
                ).Create<LocalAuthority>();

            dbContext.LocalAuthorities.Add(localAuthority);
 
            await dbContext.SaveChangesAsync();
            //tasks
            var conversionTasksData = fixture.Create<ConversionTasksData>();
            dbContext.ConversionTasksData.Add(conversionTasksData);

            //Users
            fixture.Customizations.Add(new IgnoreVirtualMembers());
            var createdBy = fixture.Create<User>();
            var assignedTo = fixture.Create<User>();

            createdBy.Id = new UserId(Guid.NewGuid());
            assignedTo.Id = new UserId(Guid.NewGuid());

            dbContext.Users.Add(createdBy);
            dbContext.Users.Add(assignedTo);

            await dbContext.SaveChangesAsync();

            var project = fixture.Customize(
                new ProjectCustomization()
                {
                    Id = new ProjectId(Guid.NewGuid()),
                    Urn = establishment.Urn,
                    IncomingTrustUkprn = establishment.Ukprn,
                    RegionalDeliveryOfficerId = createdBy.Id,
                    Type = ProjectType.Conversion,
                    TasksDataType = TaskType.Conversion,
                }
                ).Create<Project>();

            project.Urn = establishment.Urn;
            project.AcademyUrn = null;
            project.RegionalDeliveryOfficer = createdBy;
            project.AssignedToId = null;
            project.AssignedTo = assignedTo;
            project.Caseworker = null;
            project.CaseworkerId = null;
            project.SignificantDate = new DateOnly(2025, 1, 1);
            project.SignificantDateProvisional = false;
            project.TasksDataId = conversionTasksData.Id;
            project.IncomingTrustMainContactId = null;
            project.OutgoingTrustMainContactId = null;
            project.MainContactId = null;
            project.State = ProjectState.Active;
            project.LocalAuthorityId = localAuthority.Id;

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var keyContact = fixture.Create<KeyContact>();
            keyContact.HeadteacherId = null;
            keyContact.IncomingTrustCeoId = null;
            keyContact.ProjectId = project.Id;
            keyContact.Id = new KeyContactId(Guid.NewGuid());
            dbContext.KeyContacts.Add(keyContact);

            await dbContext.SaveChangesAsync();

            // Act
            var results = await csvExportClient.GetConversionCsvByMonthAsync(1, 2025);

            Assert.NotNull(results);

            StreamReader reader = new StreamReader(results.Stream);
            string fileContents = await reader.ReadToEndAsync();

            // Assert
            Assert.NotNull(fileContents);

            var lines = fileContents.Split("\n");

            Assert.Equal(3, lines.Length);
        }
    }
}
