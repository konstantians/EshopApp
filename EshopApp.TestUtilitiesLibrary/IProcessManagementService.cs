namespace EshopApp.GatewayAPI.Tests.Utilities;

public interface IProcessManagementService
{
    void BuildAndRunApplication(bool startAuthService, bool startDataService, bool startEmailService, bool startTransactionService, bool startMvcClient);
    void TerminateApplication();
}