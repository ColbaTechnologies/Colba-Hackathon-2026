#nullable enable
using System.Security.Cryptography.X509Certificates;

namespace WebApplication1.Infrastructure;

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

    public static X509Certificate2 LoadByThumbprint(string thumbprint)
    {
        using var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        certStore.Open(OpenFlags.ReadOnly);

        var cert = certStore.Certificates.OfType<X509Certificate2>()
            .FirstOrDefault(x => x.Thumbprint == thumbprint);

        return cert;
    }
}