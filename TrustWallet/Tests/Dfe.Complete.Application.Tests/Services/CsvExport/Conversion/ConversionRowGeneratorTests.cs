using Dfe.AcademiesApi.Client.Contracts;
using Dfe.Complete.Application.Common.Models;
using Dfe.Complete.Application.Services.CsvExport.Builders;
using Dfe.Complete.Application.Services.CsvExport.Conversion;
using Dfe.Complete.Application.Services.TrustCache;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using NSubstitute;
namespace Dfe.Complete.Application.Tests.Services.CsvExport.Conversion
{
    public class ConversionRowGeneratorTests
    {
        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization))]
        public void RowGeneratesAccountsForBlankData(TrustDto incomingTrust)
        {
            var model = ConversionCsvModelFactory.Make(withAcademy: false,
                                                       withSignificantDateHistory: false,
                                                       withContacts: false);

            model.Project.Type = ProjectType.Conversion;
            model.Project.AcademyUrn = null;
            model.Project.IncomingTrustUkprn = null;
            model.Project.SignificantDateProvisional = true;
            model.Project.DirectiveAcademyOrder = true;
            model.Project.TwoRequiresImprovement = true;
            model.Project.AdvisoryBoardConditions = null;
            model.Project.AllConditionsMet = false;
            model.Project.EstablishmentSharepointLink = null;
            model.Project.Team = ProjectTeam.SouthWest;

            model.CurrentSchool.PhaseName = "Not applicable";
            model.CurrentSchool.AddressStreet = null;
            model.CurrentSchool.AddressLocality = null;
            model.CurrentSchool.AddressAdditional = null;
            model.CurrentSchool.AddressTown = null;
            model.CurrentSchool.AddressCounty = null;
            model.CurrentSchool.AddressPostcode = null;

            model.ConversionTasks.ReceiveGrantPaymentCertificateDateReceived = null;
            model.ConversionTasks.ProposedCapacityOfTheAcademyReceptionToSixYears = null;
            model.ConversionTasks.ProposedCapacityOfTheAcademySevenToElevenYears = null;
            model.ConversionTasks.ProposedCapacityOfTheAcademyTwelveOrAboveYears = null;

            var TrustCache = Substitute.For<ITrustCache>();
            TrustCache.GetTrustByTrnAsync(model.Project.NewTrustReferenceNumber).Returns(incomingTrust);

            var generator = new ConversionRowGenerator(new RowBuilderFactory<ConversionCsvModel>(TrustCache));

            generator.GenerateRow(model);

            var result = generator.GenerateRow(model).Split(",");

            Assert.Equal(model.CurrentSchool.Name, result[0]);
            Assert.Equal(model.Project.Urn.Value.ToString(), result[1]);
            Assert.Equal("Conversion", result[2]);
            Assert.Equal("unconfirmed", result[3]);
            Assert.Equal("unconfirmed", result[4]);
            Assert.Equal("", result[5]);
            Assert.Equal(incomingTrust.Name, result[6]);
            Assert.Equal(model.LocalAuthority.Name, result[7]);
            Assert.Equal(model.CurrentSchool.RegionName, result[8]);
            Assert.Equal(model.CurrentSchool.DioceseName, result[9]);
            Assert.Equal(model.Project.SignificantDate.Value.ToString("yyyy-MM-dd"), result[10]);
            Assert.Equal("unconfirmed", result[11]); 
            Assert.Equal("directive academy order", result[12]);
            Assert.Equal("yes", result[13]);
            Assert.Equal(model.Project.AdvisoryBoardDate.Value.ToString("yyyy-MM-dd"), result[14]);
            Assert.Equal("", result[15]);
            Assert.Equal("standard", result[16]);
            Assert.Equal("not applicable", result[17]);
            Assert.Equal("no", result[18]);
            Assert.Equal("unconfirmed", result[19]);
            Assert.Equal(model.CurrentSchool.TypeName, result[20]);
            Assert.Equal(model.CurrentSchool.AgeRangeLower + "-" + model.CurrentSchool.AgeRangeUpper, result[21]);
            Assert.Equal(model.CurrentSchool.TypeName, result[22]);
            Assert.Equal("", result[23]);
            Assert.Equal("", result[24]);
            Assert.Equal("", result[25]);
            Assert.Equal("", result[26]);
            Assert.Equal("", result[27]);
            Assert.Equal("", result[28]);
            Assert.Equal("", result[29]);
            Assert.Equal("", result[30]);
            Assert.Equal("", result[31]);
            Assert.Equal("", result[32]);
            Assert.Equal("form a MAT", result[33]);
            Assert.Equal(incomingTrust.Ukprn.ToString(), result[34]);
            Assert.Equal(incomingTrust.ReferenceNumber, result[35]);
            Assert.Equal(incomingTrust.CompaniesHouseNumber, result[36]);
            Assert.Equal(incomingTrust.Address.Street, result[37]);
            Assert.Equal(incomingTrust.Address.Locality, result[38]);
            Assert.Equal(incomingTrust.Address.Additional, result[39]);
            Assert.Equal(incomingTrust.Address.Town, result[40]);
            Assert.Equal(incomingTrust.Address.County, result[41]);
            Assert.Equal(incomingTrust.Address.Postcode, result[42]);
            Assert.Equal(model.Project.IncomingTrustSharepointLink, result[43]);
            Assert.Equal($"{model.CreatedBy.FirstName} {model.CreatedBy.LastName}", result[44]);
            Assert.Equal(model.CreatedBy.Email, result[45]);
            Assert.Equal($"{model.AssignedTo.FirstName} {model.AssignedTo.LastName}", result[46]);
            Assert.Equal("South West", result[47]);
            Assert.Equal("", result[48]);
            Assert.Equal("", result[49]);
            Assert.Equal("", result[50]);
            Assert.Equal("", result[51]);
            Assert.Equal("", result[52]);
            Assert.Equal("", result[53]);
            Assert.Equal("", result[54]);
            Assert.Equal("", result[55]);
            Assert.Equal("", result[56]);
            Assert.Equal("", result[57]);
            Assert.Equal("", result[58]);
            Assert.Equal("", result[59]);
            Assert.Equal("", result[60]);
            Assert.Equal("", result[61]);
            Assert.Equal("", result[62]);
            Assert.Equal("", result[63]);
            Assert.Equal("", result[64]);
            Assert.Equal("", result[65]);
            Assert.Equal("", result[66]);
            Assert.Equal("", result[67]);
        }

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization))]
        public void RowGeneratesBasedOnModel(TrustDto incomingTrust)
        {
            var model = ConversionCsvModelFactory.Make();

            model.Project.Type = ProjectType.Conversion;
            model.Project.IncomingTrustUkprn = incomingTrust.Ukprn;
            model.Project.SignificantDateProvisional = false;
            model.Project.TwoRequiresImprovement = false;
            model.Project.DirectiveAcademyOrder = false;
            model.Project.AllConditionsMet = true;
            model.Project.Team = ProjectTeam.RegionalCaseWorkerServices;

            model.ConversionTasks.RiskProtectionArrangementOption = RiskProtectionArrangementOption.Commercial;

            var TrustCache = Substitute.For<ITrustCache>();
            TrustCache.GetTrustAsync(incomingTrust.Ukprn).Returns(incomingTrust);

            var generator = new ConversionRowGenerator(new RowBuilderFactory<ConversionCsvModel>(TrustCache));

            generator.GenerateRow(model);

            var result = generator.GenerateRow(model).Split(",");

            Assert.Equal(model.CurrentSchool.Name, result[0]);
            Assert.Equal(model.Project.Urn.Value.ToString(), result[1]);
            Assert.Equal("Conversion", result[2]);
            Assert.Equal(model.Academy.Name, result[3]);
            Assert.Equal(model.Academy.Urn.Value.ToString(), result[4]);
            Assert.Equal(model.Academy.LocalAuthorityCode + "/" + model.Academy.EstablishmentNumber, result[5]);
            Assert.Equal(incomingTrust.Name, result[6]);
            Assert.Equal(model.LocalAuthority.Name, result[7]);
            Assert.Equal(model.CurrentSchool.RegionName, result[8]);
            Assert.Equal(model.CurrentSchool.DioceseName, result[9]);
            Assert.Equal(model.SignificantDateHistory.PreviousDate.Value.ToString("yyyy-MM-dd"), result[10]);
            Assert.Equal(model.Project.SignificantDate.Value.ToString("yyyy-MM-dd"), result[11]);
            Assert.Equal("academy order", result[12]);
            Assert.Equal("no", result[13]);
            Assert.Equal(model.Project.AdvisoryBoardDate.Value.ToString("yyyy-MM-dd"), result[14]);
            Assert.Equal(model.Project.AdvisoryBoardConditions, result[15]);
            Assert.Equal("commercial", result[16]);
            Assert.Equal(model.ConversionTasks.RiskProtectionArrangementReason, result[17]);
            Assert.Equal("yes", result[18]);
            Assert.Equal(model.ConversionTasks.ReceiveGrantPaymentCertificateDateReceived?.ToString("yyyy-MM-dd"), result[19]);
            Assert.Equal(model.CurrentSchool.TypeName, result[20]);
            Assert.Equal(model.CurrentSchool.AgeRangeLower + "-" + model.CurrentSchool.AgeRangeUpper, result[21]);
            Assert.Equal(model.CurrentSchool.PhaseName, result[22]);
            Assert.Equal(model.ConversionTasks.ProposedCapacityOfTheAcademyReceptionToSixYears, result[23]);
            Assert.Equal(model.ConversionTasks.ProposedCapacityOfTheAcademySevenToElevenYears, result[24]);
            Assert.Equal(model.ConversionTasks.ProposedCapacityOfTheAcademyTwelveOrAboveYears, result[25]);
            Assert.Equal(model.CurrentSchool.AddressStreet, result[26]);
            Assert.Equal(model.CurrentSchool.AddressLocality, result[27]);
            Assert.Equal(model.CurrentSchool.AddressAdditional, result[28]);
            Assert.Equal(model.CurrentSchool.AddressTown, result[29]);
            Assert.Equal(model.CurrentSchool.AddressCounty, result[30]);
            Assert.Equal(model.CurrentSchool.AddressPostcode, result[31]);
            Assert.Equal(model.Project.EstablishmentSharepointLink, result[32]);
            Assert.Equal("join a MAT", result[33]);
            Assert.Equal(incomingTrust.Ukprn.ToString(), result[34]);
            Assert.Equal(incomingTrust.ReferenceNumber.ToString(), result[35]);
            Assert.Equal(incomingTrust.CompaniesHouseNumber, result[36]);
            Assert.Equal(incomingTrust.Address.Street, result[37]);
            Assert.Equal(incomingTrust.Address.Locality, result[38]);
            Assert.Equal(incomingTrust.Address.Additional, result[39]);
            Assert.Equal(incomingTrust.Address.Town, result[40]);
            Assert.Equal(incomingTrust.Address.County, result[41]);
            Assert.Equal(incomingTrust.Address.Postcode, result[42]);
            Assert.Equal(model.Project.IncomingTrustSharepointLink, result[43]);
            Assert.Equal($"{model.CreatedBy.FirstName} {model.CreatedBy.LastName}", result[44]);
            Assert.Equal(model.CreatedBy.Email, result[45]);
            Assert.Equal($"{model.AssignedTo.FirstName} {model.AssignedTo.LastName}", result[46]);
            Assert.Equal("Regional casework services", result[47]);
            Assert.Equal(model.MainContact.Name, result[48]);
            Assert.Equal(model.Headteacher.Name, result[49]);
            Assert.Equal("Headteacher", result[50]);
            Assert.Equal(model.Headteacher.Email, result[51]);
            Assert.Equal(model.LocalAuthorityContact.Name, result[52]);
            Assert.Equal(model.LocalAuthorityContact.Email, result[53]);
            Assert.Equal(model.IncomingContact.Name, result[54]);
            Assert.Equal(model.IncomingContact.Email, result[55]);
            Assert.Equal(model.OutgoingContact.Name, result[56]);
            Assert.Equal(model.OutgoingContact.Email, result[57]);
            Assert.Equal(model.IncomingCEOContact.Name, result[58]);
            Assert.Equal("CEO", result[59]);
            Assert.Equal(model.IncomingCEOContact.Email, result[60]);
            Assert.Equal(model.SolicitorContact.Name, result[61]);
            Assert.Equal(model.SolicitorContact.Email, result[62]);
            Assert.Equal(model.DioceseContact.Name, result[63]);
            Assert.Equal(model.DioceseContact.Email, result[64]);
            Assert.Equal(model.DirectorOfServicesContact.Name, result[65]);
            Assert.Equal(model.DirectorOfServicesContact.Email, result[66]);
            Assert.Equal(model.DirectorOfServicesContact.Title, result[67]);
        }

        [Fact]
        public void HeaderIsCorrectlyGenerated()
        {
            var expected = "School name,School URN,Project type,Academy name,Academy URN,Academy DfE number/LAESTAB,Incoming trust name,Local authority,Region,Diocese,Provisional conversion date,Confirmed conversion date,Academy order type,2RI (Two Requires Improvement),Advisory board date,Advisory board conditions,Risk protection arrangement,Reason for commercial insurance,All conditions met,Completed grant payment certificate received,School type,School age range,School phase,Proposed capacity for pupils in reception to year 6,Proposed capacity for pupils in years 7 to 11,Proposed capacity for students in year 12 or above,School address 1,School address 2,School address 3,School town,School county,School postcode,School sharepoint folder,Conversion type,Incoming trust UKPRN,Incoming trust group identifier,Incoming trust companies house number,Incoming trust address 1,Incoming trust address 2,Incoming trust address 3,Incoming trust address town,Incoming trust address county,Incoming trust address postcode,Incoming trust sharepoint link,Project created by name,Project created by email address,Assigned to name,Team managing the project,Project main contact name,Headteacher name,Headteacher role,Headteacher email,Local authority contact name,Local authority contact email,Primary contact for incoming trust name,Primary contact for incoming trust email,Primary contact for outgoing trust name,Primary contact for outgoing trust email,Incoming trust CEO name,Incoming trust CEO role,Incoming trust CEO email,Solicitor contact name,Solicitor contact email,Diocese contact name,Diocese contact email,Director of child services name,Director of child services email,Director of child services role";
            var generator = new ConversionRowGenerator(new RowBuilderFactory<ConversionCsvModel>(null));
            
            var result = generator.GenerateHeader();

            Assert.Equal(expected, result);
        }
    }
}
