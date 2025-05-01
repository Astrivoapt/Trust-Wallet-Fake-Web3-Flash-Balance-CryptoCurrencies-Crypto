using Dfe.Complete.Domain.Enums;

namespace Dfe.Complete.Application.Tests.Services.CsvExport.Builders
{
    public record BuilderTestModel(string? Value);
    public record ConditionalTestModel(string? Value, bool Condition);
    public record RPAModel(RiskProtectionArrangementOption Option);
}
