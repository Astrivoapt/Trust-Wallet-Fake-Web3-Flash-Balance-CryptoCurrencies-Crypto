using Dfe.Complete.Application.Services.CsvExport.Builders;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Domain.Enums;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public class AcademyOrderTypeBuilderTests
    {
        [Theory]
        [CustomAutoData(typeof(ProjectCustomization))]
        public void Build_When_Transfer(Project project)
        {
            project.Type = ProjectType.Transfer;
            var builder = new AcademyOrderTypeBuilder<Project>((project) => project);

            var result = builder.Build(project);

            Assert.Equal("not applicable", result);
        }

        [Theory]
        [CustomAutoData(typeof(ProjectCustomization))]
        public void Build_When_Conversion_And_Directive(Project project)
        {
            project.Type = ProjectType.Conversion;
            project.DirectiveAcademyOrder = true;

            var builder = new AcademyOrderTypeBuilder<Project>((project) => project);

            var result = builder.Build(project);

            Assert.Equal("directive academy order", result);
        }

        [Theory]
        [CustomAutoData(typeof(ProjectCustomization))]
        public void Build_When_Conversion_And_Not_Directive(Project project)
        {
            project.Type = ProjectType.Conversion;
            project.DirectiveAcademyOrder = false;

            var builder = new AcademyOrderTypeBuilder<Project>((project) => project);

            var result = builder.Build(project);

            Assert.Equal("academy order", result);
        }
    }
}
