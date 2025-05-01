using Dfe.Complete.Application.Services.CsvExport;

namespace Dfe.Complete.Application.Tests.Services.CsvExport
{
    internal record TestModel(string SchoolName, int SchoolUrn, string ProjectType, string AcademyName, int AcademyUrn);

    internal class TestHeaderGenerator : IHeaderGenerator<TestModel>
    {
        public string GenerateHeader()
        {
            return "School name,School URN,Project type,Academy name,Academy URN";
        }
    }

    internal class TestRowGenerator : IRowGenerator<TestModel>
    {
        public string GenerateRow(TestModel model)
        {
            return $"{model.SchoolName},{model.SchoolUrn},{model.ProjectType},{model.AcademyName},{model.AcademyUrn}";
        }
    }

    public class CSVFileContentGeneratorTests
    {
        [Fact]
        public void Generate_ShouldReturnCsvString_WhenModelIsProvided()
        {
            var model = new List<TestModel>
            {
                new TestModel("Round Hill Primary School", 122707, "Conversion", "Round Hill Primary School", 150250)
            };

            var generator = new CSVFileContentGenerator<TestModel>(new TestHeaderGenerator(), new TestRowGenerator());

            var result = generator.Generate(model);

            Assert.Equal("School name,School URN,Project type,Academy name,Academy URN\nRound Hill Primary School,122707,Conversion,Round Hill Primary School,150250\n", result);
        }

        [Fact]
        public void Generate_ShouldReturnCsvString_WhenMultipleModelsAreProvided()
        {
            var models = new List<TestModel>
            {
                new TestModel("Round Hill Primary School", 122707, "Conversion", "Primary School", 150250),
                new TestModel("Hill Primary School", 122708, "Transfer", "Hill Primary School", 150251),
                new TestModel("Primary School", 122709, "Conversion", "Round Hill Primary School", 150252)
            };

            var generator = new CSVFileContentGenerator<TestModel>(new TestHeaderGenerator(), new TestRowGenerator());

            var result = generator.Generate(models);

            var expected = string.Join("\n", models.Select(m => $"{m.SchoolName},{m.SchoolUrn},{m.ProjectType},{m.AcademyName},{m.AcademyUrn}"));

            Assert.Equal($"School name,School URN,Project type,Academy name,Academy URN\n{expected}\n", result);
        }
    }
}
