using Dfe.Complete.Application.Services.CsvExport.Builders;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public class BoolBuilderTests
    {
        [Fact]
        public void ReturnsYesValueWhenTrue()
        {
            var builder = new BoolBuilder<ConditionalTestModel>(x => x.Condition, "Yes", "No");

            var result = builder.Build(new ConditionalTestModel(null, true));

            Assert.Equal("Yes", result);
        }

        [Fact]
        public void ReturnsNoValueWhenFalse()
        {
            var builder = new BoolBuilder<ConditionalTestModel>(x => x.Condition, "Yes", "No");

            var result = builder.Build(new ConditionalTestModel(null, false));

            Assert.Equal("No", result);
        }

        [Fact]
        public void ReturnsNoValueWhenNull()
        {
            var builder = new BoolBuilder<ConditionalTestModel>(x => null, "Yes", "No");

            var result = builder.Build(new ConditionalTestModel(null, true));

            Assert.Equal("No", result);
        }
    }
}
