using Dfe.AcademiesApi.Client.Contracts;
using Dfe.Complete.Application.Services.CsvExport.Builders;
using Dfe.Complete.Application.Services.TrustCache;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using NSubstitute;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{

    public class TrustDataBuilderTests
    {

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization), typeof(ProjectCustomization))]
        public void ShouldBeAbleToGetFromTrust(TrustDto trust, Project project)
        {
            project.IncomingTrustUkprn = trust.Ukprn;
            var TrustCache = Substitute.For<ITrustCache>();
            TrustCache.GetTrustAsync(trust.Ukprn).Returns(trust);
            var TrustData = new IncomingTrustDataBuilder<Project>(TrustCache, u => u, t => t.CompaniesHouseNumber);

            var result = TrustData.Build(project);

            Assert.Equal(trust.CompaniesHouseNumber, result);
        }

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization), typeof(ProjectCustomization))]
        public void ShouldBeBlankIfTrustNotFound(Project project)
        {
            var TrustCache = Substitute.For<ITrustCache>();

            var TrustData = new IncomingTrustDataBuilder<Project>(TrustCache, u => u, t => t.CompaniesHouseNumber);

            var result = TrustData.Build(project);

            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization), typeof(ProjectCustomization))]
        public void ShouldBeAbleToGetFromTRN(TrustDto trust, Project project)
        {
            project.IncomingTrustUkprn = null;
            project.NewTrustReferenceNumber = trust.ReferenceNumber;
            var TrustCache = Substitute.For<ITrustCache>();
            TrustCache.GetTrustByTrnAsync(trust.ReferenceNumber).Returns(trust);
            var TrustData = new IncomingTrustDataBuilder<Project>(TrustCache, u => u, t => t.CompaniesHouseNumber);

            var result = TrustData.Build(project);

            Assert.Equal(trust.CompaniesHouseNumber, result);
        }

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization), typeof(ProjectCustomization))]
        public void ShouldBeBlankIfTRNNotFound(Project project)
        {
            project.IncomingTrustUkprn = null;

            var TrustCache = Substitute.For<ITrustCache>();

            var TrustData = new IncomingTrustDataBuilder<Project>(TrustCache, u => u, t => t.CompaniesHouseNumber);

            var result = TrustData.Build(project);

            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization), typeof(ProjectCustomization))]
        public void ShouldBeBlankIfSelectedValueIsNull(Project project)
        {
            var TrustCache = Substitute.For<ITrustCache>();

            var TrustData = new IncomingTrustDataBuilder<Project>(TrustCache, u => u, t => null);

            var result = TrustData.Build(project);

            Assert.Equal(string.Empty, result);
        }
    }
}
