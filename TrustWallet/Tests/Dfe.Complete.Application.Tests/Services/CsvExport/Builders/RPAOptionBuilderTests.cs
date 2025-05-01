using Dfe.Complete.Application.Services.CsvExport.Builders;
using Dfe.Complete.Domain.Enums;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public class RPAOptionBuilderTests
    {
        [Theory]
        [InlineData(RiskProtectionArrangementOption.Standard, "standard")]
        [InlineData(RiskProtectionArrangementOption.Commercial, "commercial")]
        [InlineData(RiskProtectionArrangementOption.ChurchOrTrust, "church or trust")]
        [InlineData(null, "standard")]
        public void ReturnsStandardOption(RiskProtectionArrangementOption option, string expected)
        {
            var builder = new RPAOptionBuilder<RPAModel>(x => x.Option);

            var result = builder.Build(new RPAModel(option));

            Assert.Equal(expected, result);
        }
    }
}
