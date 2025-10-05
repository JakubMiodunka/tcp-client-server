namespace TcpClientServer.Common.Protocols;

/// <summary>
/// Handler for data manipulation related to Simple Session Layer Protocol version 1 - simple 5 layer protocol
/// created by Jakub Miodunka for sake of 'chat-net' project.
/// </summary>
/// <remarks>
/// Sum of bytes contained by packet header is equal to length of its payload.
/// Currenty it is prefered to use version 2 as it is much more efficient.
/// </remarks>
public class SSLPv1 : IProtocol
{
    #region Properties
    private readonly int _maxPayloadLength;

    public readonly int HeaderLength;
    #endregion

    #region Instantiation
    /// <summary>
    /// Instantiates Simple Session Layer Protocol handler with specified configuration values. 
    /// </summary>
    /// <param name="headerLength">
    /// Desired length of packet header.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    public SSLPv1(int headerLength)
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
        _maxPayloadLength = HeaderLength * byte.MaxValue;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Determines length of payload declared by provided packet header.
    /// </summary>
    /// <param name="header">
    /// Header, basing on which length of payload shall be determined.
    /// </param>
    /// <returns>
    /// Payload length declared by provided packet header.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    protected virtual int ComputePayloadLength(IEnumerable<byte> header)
    {
        #region Arguments validation
        if (header is null)
        {
            string argumentName = nameof(header);
            const string ErrorMessage = "Provided header is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (header.Count() != HeaderLength)
        {
            string argumentName = nameof(header);
            string errorMessage = $"Invalid size of provided header: {header.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        return header
            .Select(Convert.ToInt32)
            .Sum();
    }

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
    protected virtual byte[] PrepareHeader(IEnumerable<byte> payload)
    {
        #region Arguments validation
        if (payload is null)
        {
            string argumentName = nameof(payload);
            const string ErrorMessage = "Provided payload is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (_maxPayloadLength < payload.Count())
        {
            string argumentName = nameof(payload);
            string errorMessage = $"Size of provided payload too large: {payload.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        var header = new byte[HeaderLength];

        foreach (int index in Enumerable.Range(0, HeaderLength))
        {
            int headerDeclaredPayloadLength = ComputePayloadLength(header);
            int headerSumDiviation = payload.Count() - headerDeclaredPayloadLength;

            header[index] = byte.MaxValue < headerSumDiviation ? byte.MaxValue : Convert.ToByte(headerSumDiviation);
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
        int payloadLength = ComputePayloadLength(header);

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
