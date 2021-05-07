using System;
using System.Collections.Generic;
using Billing.API.DopplerSecurity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Billing.API.Test
{
    public static class WebApplicationFactoryHelper
    {
        public static WebApplicationFactory<Startup> Create()
            => new WebApplicationFactory<Startup>();

        public static WebApplicationFactory<Startup> WithBypassAuthorization(this WebApplicationFactory<Startup> factory)
            => factory.WithWebHostBuilder(
                builder => builder.ConfigureTestServices(services =>
                {
                    services
                        .AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                    services
                        .AddOptions<AuthorizationOptions>()
                        .Configure(o =>
                        {
                            o.DefaultPolicy = new AuthorizationPolicyBuilder()
                                .AddAuthenticationSchemes("Test")
                                .RequireAuthenticatedUser()
                                .Build();
                        });
                }));

        public static WebApplicationFactory<Startup> ConfigureService<TOptions>(this WebApplicationFactory<Startup> factory, Action<TOptions> configureOptions) where TOptions : class
            => factory.WithWebHostBuilder(
                builder => builder.ConfigureTestServices(
                    services => services.Configure(configureOptions)));

        public static WebApplicationFactory<Startup> ConfigureSecurityOptions(this WebApplicationFactory<Startup> factory, Action<DopplerSecurityOptions> configureOptions)
            => factory.ConfigureService(configureOptions);

        public static WebApplicationFactory<Startup> WithDisabledLifeTimeValidation(this WebApplicationFactory<Startup> factory)
            => factory.ConfigureSecurityOptions(
                o => o.SkipLifetimeValidation = true);

        public static WebApplicationFactory<Startup> AddConfiguration(this WebApplicationFactory<Startup> factory, IEnumerable<KeyValuePair<string, string>> initialData)
            => factory.WithWebHostBuilder(
                builder => builder.ConfigureAppConfiguration(
                    (builderContext, configurationBuilder) => configurationBuilder.AddInMemoryCollection(initialData)));
    }
}
