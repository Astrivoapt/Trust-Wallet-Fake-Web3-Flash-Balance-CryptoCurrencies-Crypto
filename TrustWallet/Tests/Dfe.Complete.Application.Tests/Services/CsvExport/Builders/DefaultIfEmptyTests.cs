using Dfe.Complete.Application.Services.CsvExport.Builders;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public class DefaultIfEmptyTests
    {
        private const string DefaultValue = "Default";

        [Fact]
        public void Build_WhenValueIsNull_ReturnsDefault()
        {
            var builder = new DefaultIfEmpty<BuilderTestModel>(x => x.Value, DefaultValue);

            var result = builder.Build(new BuilderTestModel(null));

            Assert.Equal(DefaultValue, result);
        }

        [Fact]
        public void Build_WhenValueIsEmpty_ReturnsDefault()
        {
            var builder = new DefaultIfEmpty<BuilderTestModel>(x => x.Value, DefaultValue);

            var result = builder.Build(new BuilderTestModel(string.Empty));

            Assert.Equal(DefaultValue, result);
        }

        [Fact]
        public void Build_WhenValueIsNotNullOrEmpty_ReturnsValueToString()
        {
            var builder = new DefaultIfEmpty<BuilderTestModel>(x => x.Value, DefaultValue);

            var result = builder.Build(new BuilderTestModel("Hello"));

            Assert.Equal("Hello", result);
        }
    }
}
