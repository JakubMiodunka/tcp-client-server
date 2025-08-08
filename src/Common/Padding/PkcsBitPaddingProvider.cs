namespace Common.Padding;

/// <summary>
/// Bit padding provider utilizing PKCS algorithm.
/// </summary>
/// <seealso href="https://www.ibm.com/docs/en/zos/2.4.0?topic=rules-pkcs-padding-method"/>
public sealed class PkcsBitPaddingProvider : IBitPaddingProvider
{
    #region Properties
    public int SizeOfDataBlock { get; init; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates a new provider of bit padding.
    /// </summary>
    /// <param name="sizeOfDataBlock">
    /// Expected size of data block, to which provided data sets shall be padded.
    /// Shall range between 2 and 256.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    public PkcsBitPaddingProvider(int sizeOfDataBlock)
    {
        #region Arguments validation
        if (sizeOfDataBlock < 2)
        {
            string argumentName = nameof(sizeOfDataBlock);
            string errorMessage = $"Specified size of data block too small: {sizeOfDataBlock}";
            throw new ArgumentOutOfRangeException(argumentName, sizeOfDataBlock, errorMessage);
        }

        if ((byte.MaxValue + 1) < sizeOfDataBlock)
        {
            string argumentName = nameof(sizeOfDataBlock);
            string errorMessage = $"Specified size of data block too large: {sizeOfDataBlock}";
            throw new ArgumentOutOfRangeException(argumentName, sizeOfDataBlock, errorMessage);
        }
        #endregion

        SizeOfDataBlock = sizeOfDataBlock;
    }
    #endregion

    #region Adding bit padding
    /// <summary>
    /// Adds bit padding to provided data block.
    /// </summary>
    /// <param name="dataBlock">
    /// Data block, to which bit padding shall be added. 
    /// </param>
    /// <returns>
    /// Provided data block with added bit padding.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    private byte[] AddBitPaddingToDataBlock(IEnumerable<byte> dataBlock)
    {
        #region Arguments validation
        if (dataBlock is null)
        {
            string argumentName = nameof(dataBlock);
            const string ErrorMessage = "Provided data block is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if ((SizeOfDataBlock - 1) < dataBlock.Count())
        {
            string argumentName = nameof(dataBlock);
            string errorMessage = $"Invalid size of provided data block: {dataBlock.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        byte paddingLength = Convert.ToByte(SizeOfDataBlock - dataBlock.Count());
        byte paddingByte = paddingLength;

        IEnumerable<byte> padding = Enumerable.Repeat(paddingByte, paddingLength);

        return dataBlock.Concat(padding).ToArray();
    }

    /// <summary>
    /// Adds bit padding to the given data set to make its length a multiple of the block size.
    /// </summary>
    /// <param name="data">
    /// Data set, to which bit padding shall be added.
    /// </param>
    /// <returns>
    /// Provided data set, with added bit padding.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    public byte[] AddBitPadding(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data set is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        byte[] paddedData = data
            .Chunk(SizeOfDataBlock - 1)
            .SelectMany(AddBitPaddingToDataBlock)
            .ToArray();

        return paddedData;
    }
    #endregion

    #region Removing bit padding
    /// <summary>
    /// Removes bit padding from provided data block.
    /// </summary>
    /// <param name="dataBlock">
    /// Data block, from which bit padding shall be removed.
    /// </param>
    /// <returns>
    /// Provided data block with removed bit padding.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    private byte[] RemoveBitPaddingFromDataBlock(IEnumerable<byte> dataBlock)
    {
        #region Arguments validation
        if (dataBlock is null)
        {
            string argumentName = nameof(dataBlock);
            const string ErrorMessage = "Provided data block is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (dataBlock.Count() != SizeOfDataBlock)
        {
            string argumentName = nameof(dataBlock);
            string errorMessage = $"Invalid size of provided data block: {dataBlock.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        byte paddingLength = dataBlock.Last();

        byte[] unpaddedDataBlock = dataBlock
            .SkipLast(paddingLength)
            .ToArray();

        return unpaddedDataBlock;
    }

    /// <summary>
    /// Removes bit padding from provided data set.
    /// </summary>
    /// <param name="data">
    /// Set of data, from which bit padding shall be removed.
    /// </param>
    /// <returns>
    /// Provided data set, with removed bit padding.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public byte[] RemoveBitPadding(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data set is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        byte[] unpaddedData = data
            .Chunk(SizeOfDataBlock)
            .SelectMany(RemoveBitPaddingFromDataBlock)
            .ToArray();

        return unpaddedData;
    }
    #endregion
}
