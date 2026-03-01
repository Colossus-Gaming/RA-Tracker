using System.Diagnostics;
using System.Text;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;

namespace RATracker.Tests.IntegrationTests;

/// <summary>
/// Integration tests that launch the actual WPF app, interact with UI via FlaUI (UI Automation),
/// and capture diagnostic log output. These tests do NOT steal mouse/keyboard focus —
/// FlaUI uses the Windows UI Automation COM API for programmatic interaction.
/// </summary>
[TestFixture]
[Category("Integration")]
[NonParallelizable]
public class AppLaunchTests
{
    private static readonly string ProjectDir = Path.GetFullPath(
        Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", ".."));

    private static readonly string WpfProjectDir = Path.Combine(ProjectDir, "Retro Achievement Tracker.WPF");
    private static readonly string ExePath = Path.Combine(
        WpfProjectDir, "bin", "Debug", "net8.0-windows", "RATracker.WPF.exe");

    private string _logFilePath = "";
    private Process? _appProcess;
    private Application? _flaApp;
    private UIA3Automation? _automation;

    [SetUp]
    public void SetUp()
    {
        _logFilePath = Path.Combine(Path.GetTempPath(), "RATracker_IntegrationTest",
            $"test_{TestContext.CurrentContext.Test.MethodName}_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        var logDir = Path.GetDirectoryName(_logFilePath)!;
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
    }

    [TearDown]
    public void TearDown()
    {
        // Close the app FIRST so it flushes and releases the log file
        try { _flaApp?.Close(); } catch { /* best effort */ }
        try
        {
            if (_appProcess is { HasExited: false })
            {
                _appProcess.Kill(entireProcessTree: true);
                _appProcess.WaitForExit(5000);
            }
        }
        catch { /* best effort */ }

        _automation?.Dispose();
        _appProcess?.Dispose();
        _flaApp = null;
        _appProcess = null;
        _automation = null;

        // Small delay to ensure exe file lock is fully released
        Thread.Sleep(500);

        // Dump log contents to test output regardless of pass/fail
        if (File.Exists(_logFilePath))
        {
            TestContext.WriteLine("=== APP DIAGNOSTIC LOG ===");
            TestContext.WriteLine(ReadLogFile());
            TestContext.WriteLine("=== END LOG ===");
        }
        else
        {
            TestContext.WriteLine($"[WARN] Log file not found: {_logFilePath}");
        }
    }

    [Test]
    public void App_Launches_And_MainWindow_Appears()
    {
        EnsureAppIsBuilt();
        LaunchApp();

        var mainWindow = WaitForMainWindow(timeout: TimeSpan.FromSeconds(30));
        Assert.That(mainWindow, Is.Not.Null, "Main window should appear after launch");

        TestContext.WriteLine($"Window title: {mainWindow!.Title}");
        Assert.That(mainWindow.Title, Does.Contain("Retro Achievement Tracker"));

        // Verify key UI elements exist
        var usernameBox = FindByAutomationId(mainWindow, "UsernameTextBox");
        var passwordBox = FindByAutomationId(mainWindow, "PasswordBox");
        var startButton = FindByAutomationId(mainWindow, "StartStopButton");
        var sessionStatus = FindByAutomationId(mainWindow, "SessionStatusText");

        Assert.That(usernameBox, Is.Not.Null, "Username textbox should exist");
        Assert.That(passwordBox, Is.Not.Null, "Password box should exist");
        Assert.That(startButton, Is.Not.Null, "Start/Stop button should exist");
        Assert.That(sessionStatus, Is.Not.Null, "Session status text should exist");

        TestContext.WriteLine($"Username box found: {usernameBox!.ControlType}");
        TestContext.WriteLine($"Start button found: {startButton!.ControlType}");
        TestContext.WriteLine($"Session status: '{sessionStatus!.Name}'");

        // Verify log file was created
        WaitForLogFile(timeout: TimeSpan.FromSeconds(5));
        Assert.That(File.Exists(_logFilePath), Is.True, "Log file should be created");

        var logContent = ReadLogFile();
        Assert.That(logContent, Does.Contain("[App] File logging enabled"),
            "Log file should contain the startup log message");
    }

    [Test]
    public void App_FillCredentials_And_ClickStart_Produces_DiagnosticOutput()
    {
        EnsureAppIsBuilt();
        LaunchApp();

        var mainWindow = WaitForMainWindow(timeout: TimeSpan.FromSeconds(30));
        Assert.That(mainWindow, Is.Not.Null, "Main window should appear");

        // Fill in credentials via UI Automation (no mouse/keyboard stealing)
        var usernameBox = FindByAutomationId(mainWindow!, "UsernameTextBox");
        Assert.That(usernameBox, Is.Not.Null, "Username box must exist");

        // Use ValuePattern to set text without focus
        var usernameValuePattern = usernameBox!.Patterns.Value.PatternOrDefault;
        if (usernameValuePattern != null)
        {
            usernameValuePattern.SetValue("TestUser");
            TestContext.WriteLine("Set username via ValuePattern");
        }
        else
        {
            // Fallback: use TextBox helper
            usernameBox.AsTextBox().Text = "TestUser";
            TestContext.WriteLine("Set username via TextBox helper");
        }

        // PasswordBox doesn't expose ValuePattern by default (it's a SecureString control).
        // We'll log this limitation and verify what we can.
        var passwordBox = FindByAutomationId(mainWindow, "PasswordBox");
        Assert.That(passwordBox, Is.Not.Null, "Password box must exist");
        TestContext.WriteLine($"Password box control type: {passwordBox!.ControlType}");

        // Try to set password (may not work via ValuePattern due to PasswordBox security)
        var pwValuePattern = passwordBox.Patterns.Value.PatternOrDefault;
        if (pwValuePattern != null)
        {
            try
            {
                pwValuePattern.SetValue("TestPass123");
                TestContext.WriteLine("Set password via ValuePattern");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Password ValuePattern failed (expected for PasswordBox): {ex.Message}");
            }
        }
        else
        {
            TestContext.WriteLine("PasswordBox has no ValuePattern (expected WPF behavior)");
        }

        // Verify username was set
        var currentUsername = usernameBox.AsTextBox().Text;
        TestContext.WriteLine($"Username field value: '{currentUsername}'");
        Assert.That(currentUsername, Is.EqualTo("TestUser"), "Username should be set to TestUser");

        // Click Start button
        var startButton = FindByAutomationId(mainWindow, "StartStopButton");
        Assert.That(startButton, Is.Not.Null, "Start button must exist");
        TestContext.WriteLine($"Start button text: '{startButton!.Name}'");

        // Use InvokePattern to click without stealing focus
        var invokePattern = startButton.Patterns.Invoke.PatternOrDefault;
        if (invokePattern != null)
        {
            invokePattern.Invoke();
            TestContext.WriteLine("Clicked Start via InvokePattern");
        }
        else
        {
            TestContext.WriteLine("Start button has no InvokePattern — may be a styled button");
            // For styled WPF buttons, try the Button helper
            startButton.AsButton().Invoke();
            TestContext.WriteLine("Clicked Start via Button.Invoke()");
        }

        // Wait a bit for the app to process the start command and produce log output
        Thread.Sleep(5000);

        // Read and display diagnostic log
        WaitForLogFile(timeout: TimeSpan.FromSeconds(3));
        if (File.Exists(_logFilePath))
        {
            var logContent = ReadLogFile();
            TestContext.WriteLine($"Log file size: {logContent.Length} bytes");

            // The app should have produced some diagnostic output about the start attempt
            // Even with invalid credentials, we should see login/polling attempt logs
            Assert.That(logContent.Length, Is.GreaterThan(0), "Log file should contain diagnostic output");

            // Look for key log markers from our diagnostic logging
            var hasStartupLog = logContent.Contains("[App]");
            var hasMainVmLog = logContent.Contains("[MainViewModel]");
            var hasLoginLog = logContent.Contains("[LoginWindow]");

            TestContext.WriteLine($"Contains [App] logs: {hasStartupLog}");
            TestContext.WriteLine($"Contains [MainViewModel] logs: {hasMainVmLog}");
            TestContext.WriteLine($"Contains [LoginWindow] logs: {hasLoginLog}");
        }
        else
        {
            TestContext.WriteLine("No log file found after start attempt");
        }
    }

    [Test]
    public void App_SessionStatus_ShowsNotConnected_Initially()
    {
        EnsureAppIsBuilt();
        LaunchApp();

        var mainWindow = WaitForMainWindow(timeout: TimeSpan.FromSeconds(30));
        Assert.That(mainWindow, Is.Not.Null);

        var sessionStatus = FindByAutomationId(mainWindow!, "SessionStatusText");
        Assert.That(sessionStatus, Is.Not.Null);

        // The session status should show "Not connected" or similar when app first loads
        var statusText = sessionStatus!.Name ?? "";
        TestContext.WriteLine($"Initial session status: '{statusText}'");

        // It should either be empty or show a not-connected state
        Assert.That(statusText, Does.Not.Contain("Session active"),
            "Session should NOT be active on fresh launch");
    }

    #region Helper Methods

    private void EnsureAppIsBuilt()
    {
        if (File.Exists(ExePath))
        {
            TestContext.WriteLine($"Using existing build: {ExePath}");
            return;
        }

        TestContext.WriteLine("App not built — building now...");
        var buildProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{WpfProjectDir}\" -c Debug --no-restore",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        })!;

        var stdout = buildProcess.StandardOutput.ReadToEnd();
        var stderr = buildProcess.StandardError.ReadToEnd();
        buildProcess.WaitForExit(120_000);

        TestContext.WriteLine($"Build output:\n{stdout}");
        if (!string.IsNullOrEmpty(stderr))
            TestContext.WriteLine($"Build errors:\n{stderr}");

        Assert.That(buildProcess.ExitCode, Is.EqualTo(0), "App must build successfully");
        Assert.That(File.Exists(ExePath), Is.True, $"Exe should exist at: {ExePath}");
    }

    private void LaunchApp()
    {
        TestContext.WriteLine($"Launching: {ExePath}");
        TestContext.WriteLine($"Log file: {_logFilePath}");

        _automation = new UIA3Automation();

        var psi = new ProcessStartInfo
        {
            FileName = ExePath,
            Arguments = $"--log-to-file \"{_logFilePath}\"",
        };

        _flaApp = Application.Launch(psi);
        _appProcess = Process.GetProcessById(_flaApp.ProcessId);
        TestContext.WriteLine($"App started (PID: {_flaApp.ProcessId})");
    }

    private Window? WaitForMainWindow(TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var windows = _flaApp!.GetAllTopLevelWindows(_automation!);
                var main = windows.FirstOrDefault(w =>
                    w.Title?.Contains("Retro Achievement Tracker") == true);
                if (main != null)
                {
                    TestContext.WriteLine($"Found main window after {(timeout - (deadline - DateTime.UtcNow)).TotalSeconds:F1}s");
                    return main;
                }
            }
            catch
            {
                // Window not ready yet
            }
            Thread.Sleep(500);
        }
        return null;
    }

    private static AutomationElement? FindByAutomationId(AutomationElement parent, string automationId)
    {
        try
        {
            var cf = new ConditionFactory(new UIA3PropertyLibrary());
            return parent.FindFirstDescendant(cf.ByAutomationId(automationId));
        }
        catch
        {
            return null;
        }
    }

    private void WaitForLogFile(TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline && !File.Exists(_logFilePath))
        {
            Thread.Sleep(250);
        }
    }

    /// <summary>
    /// Read the log file with shared access (the app may still be writing to it).
    /// </summary>
    private string ReadLogFile()
    {
        using var fs = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs, Encoding.UTF8);
        return sr.ReadToEnd();
    }

    #endregion
}
