namespace Billing.API.Services.Invoice
{
    public interface ISapServiceSettingsService
    {
        string GetSapSchema(string billingSystem);
    }
}
