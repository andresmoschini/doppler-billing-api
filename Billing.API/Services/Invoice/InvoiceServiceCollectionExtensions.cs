using Billing.API.Services.Invoice;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InvoiceServiceCollectionExtensions
    {
        public static IServiceCollection AddInvoiceService(this IServiceCollection services)
        {
            services.ConfigureOptions<ConfigureInvoiceProviderOptions>();

            services.AddSingleton<DummyInvoiceService>();
            services.AddTransient<InvoiceService>();
            services.AddTransient<ISapServiceSettingsService, SapServiceSettingsService>();

            services.AddTransient(serviceProvider =>
            {
                var invoiceProviderOptions = serviceProvider.GetRequiredService<IOptions<InvoiceProviderOptions>>();

                return invoiceProviderOptions.Value.UseDummyData
                    ? (IInvoiceService)serviceProvider.GetRequiredService<DummyInvoiceService>()
                    : serviceProvider.GetRequiredService<InvoiceService>();
            });

            return services;
        }
    }
}
