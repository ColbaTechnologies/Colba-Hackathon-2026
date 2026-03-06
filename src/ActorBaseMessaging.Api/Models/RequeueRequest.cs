namespace ActorBaseMessaging.Api.Models;

public record RequeueRequest(string RequestId, string TargetUrl, string RawPayload);
