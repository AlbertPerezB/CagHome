using CagHome.Simulator.Domain.Models;

namespace CagHome.Simulator.Infrastructure;

/// <summary>
/// Publishes injected telemetry batches to the simulator MQTT topic.
/// </summary>
public interface IInjectedTelemetryPublisher
{
    /// <summary>
    /// Publishes a telemetry batch payload for a patient.
    /// </summary>
    /// <param name="batchPayload">Payload to publish.</param>
    /// <param name="patientId">Id of the patient.</param>
    /// <param name="cancellationToken">Token that can cancel the publish operation.</param>
    /// <returns>A task when publish succeeds.</returns>
    Task PublishAsync(
        MeasurementBatchPayload batchPayload,
        Guid patientId,
        CancellationToken cancellationToken
    );
}