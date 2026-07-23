using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Abstractions.Services.Memory;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Helpers;
using JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Models;

namespace JacksonVeroneze.NET.DotnetDiagnosticsLab.Api.Services.Memory;

public class EventLeakService : IEventLeakService
{
    private const int MinSubscriberCount = 1;
    private const int MaxSubscriberCount = 10_000;
    private const int MinPayloadSizeBytes = 1;
    private const int MaxPayloadSizeBytes = 1_048_576;

    private static readonly EventPublisher Publisher = new();

    public SimulationResult Run(
        int subscriberCount,
        int payloadSizeBytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(subscriberCount, MinSubscriberCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(subscriberCount, MaxSubscriberCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(payloadSizeBytes, MinPayloadSizeBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(payloadSizeBytes, MaxPayloadSizeBytes);

        return SimulationRunner.Run(()
            => InternalRun(subscriberCount, payloadSizeBytes));
    }

    private static void InternalRun(
        int subscriberCount,
        int payloadSizeBytes)
    {
        for (var i = 0; i < subscriberCount; i++)
        {
            var subscriber = new EventSubscriber(payloadSizeBytes);

            Publisher.Subscribe(subscriber);
        }
    }

    private sealed class EventPublisher
    {
        public event EventHandler? DataReceived;

        public void Subscribe(
            EventSubscriber subscriber)
        {
            DataReceived += subscriber.OnDataReceived;
        }

        public void Publish()
        {
            DataReceived?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class EventSubscriber(int payloadSizeBytes)
    {
        private byte[] Payload { get; } = new byte[payloadSizeBytes];

        public void OnDataReceived(object? sender, EventArgs e)
        {
            _ = Payload.Length;
        }
    }
}
