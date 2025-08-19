namespace Common.Protocols;

/// <summary>
/// Handler for data manipulation related to Simple Session Layer Protocol - simple 5 layer protocol
/// created by Jakub Miodunka for sake of 'chat-net' project.
/// </summary>
/// <remarks>
/// Sum of bytes contained by packet header is equal to length of its payload.
/// </remarks>
public class SimpleSessionLayerProtocol : IProtocol
{
    #region Properties
    public readonly int HeaderLength;

    public int MaxPayloadLength =>
        HeaderLength * byte.MaxValue;
    #endregion

    #region Instantiation
    /// <summary>
    /// Instantiates Simple Session Layer Protocol handler with specified configuration values. 
    /// </summary>
    /// <param name="headerLength">
    /// Desired length of packet header.
    /// Limits maximal length of packet payload to (herderLength * byte.MaxValue) bytes.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    public SimpleSessionLayerProtocol(int headerLength)
    {
        #region Arguments validation
        if (headerLength < 1)
        {
            string argumentName = nameof(headerLength);
            string errorMessage = $"Specified header length too small: {headerLength}";
            throw new ArgumentOutOfRangeException(argumentName, headerLength, errorMessage);
        }
        #endregion

        HeaderLength = headerLength;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Prepares packet header corresponding to provided payload.
    /// </summary>
    /// <param name="payload">
    /// Payload, to which prepared header shall corresponds.
    /// </param>
    /// <returns>
    /// Packet header corresponding to provided payload.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    private byte[] PrepareHeader(IEnumerable<byte> payload)
    {
        #region Arguments validation
        if (payload is null)
        {
            string argumentName = nameof(payload);
            const string ErrorMessage = "Provided payload is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (MaxPayloadLength < payload.Count())
        {
            string argumentName = nameof(payload);
            string errorMessage = $"Provided payload too large: {payload.Count()} bytes";
            throw new ArgumentException(argumentName, errorMessage);
        }
        #endregion

        var header = new List<int>();
        int headerSum = header.Sum();   // Shall be equal to length of provided payload.

        while (headerSum != payload.Count())
        {
            int headerSumDiviation = payload.Count() - headerSum;

            if (byte.MaxValue < headerSumDiviation)
            {
                header.Add(byte.MaxValue);
            }
            else
            {
                header.Add(headerSumDiviation);
            }

            headerSum = header.Sum();
        }

        while (header.Count() < HeaderLength)   // Zero-padding if needed.
        {
            header.Add(0);
        }

        return header.Select(Convert.ToByte).ToArray();
    }

    /// <summary>
    /// Prepares Simple Session Layer Protocol packet ready to transfer provided payload.
    /// </summary>
    /// <param name="payload">
    /// Payload, which prepared packet shall contain.
    /// </param>
    /// <returns>
    /// Simple Session Layer Protocol packet containing provided payload.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public byte[] PreparePacket(IEnumerable<byte> payload)
    {
        #region Arguments validation
        if (payload is null)
        {
            string argumentName = nameof(payload);
            const string ErrorMessage = "Provided payload is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        var packet = new List<byte>();
        byte[] header = PrepareHeader(payload);

        packet.AddRange(header);
        packet.AddRange(payload);

        return packet.ToArray();
    }

    /// <summary>
    /// Extracts payload transfered by provided Simple Session Layer Protocol packet.
    /// </summary>
    /// <param name="packet">
    /// Packet, which payload shall be extracted.
    /// </param>
    /// <returns>
    /// Payload transfered by provided Simple Session Layer Protocol packet.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    public byte[] ExtractPayload(IEnumerable<byte> packet)
    {
        #region Arguments validation
        if (packet is null)
        {
            string argumentName = nameof(packet);
            const string ErrorMessage = "Provided packet is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (packet.Count() < HeaderLength)
        {
            string argumentName = nameof(packet);
            string errorMessage = $"Size of provided packet too small: {packet.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        byte[] header = packet.Take(HeaderLength).ToArray();
        int payloadLength = header.Select(Convert.ToInt32).Sum();

        if (packet.Count() - HeaderLength < payloadLength)
        {
            string argumentName = nameof(packet);
            const string ErrorMessage = $"Invalid packet provided:";
            throw new ArgumentException(ErrorMessage, argumentName);
        }

        return packet.Skip(HeaderLength).Take(payloadLength).ToArray();
    }
    #endregion
}
