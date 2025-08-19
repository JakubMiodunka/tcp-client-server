namespace Common.Protocols;

/// <summary>
/// Shall be implemented by every session layer protocol handler.
/// </summary>
public interface IProtocol
{
    /// <summary>
    /// Maximal size of payload, that can be contained by a single packet.
    /// </summary>
    int MaxPayloadLength { get; }
    
    /// <summary>
    /// Prepares packet containing provided payload.
    /// </summary>
    /// <param name="payload">
    /// Payload, which prepared packet shall contain.
    /// </param>
    /// <returns>
    /// Packet containing provided payload.
    /// </returns>
    byte[] PreparePacket(IEnumerable<byte> payload);

    /// <summary>
    /// Extracts payload contained by provided packet.
    /// </summary>
    /// <param name="packet">
    /// Packet, which payload shall be extracted.
    /// </param>
    /// <returns>
    /// Payload contained by provided packet.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Shall be thrown, when provided packet is invalid.
    /// </exception>
    byte[] ExtractPayload(IEnumerable<byte> packet);

}
