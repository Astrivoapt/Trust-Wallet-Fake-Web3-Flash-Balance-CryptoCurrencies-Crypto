using Dfe.Complete.Application.Services.CsvExport.Builders;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public class ProvisionalDateBuilderTests
    {
        [Fact]
        public void IfHistoryDoesNotExistUseDateOnProject()
        {
            var model = ConversionCsvModelFactory.Make(withSignificantDateHistory: false);

            var builder = new ProvisionalDateBuilder();

            var result = builder.Build(model);

            Assert.Equal(model.Project.SignificantDate?.ToString("yyyy-MM-dd"), result);
        }

        [Fact]
        public void IfHistoryExistsUseDateOnHistory()
        {
            var model = ConversionCsvModelFactory.Make();

            var builder = new ProvisionalDateBuilder();

            var result = builder.Build(model);

            Assert.Equal(model.SignificantDateHistory?.PreviousDate.Value.ToString("yyyy-MM-dd"), result);
        }
    }
}
