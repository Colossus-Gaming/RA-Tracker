using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using RATracker.WPF.Converters;
using RATracker.WPF.Services;

namespace RATracker.Tests.ConverterTests;

[TestFixture]
public class EnumToBoolConverterTests
{
    private readonly EnumToBoolConverter _converter = new();

    [Test]
    public void Convert_MatchingEnum_ReturnsTrue()
    {
        var result = _converter.Convert(RefocusBehaviorEnum.GO_TO_NEXT, typeof(bool), "GO_TO_NEXT", CultureInfo.InvariantCulture);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Convert_NonMatchingEnum_ReturnsFalse()
    {
        var result = _converter.Convert(RefocusBehaviorEnum.GO_TO_NEXT, typeof(bool), "GO_TO_FIRST", CultureInfo.InvariantCulture);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Convert_CaseInsensitive()
    {
        var result = _converter.Convert(RefocusBehaviorEnum.GO_TO_LAST, typeof(bool), "go_to_last", CultureInfo.InvariantCulture);

        Assert.That(result, Is.True);
    }

    [Test]
    public void Convert_NullValueOrParameter_ReturnsFalse()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_converter.Convert(null!, typeof(bool), "X", CultureInfo.InvariantCulture), Is.False);
            Assert.That(_converter.Convert(RefocusBehaviorEnum.GO_TO_NEXT, typeof(bool), null!, CultureInfo.InvariantCulture), Is.False);
        });
    }

    [Test]
    public void ConvertBack_CheckedTrue_ReturnsParsedEnum()
    {
        var result = _converter.ConvertBack(true, typeof(RefocusBehaviorEnum), "GO_TO_LAST", CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(RefocusBehaviorEnum.GO_TO_LAST));
    }

    [Test]
    public void ConvertBack_CheckedFalse_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack(false, typeof(RefocusBehaviorEnum), "GO_TO_LAST", CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(Binding.DoNothing));
    }
}

[TestFixture]
[Apartment(ApartmentState.STA)]
public class StringToImageSourceConverterTests
{
    private readonly StringToImageSourceConverter _converter = new();

    [Test]
    public void Convert_NullEmptyOrWhitespace_ReturnsNull()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_converter.Convert(null!, typeof(object), null!, CultureInfo.InvariantCulture), Is.Null);
            Assert.That(_converter.Convert(string.Empty, typeof(object), null!, CultureInfo.InvariantCulture), Is.Null);
            Assert.That(_converter.Convert("   ", typeof(object), null!, CultureInfo.InvariantCulture), Is.Null);
        });
    }

    [Test]
    public void Convert_ValidAbsoluteUri_ReturnsBitmapImage()
    {
        var result = _converter.Convert("https://media.retroachievements.org/Badge/00001.png",
            typeof(object), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.InstanceOf<BitmapImage>());
    }

    [Test]
    public void ConvertBack_ReturnsNull()
    {
        Assert.That(_converter.ConvertBack("x", typeof(object), null!, CultureInfo.InvariantCulture), Is.Null);
    }
}
