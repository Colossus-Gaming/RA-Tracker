using RATracker.WPF.Services;

namespace RATracker.Tests.ServiceTests;

/// <summary>
/// Crash logging is the only diagnostic a Production build produces — the developer tooling is
/// compiled out of Release — so it needs to actually write, and it must never throw while doing so.
/// </summary>
[TestFixture]
public class CrashLoggerTests
{
    private static string TodayLogPath =>
        Path.Combine(CrashLogger.LogDirectory, $"crash-{DateTime.Now:yyyyMMdd}.log");

    [Test]
    public void Write_CreatesLogFileContainingTheExceptionDetail()
    {
        var marker = $"marker-{Guid.NewGuid():N}";
        var exception = new InvalidOperationException(marker);

        CrashLogger.Write("UnitTest", exception);

        Assert.That(File.Exists(TodayLogPath), Is.True, "A crash log should have been written");

        var contents = ReadShared(TodayLogPath);
        Assert.Multiple(() =>
        {
            Assert.That(contents, Does.Contain(marker), "The exception message should be recorded");
            Assert.That(contents, Does.Contain(nameof(InvalidOperationException)), "The exception type should be recorded");
            Assert.That(contents, Does.Contain("UnitTest"), "The reported source should be recorded");
        });
    }

    [Test]
    public void Write_RecordsTheInnerExceptionChain()
    {
        var innerMarker = $"inner-{Guid.NewGuid():N}";
        var exception = new ApplicationException("outer", new InvalidOperationException(innerMarker));

        CrashLogger.Write("UnitTest", exception);

        Assert.That(ReadShared(TodayLogPath), Does.Contain(innerMarker),
            "Inner exceptions carry the real cause and must be logged too");
    }

    [Test]
    public void Write_WithNullException_DoesNotThrow()
    {
        // The handlers pass ExceptionObject as Exception?, which can be null.
        Assert.DoesNotThrow(() => CrashLogger.Write("UnitTest", null));
    }

    /// <summary>Reads with sharing, since the logger may still hold the file briefly.</summary>
    private static string ReadShared(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
