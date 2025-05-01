using Dfe.Complete.Application.Services.CsvExport.Builders;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{

    public class BlankIfEmptyTests
    {
        [Fact]
        public void Build_WhenValueIsNull_ReturnsEmptyString()
        {
            var builder = new BlankIfEmpty<BuilderTestModel>(x => x.Value);

            var result = builder.Build(new BuilderTestModel(null));

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Build_WhenValueIsEmpty_ReturnsEmptyString()
        {
            var builder = new BlankIfEmpty<BuilderTestModel>(x => x.Value);

            var result = builder.Build(new BuilderTestModel(string.Empty));

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Build_WhenValueIsNotNullOrEmpty_ReturnsValueToString()
        {
            var builder = new BlankIfEmpty<BuilderTestModel>(x => x.Value);

            var result = builder.Build(new BuilderTestModel("Hello"));

            Assert.Equal("Hello", result);
        }
    }
}
