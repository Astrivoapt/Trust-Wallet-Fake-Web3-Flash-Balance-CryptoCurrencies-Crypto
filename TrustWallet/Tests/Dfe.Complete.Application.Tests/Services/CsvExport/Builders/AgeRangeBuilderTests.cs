using AutoFixture;
using Dfe.Complete.Application.Services.CsvExport.Builders;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Tests.Common.Customizations.Models;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public class AgeRangeBuilderTests
    {
        [Theory]
        [InlineData(null, null, "")]
        [InlineData(null, 11, "")]
        [InlineData(4, null, "")]
        [InlineData(4, 11, "4-11")]
        [InlineData(2, 17, "2-17")]
        public void Build_WhenCalled_ShouldReturnNull(int? ageLower, int? ageUpper, string expected)
        {
            GiasEstablishment establishment = new Fixture().Customize(new EstablishmentsCustomization()).Create<GiasEstablishment>();

            establishment.AgeRangeLower = ageLower;
            establishment.AgeRangeUpper = ageUpper;

            var builder = new AgeRangeBuilder<GiasEstablishment>(x => x);

            var result = builder.Build(establishment);

            Assert.Equal(expected, result);
        }
    }
}
