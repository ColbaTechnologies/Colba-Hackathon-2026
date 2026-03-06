namespace ActorBaseMessaging.Api.Services;

using ActorBaseMessaging.Domain.Interfaces;

/// <summary>
/// Tracks whether the API is ready to accept new inbound requests.
///
/// When <c>Api:RequireInitializer</c> is true the gate is closed on startup
/// and opens only after the shared RavenDB flag shows a <c>CompletedAt</c>
/// timestamp that is newer than this instance's own startup time.  This works
/// correctly across any number of API replicas: each instance independently
/// validates the shared flag against its own start time, so no instance-to-
/// instance HTTP coordination is required.
///
/// When the config flag is false (default for local dev) the gate is open
/// immediately and no DB check is performed.
/// </summary>
public sealed class ApiReadinessService(IConfiguration configuration, IMessageRepository repository)
{
    private readonly bool _requireInitializer =
        configuration.GetValue<bool>("Api:RequireInitializer", false);

    private readonly DateTime _startedAt = DateTime.UtcNow;

    private volatile bool _localReady;

    public async ValueTask<bool> EnsureReadyAsync()
    {
        if (!_requireInitializer || _localReady) return true;

        var completedAt = await repository.GetRecoveryCompletedAtAsync();
        if (completedAt.HasValue && completedAt.Value > _startedAt)
        {
            _localReady = true;
            return true;
        }

        return false;
    }
}
