using Dfe.Complete.Application.Services.CsvExport.Builders;
using Dfe.Complete.Domain.Entities;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public class FormAMatTests
    {
        [Theory]
        [CustomAutoData(typeof(ProjectCustomization))]

        public void WhenIncomingUkprnisNullIsFormAMat(Project project)
        {
            project.IncomingTrustUkprn = null;
                
            var builder = new FormAMat<Project>(x => x);

            var result = builder.Build(project);

            Assert.Equal("form a MAT", result);
        }

        [Theory]
        [CustomAutoData(typeof(ProjectCustomization))]

        public void WhenIncomingUkprnisNotNullIsJoinAMat(Project project)
        {
            var builder = new FormAMat<Project>(x => x);

            var result = builder.Build(project);

            Assert.Equal("join a MAT", result);
        }
    }
}
