using Common.Encryption;
using Common.Padding;
using Moq;
using NUnit.Framework.Internal;

namespace UnitTests.Common.Ciphers;

[Category("UnitTest")]
[TestOf(typeof(TeaCipher))]
[Author("Jakub Miodunka")]
public sealed class TeaCipherTests
{
    #region Constants
    private const int ValidSizeOfDataBlock = 8;         // TEA operates on 32-bit (8 bytes) data blocks.
    private const int ValidSizeOfEncryptionKey = 16;    // TEA is using 128-bit (16 bytes) encryption key.
    #endregion

    #region Auxiliary methods
    private static Mock<IBitPaddingProvider> CreateTransparentBitPaddingProviderFake()
    {
        var bitPaddingProviderFake = new Mock<IBitPaddingProvider>();

        bitPaddingProviderFake
            .Setup(bitPaddingProvider => bitPaddingProvider.SizeOfDataBlock)
            .Returns(ValidSizeOfDataBlock);

        bitPaddingProviderFake
            .Setup(bitPaddingProvider => bitPaddingProvider.AddBitPadding(It.IsAny<byte[]>()))
            .Returns<byte[]>(inputDataSet => inputDataSet);

        bitPaddingProviderFake
            .Setup(bitPaddingProvider => bitPaddingProvider.RemoveBitPadding(It.IsAny<byte[]>()))
            .Returns<byte[]>(inputDataSet => inputDataSet);

        return bitPaddingProviderFake;
    }
    #endregion

    #region Test cases
    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsEncryptionKey()
    {
        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        TestDelegate actionUnderTest = () => new TeaCipher(null!, bitPaddingProviderStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingInvalidEncryptionKey(
        [Values(15, 17)] int invalidSizeOfEncryptionKey)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var encryptionKey = new byte[invalidSizeOfEncryptionKey];
        randomizer.NextBytes(encryptionKey);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        TestDelegate actionUnderTest = () => new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsBitPaddingProvider()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        randomizer.NextBytes(encryptionKey);

        TestDelegate actionUnderTest = () => new TeaCipher(encryptionKey, null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingMisconfiguredBitPaddingProvider(
        [Values(7, 9)] int invalidSizeOfDataBlock)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        randomizer.NextBytes(encryptionKey);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();
        bitPaddingProviderStub
            .Setup(bitPaddingProvider => bitPaddingProvider.SizeOfDataBlock)
            .Returns(invalidSizeOfDataBlock);

        TestDelegate actionUnderTest = () => new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }

    [Test]
    public void EncryptionOfNullReferenceNotPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        randomizer.NextBytes(encryptionKey);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        var instanceUnderTest = new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        TestDelegate actionUnderTest = () => instanceUnderTest.Encrypt(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void DecryptionOfNullReferenceNotPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        randomizer.NextBytes(encryptionKey);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        var instanceUnderTest = new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        TestDelegate actionUnderTest = () => instanceUnderTest.Decrypt(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void EncryptionIsTransparent(
        [Values(0, 2, 3, 4, 8, 9, 16, 27)] int numberOfDataBlocksToProcess)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var encryptionKey = new byte[ValidSizeOfEncryptionKey];
        randomizer.NextBytes(encryptionKey);

        var inputDataSet = new byte[ValidSizeOfDataBlock * numberOfDataBlocksToProcess];
        randomizer.NextBytes(inputDataSet);

        Mock<IBitPaddingProvider> bitPaddingProviderStub = CreateTransparentBitPaddingProviderFake();

        var instanceUnderTest = new TeaCipher(encryptionKey, bitPaddingProviderStub.Object);

        byte[] encryptedDataSet = instanceUnderTest.Encrypt(inputDataSet);
        byte[] decryptedDataSet = instanceUnderTest.Decrypt(encryptedDataSet);

        Assert.That(decryptedDataSet.SequenceEqual(inputDataSet));
    }
    #endregion
}
