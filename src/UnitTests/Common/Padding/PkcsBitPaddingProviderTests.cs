using Common.Padding;
using NUnit.Framework.Internal;

namespace UnitTests.Common.Padding;

[Category("UnitTest")]
[TestOf(typeof(PkcsBitPaddingProvider))]
[Author("Jakub Miodunka")]
public sealed class PkcsBitPaddingProviderTests
{
    #region Default values
    private const int DefaultDataBlockSize = 8;
    #endregion

    #region Test parameters
    // Values chosen using 3-value boundary analysis.
    private static readonly int[] s_validSizesOfDataBlock = [2, 3, 255, 256];
    private static readonly int[] s_invalidSizesOfDataBlock = [1, 257];

    // Any size greater or equal to zero is valid.
    private static readonly int[] s_validSizesOfInputDataSet = [0, 2, 3, 4, 9, 8, 16, 27, 32, 64, 81, 128, 243, 256];
    #endregion

    #region Test cases
    [Test]
    public void InstantiationPossibleUsingValidBlockSize(
        [ValueSource(nameof(s_validSizesOfDataBlock))] int validDataBlockSize)
    {
        TestDelegate actionUnderTest = () => new PkcsBitPaddingProvider(validDataBlockSize);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingInvalidBlockSize(
        [ValueSource(nameof(s_invalidSizesOfDataBlock))] int invalidDataBlockSize)
    {
        TestDelegate actionUnderTest = () => new PkcsBitPaddingProvider(invalidDataBlockSize);

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }

    [Test]
    public void AddingBitPaddingToNullReferenceNotPossible()
    {
        var instanceUnderTest = new PkcsBitPaddingProvider(DefaultDataBlockSize);

        TestDelegate actionUnderTest = () => instanceUnderTest.AddBitPadding(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void SizeOfOutputDataSetMultipleOfDataBlockSize(
        [ValueSource(nameof(s_validSizesOfDataBlock))] int sizeOfDataBlock,
        [ValueSource(nameof(s_validSizesOfInputDataSet))] int sizeOfInputDataSet)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var instanceUnderTest = new PkcsBitPaddingProvider(sizeOfDataBlock);
        var inputDataSet = new byte[sizeOfInputDataSet];
        randomizer.NextBytes(inputDataSet);

        byte[] paddedDataSet = instanceUnderTest.AddBitPadding(inputDataSet);

        Assert.That(paddedDataSet.Count() % sizeOfDataBlock, Is.EqualTo(0));
    }

    [Test]
    public void RemovingBitPaddingFromNullReferenceNotPossible()
    {
        var instanceUnderTest = new PkcsBitPaddingProvider(DefaultDataBlockSize);

        TestDelegate actionUnderTest = () => instanceUnderTest.RemoveBitPadding(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void BitPaddingIsTransparent(
        [ValueSource(nameof(s_validSizesOfDataBlock))] int sizeOfDataBlock,
        [ValueSource(nameof(s_validSizesOfInputDataSet))] int sizeOfInputDataSet)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var instanceUnderTest = new PkcsBitPaddingProvider(sizeOfDataBlock);
        var inputDataSet = new byte[sizeOfInputDataSet];
        randomizer.NextBytes(inputDataSet);

        byte[] paddedDataSet = instanceUnderTest.AddBitPadding(inputDataSet);
        byte[] unpaddedDataSet = instanceUnderTest.RemoveBitPadding(paddedDataSet);

        Assert.That(unpaddedDataSet.SequenceEqual(inputDataSet));
    }
    #endregion
}
