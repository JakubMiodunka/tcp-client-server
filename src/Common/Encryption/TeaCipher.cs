using Common.Padding;

namespace Common.Encryption;

/// <summary>
/// Cipher utilizing TEA (Tiny Encryption Algorithm) to encrypt and decrypt data.
/// </summary>
/// <seealso href="https://en.wikipedia.org/wiki/Tiny_Encryption_Algorithm"/>
public sealed class TeaCipher : ICipher
{
    #region Constants
    public const byte DataBlockSize = 8;        // TEA operates on 32-bit (8 bytes) data blocks.
    public const int EncryptionKeySize = 16;    // TEA is using 128-bit (16 bytes) encryption key.
    private const byte Cycles = 32;             // Algorithm authors suggest to perform 32 cycles (64 rounds).
    #endregion

    #region Properties
    private readonly uint[] _keyComponents;
    private readonly IBitPaddingProvider _bitPaddingProvider;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates a new TEA cipher.
    /// </summary>
    /// <param name="encryptionKey">
    /// Encryption key, which new cipher shall use to encrypt and decrypt data.
    /// </param>
    /// <param name="bitPaddingProvider">
    /// A provider of bit padding scheme, which cipher shall use.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    public TeaCipher(byte[] encryptionKey, IBitPaddingProvider bitPaddingProvider)
    {
        #region Arguments validation
        if (encryptionKey is null)
        {
            string argumentName = nameof(encryptionKey);
            const string ErrorMessage = "Provided encryption key is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (encryptionKey.Count() != EncryptionKeySize)
        {
            string argumentName = nameof(encryptionKey);
            string errorMessage = $"Invalid length of provided encryption key: {encryptionKey.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }

        if (bitPaddingProvider is null)
        {
            string argumentName = nameof(bitPaddingProvider);
            const string ErrorMessage = "Provided bit padding provider is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (bitPaddingProvider.SizeOfDataBlock != DataBlockSize)
        {
            string argumentName = nameof(bitPaddingProvider);
            string errorMessage = $"Invalid size of data block set to bit padding provider: {bitPaddingProvider.SizeOfDataBlock}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        _keyComponents = BitUtilities.AsUintArray(encryptionKey);
        _bitPaddingProvider = bitPaddingProvider;
    }
    #endregion

    #region Encryption
    /// <summary>
    /// Encrypts provided data block using TEA (Tiny Encryption Algorithm).
    /// </summary>
    /// <param name="dataBlock">
    /// Data block, which shall be encrypted.
    /// </param>
    /// <returns>
    /// Set of encrypted data, corresponding to provided data block.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    private byte[] EncryptDataBlock(IEnumerable<byte> dataBlock)
    {
        #region Arguments validation
        if (dataBlock is null)
        {
            string argumentName = nameof(dataBlock);
            const string ErrorMessage = "Provided data block is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (dataBlock.Count() != DataBlockSize)
        {
            string argumentName = nameof(dataBlock);
            string errorMessage = $"Invalid size of provided data block: {dataBlock.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        const uint Delta = 2654435769;  // A key schedule constant specified by algorithm authors. 
        uint sum = 0;

        uint[] blockComponents = BitUtilities.AsUintArray(dataBlock);

        byte remainingCycles = Cycles;

        while (0 < remainingCycles--)
        {
            sum += Delta;

            blockComponents[0] += ((blockComponents[1] << 4) + _keyComponents[0]) ^ (blockComponents[1] + sum) ^ ((blockComponents[1] >> 5) + _keyComponents[1]);
            blockComponents[1] += ((blockComponents[0] << 4) + _keyComponents[2]) ^ (blockComponents[0] + sum) ^ ((blockComponents[0] >> 5) + _keyComponents[3]);
        }

        byte[] encryptedBlock = blockComponents
            .SelectMany(BitConverter.GetBytes)
            .ToArray();

        return encryptedBlock;
    }

    /// <summary>
    /// Encrypts provided data set using TEA (Tiny Encryption Algorithm).
    /// </summary>
    /// <param name="data">
    /// Data set, which shall be encrypted.
    /// </param>
    /// <returns>
    /// Set of encrypted data, corresponding to provided data set.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public byte[] Encrypt(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data set is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        byte[] paddedData = _bitPaddingProvider.AddBitPadding(data);

        byte[] encryptedData = paddedData
            .Chunk(DataBlockSize)
            .SelectMany(EncryptDataBlock)
            .ToArray();

        return encryptedData;
    }
    #endregion

    #region Decryption
    /// <summary>
    /// Decrypts provided data block using TEA (Tiny Encryption Algorithm).
    /// </summary>
    /// <param name="dataBlock">
    /// Data block, which shall be decrypted.
    /// </param>
    /// <returns>
    /// Set of decrypted data, corresponding to provided data block.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>    
    private byte[] DecryptDataBlock(IEnumerable<byte> dataBlock)
    {
        #region Arguments validation
        if (dataBlock is null)
        {
            string argumentName = nameof(dataBlock);
            const string ErrorMessage = "Provided data block is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (dataBlock.Count() != DataBlockSize)
        {
            string argumentName = nameof(dataBlock);
            string errorMessage = $"Invalid size of provided data block: {dataBlock.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        const uint Delta = 2654435769;  // A key schedule constant specified by algorithm authors. 
        uint sum = 3337565984;          // Sum is (delta << 5) & 0xFFFFFFFF.

        uint[] blockComponents = BitUtilities.AsUintArray(dataBlock);

        byte remainingCycles = Cycles;

        while (0 < remainingCycles--)
        {
            blockComponents[1] -= ((blockComponents[0] << 4) + _keyComponents[2]) ^ (blockComponents[0] + sum) ^ ((blockComponents[0] >> 5) + _keyComponents[3]);
            blockComponents[0] -= ((blockComponents[1] << 4) + _keyComponents[0]) ^ (blockComponents[1] + sum) ^ ((blockComponents[1] >> 5) + _keyComponents[1]);

            sum -= Delta;
        }

        byte[] decryptedBlock = blockComponents
            .SelectMany(BitConverter.GetBytes)
            .ToArray();

        return decryptedBlock;
    }

    /// <summary>
    /// Decrypts provided data set using TEA (Tiny Encryption Algorithm).
    /// </summary>
    /// <param name="data">
    /// Data, which shall be decrypted.
    /// </param>
    /// <returns>
    /// Set of decrypted data, corresponding to provided data set.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public byte[] Decrypt(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data set is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        byte[] decryptedData = data
            .Chunk(DataBlockSize)
            .SelectMany(DecryptDataBlock)
            .ToArray();

        byte[] unpaddedData = _bitPaddingProvider.RemoveBitPadding(decryptedData);

        return unpaddedData;
    }
    #endregion
}
