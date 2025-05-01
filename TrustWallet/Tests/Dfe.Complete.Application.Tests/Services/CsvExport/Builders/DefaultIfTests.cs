using Dfe.Complete.Application.Services.CsvExport.Builders;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public class DefaultIfTests
    {
        private const string DefaultValue = "Default";

        [Fact]
        public void Build_WhenValueIsNull_ReturnsDefault()
        {
            var builder = new DefaultIf<ConditionalTestModel>(x => x.Condition, x => x.Value, DefaultValue);

            var result = builder.Build(new ConditionalTestModel("Hello", true));

            Assert.Equal(DefaultValue, result);
        }

        [Fact]
        public void Build_WhenValueIsNotNullOrEmpty_ReturnsValueToString()
        {
            var builder = new DefaultIf<ConditionalTestModel>(x => x.Condition, x => x.Value, DefaultValue);

            var result = builder.Build(new ConditionalTestModel("Hello", false));

            Assert.Equal("Hello", result);
        }
    }
}
