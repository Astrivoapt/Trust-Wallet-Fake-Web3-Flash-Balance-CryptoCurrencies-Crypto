namespace Dfe.Complete.Api.Client.Security
{
    public interface ITokenAcquisitionService
    {
        Task<string> GetTokenAsync();
    }
}
