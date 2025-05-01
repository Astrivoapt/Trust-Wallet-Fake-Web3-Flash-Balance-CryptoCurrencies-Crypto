using Dfe.Complete.Application.Services.CsvExport.Builders;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public class ProjectTypeTests
    {
        [Theory]
        [CustomAutoData(typeof(ProjectCustomization))]
        public void Build_When_Conversion(Project project)
        {
            
            var builder = new ProjectTypeBuilder();
            
            var model = ConversionCsvModelFactory.Make();
            model.Project.Type = ProjectType.Conversion;

            var result = builder.Build(model);

            Assert.Equal("Conversion", result);
        }

        [Theory]
        [CustomAutoData(typeof(ProjectCustomization))]
        public void Build_When_Transfer(Project project)
        {   
            var builder = new ProjectTypeBuilder();
            var model = ConversionCsvModelFactory.Make();
            model.Project.Type = ProjectType.Transfer;

            var result = builder.Build(model);

            Assert.Equal("Transfer", result);
        }
    }
}
