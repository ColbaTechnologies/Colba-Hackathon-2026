#nullable enable
using System.Security.Cryptography.X509Certificates;
using MessagingSystem.Application.Configuration;
using Microsoft.Extensions.Configuration;

namespace MessagingSystem.Infrastructure.Configuration;

public record RavenSettings(string[] Urls, string? DatabaseName);

/// <summary>
/// 
/// </summary>
public static class ConfigurationExtensions
{
    public static RavenSettings? GetRavenDbSettings(this IConfiguration configuration, string? sectionName = null)
    {
        var dbSettings = configuration.GetSection(sectionName ?? nameof(RavenSettings)).Get<RavenSettings>();
        return dbSettings;
    }
    
    public static RetrySettings? GetRetrySettings(this IConfiguration configuration, string? sectionName = null)
    {
        var settings = configuration.GetSection(sectionName ?? nameof(RetrySettings)).Get<RetrySettings>();
        return settings;
    }

    public static X509Certificate2 LoadByThumbprint(string thumbprint)
    {
        using var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        certStore.Open(OpenFlags.ReadOnly);

        var cert = certStore.Certificates.OfType<X509Certificate2>()
            .FirstOrDefault(x => x.Thumbprint == thumbprint);

        return cert;
    }
}