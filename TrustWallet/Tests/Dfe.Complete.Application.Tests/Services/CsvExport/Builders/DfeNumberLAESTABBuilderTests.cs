using Dfe.Complete.Application.Services.CsvExport.Builders;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{

    public class DfeNumberLAESTABBuilderTests
    {
        [Fact]

        public void BuildsCorrectDfeNumber()
        {
            var builder = new DfeNumberLAESTABBuilder();
            var model = ConversionCsvModelFactory.Make();

            var result = builder.Build(model);

            Assert.Equal(model.Academy.LocalAuthorityCode + "/" + model.Academy.EstablishmentNumber, result);
        }

        [Fact]
        public void BuildBlankIfEmpty()
        {
            var builder = new DfeNumberLAESTABBuilder();
            var model = ConversionCsvModelFactory.Make();
            model.Project.AcademyUrn = null;

            var result = builder.Build(model);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void BuildBlankIfAcademyNotFound()
        {
            var builder = new DfeNumberLAESTABBuilder();

            var result = builder.Build(ConversionCsvModelFactory.Make(withAcademy: false));

            Assert.Equal(string.Empty, result);
        }
    }
}
