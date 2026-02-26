using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Equipo1_QStash_Clone.Services;

public class QueueMetrics : IDisposable
{
    public const string MeterName = "QStash.Queue";

    private readonly Meter _meter;
    private readonly Counter<long> _messagesCreated;
    private readonly Counter<long> _messagesDeleted;
    private readonly Counter<long> _messagesDelivered;
    private readonly Counter<long> _messagesDeliveryFailed;
    private readonly Counter<long> _deadLetterMessages;
    private readonly Counter<long> _deliveryRetries;
    private readonly Histogram<double> _publishDuration;
    private readonly Histogram<double> _deliveryDuration;

    public QueueMetrics()
    {
        _meter = new Meter(MeterName, "1.0.0");

        _messagesCreated = _meter.CreateCounter<long>(
            "qstash.messages.created",
            unit: "messages",
            description: "Number of messages published to a queue");

        _messagesDeleted = _meter.CreateCounter<long>(
            "qstash.messages.deleted",
            unit: "messages",
            description: "Number of messages deleted from a queue");

        _messagesDelivered = _meter.CreateCounter<long>(
            "qstash.messages.delivered",
            unit: "messages",
            description: "Messages successfully delivered to target URL");

        _messagesDeliveryFailed = _meter.CreateCounter<long>(
            "qstash.messages.delivery_failed",
            unit: "messages",
            description: "Messages that failed all retry attempts");

        _deadLetterMessages = _meter.CreateCounter<long>(
            "qstash.messages.dead_letter",
            unit: "messages",
            description: "Messages sent to dead letter queue");

        _deliveryRetries = _meter.CreateCounter<long>(
            "qstash.messages.retries",
            unit: "retries",
            description: "Total delivery retry attempts");

        _publishDuration = _meter.CreateHistogram<double>(
            "qstash.publish.duration",
            unit: "ms",
            description: "Time elapsed to publish a message end-to-end");

        _deliveryDuration = _meter.CreateHistogram<double>(
            "qstash.delivery.duration",
            unit: "ms",
            description: "Time to deliver a message to the target URL");
    }

    public void MessageCreated(string queueId) =>
        _messagesCreated.Add(1, new KeyValuePair<string, object?>("queue.id", queueId));

    public void MessagesDeleted(string queueId, int count) =>
        _messagesDeleted.Add(count, new KeyValuePair<string, object?>("queue.id", queueId));

    public void MessageDelivered(string queueId) =>
        _messagesDelivered.Add(1, new KeyValuePair<string, object?>("queue.id", queueId));

    public void MessageDeliveryFailed(string queueId) =>
        _messagesDeliveryFailed.Add(1, new KeyValuePair<string, object?>("queue.id", queueId));

    public void DeadLetterMessage(string queueId) =>
        _deadLetterMessages.Add(1, new KeyValuePair<string, object?>("queue.id", queueId));

    public void DeliveryRetry(string queueId) =>
        _deliveryRetries.Add(1, new KeyValuePair<string, object?>("queue.id", queueId));

    public Stopwatch StartPublishTimer() => Stopwatch.StartNew();

    public void RecordPublishDuration(Stopwatch sw, string queueId)
    {
        sw.Stop();
        _publishDuration.Record(sw.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("queue.id", queueId));
    }

    public void RecordDeliveryDuration(Stopwatch sw, string queueId)
    {
        sw.Stop();
        _deliveryDuration.Record(sw.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("queue.id", queueId));
    }

    public void Dispose() => _meter.Dispose();
}
