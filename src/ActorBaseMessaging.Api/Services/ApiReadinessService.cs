namespace ActorBaseMessaging.Api.Services;

/// <summary>
/// Tracks whether the API is ready to accept new inbound requests.
/// When <c>Api:RequireInitializer</c> is true the gate starts closed and
/// opens only when <see cref="MarkReady"/> is called (by the initializer
/// via POST /internal/recovery-complete).  When the config flag is false
/// (default for local dev) the gate starts open immediately.
/// </summary>
public sealed class ApiReadinessService(IConfiguration configuration)
{
    private volatile bool _isReady =
        !configuration.GetValue<bool>("Api:RequireInitializer", false);

    public bool IsReady => _isReady;

    public void MarkReady() => _isReady = true;
}
