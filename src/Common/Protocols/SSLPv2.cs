using System.Collections.ObjectModel;

namespace TcpClientServer.Common.Protocols;

/// <summary>
/// Handler for data manipulation related to Simple Session Layer Protocol version 2.
/// </summary>
/// <remarks>
/// Protocol header is base 255 representation of payload length.
/// Much more efficient than SSLP version 1.
/// </remarks>
public class SSLPv2 : SSLPv1
{
    #region Properties
    private readonly ReadOnlyCollection<int> _headerWeights;
    private readonly int _maxPayloadLength;
    #endregion

    #region Instantiation
    /// <summary>
    /// Instantiates Simple Session Layer Protocol V2 handler with specified configuration values. 
    /// </summary>
    /// <param name="headerLength">
    /// Desired length of packet header.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    public SSLPv2(int headerLength) : base(headerLength)
    {
        _headerWeights = Enumerable.Range(0, headerLength)
            .Reverse()
            .Select(weight => Math.Pow(byte.MaxValue, weight))
            .Select(Convert.ToInt32)
            .ToArray()
            .AsReadOnly();

        _maxPayloadLength = Convert.ToInt32(Math.Pow(byte.MaxValue, headerLength)) - 1;
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
    protected override int ComputePayloadLength(IEnumerable<byte> header)
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
            string errorMessage = $"Size of provided header too small: {header.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        return Enumerable.Range(0, HeaderLength)
            .Select(index => header.ElementAt(index) * _headerWeights[index])
            .Sum();
    }

    /// <summary>
    /// Prepares packet header corresponding to provided payload.
    /// </summary>
    /// <param name="payload">
    /// Payload, to which prepared header shall correspond.
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
    protected override byte[] PrepareHeader(IEnumerable<byte> payload)
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
            int diviationOfHeaderDeclaration = payload.Count() - headerDeclaredPayloadLength;

            header[index] = (byte)(diviationOfHeaderDeclaration / _headerWeights[index]);
        }

        return header;
    }
    #endregion
}
