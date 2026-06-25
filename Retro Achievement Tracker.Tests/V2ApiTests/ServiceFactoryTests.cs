using RATracker.WPF.Services;

namespace RATracker.Tests.V2ApiTests;

/// <summary>
/// Tests for ServiceFactory and FeatureFlagService.
/// </summary>
[TestFixture]
public class ServiceFactoryTests
{
    private const string TestUsername = "TestUser";
    private const string TestApiKey = "test-api-key-12345";

    #region ServiceFactory Construction Tests

    [Test]
    public void Constructor_WithValidCredentials_Succeeds()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            using var factory = new ServiceFactory(TestUsername, TestApiKey);
        });
    }

    [Test]
    public void Constructor_WithNullUsername_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new ServiceFactory(null!, TestApiKey));
        
        Assert.That(ex!.ParamName, Is.EqualTo("username"));
    }

    [Test]
    public void Constructor_WithEmptyUsername_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new ServiceFactory("", TestApiKey));
        
        Assert.That(ex!.ParamName, Is.EqualTo("username"));
    }

    [Test]
    public void Constructor_WithNullApiKey_Succeeds()
    {
        // The API key is optional: session-cookie auth can be used instead, so a null
        // key is normalized to empty rather than throwing.
        Assert.DoesNotThrow(() =>
        {
            using var factory = new ServiceFactory(TestUsername, null!);
        });
    }

    [Test]
    public void Constructor_WithEmptyApiKey_Succeeds()
    {
        Assert.DoesNotThrow(() =>
        {
            using var factory = new ServiceFactory(TestUsername, "");
        });
    }

    #endregion

    #region MetadataService Tests

    [Test]
    public void GetMetadataService_WithV2Enabled_ReturnsV2MetadataService()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForMetadata: true);
        using var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        // Act
        var service = factory.GetMetadataService();

        // Assert
        Assert.That(service, Is.InstanceOf<V2MetadataService>());
    }

    [Test]
    public void GetMetadataService_WithV2Disabled_ThrowsNotSupportedException()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForMetadata: false);
        using var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => factory.GetMetadataService());
    }

    [Test]
    public void GetMetadataService_CalledTwice_ReturnsSameInstance()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForMetadata: true);
        using var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        // Act
        var service1 = factory.GetMetadataService();
        var service2 = factory.GetMetadataService();

        // Assert - should return cached instance
        Assert.That(service2, Is.SameAs(service1));
    }

    #endregion

    #region ProgressService Tests

    [Test]
    public void GetProgressService_ReturnsHybridProgressService()
    {
        // Arrange
        using var factory = new ServiceFactory(TestUsername, TestApiKey);

        // Act
        var service = factory.GetProgressService();

        // Assert - the hybrid service handles V1/V2 selection internally.
        Assert.That(service, Is.InstanceOf<HybridProgressService>());
    }

    [Test]
    public void GetProgressService_CalledTwice_ReturnsSameInstance()
    {
        // Arrange
        using var factory = new ServiceFactory(TestUsername, TestApiKey);

        // Act
        var service1 = factory.GetProgressService();
        var service2 = factory.GetProgressService();

        // Assert - should return cached instance
        Assert.That(service2, Is.SameAs(service1));
    }

    #endregion

    #region ResetServices Tests

    [Test]
    public void ResetServices_ClearsCache_NewInstancesCreated()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForMetadata: true);
        using var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        var metadata1 = factory.GetMetadataService();
        var progress1 = factory.GetProgressService();

        // Act
        factory.ResetServices();

        var metadata2 = factory.GetMetadataService();
        var progress2 = factory.GetProgressService();

        // Assert - new instances should be created after reset
        Assert.That(metadata2, Is.Not.SameAs(metadata1));
        Assert.That(progress2, Is.Not.SameAs(progress1));
    }

    #endregion

    #region Dispose Tests

    [Test]
    public void Dispose_DisposesServices()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForMetadata: true);
        var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        // Get services to create them
        factory.GetMetadataService();
        factory.GetProgressService();

        // Act
        factory.Dispose();

        // Assert - accessing services after dispose should throw
        Assert.Throws<ObjectDisposedException>(() => factory.GetMetadataService());
        Assert.Throws<ObjectDisposedException>(() => factory.GetProgressService());
    }

    [Test]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var factory = new ServiceFactory(TestUsername, TestApiKey);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            factory.Dispose();
            factory.Dispose();
        });
    }

    #endregion
}

/// <summary>
/// Tests for FeatureFlagService.
/// </summary>
[TestFixture]
public class FeatureFlagServiceTests
{
    #region Default Values Tests

    [Test]
    public void DefaultConstructor_EnablesV2WithV1Fallback()
    {
        // Act
        var service = new FeatureFlagService();

        // Assert - V2 is the default path for all operations, with V1 as a safety-net fallback.
        Assert.Multiple(() =>
        {
            Assert.That(service.UseV2ForMetadata, Is.True, "V2 should be enabled for metadata by default");
            Assert.That(service.UseV2ForProgress, Is.True, "V2 should be enabled for progress by default");
            Assert.That(service.UseV2ForUserLookup, Is.True, "V2 should be enabled for user lookup by default");
            Assert.That(service.EnableMultiSet, Is.True, "Multi-set should be enabled by default");
            Assert.That(service.EnableV1Fallback, Is.True, "V1 fallback should be enabled by default");
            Assert.That(service.EnableApiLogging, Is.False, "API logging should be disabled by default");
        });
    }

    #endregion

    #region Custom Configuration Tests

    [Test]
    public void Constructor_WithCustomValues_SetsAllProperties()
    {
        // Act
        var service = new FeatureFlagService(
            useV2ForMetadata: false,
            useV2ForProgress: true,
            useV2ForUserLookup: true,
            enableMultiSet: true,
            enableV1Fallback: false,
            enableApiLogging: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.UseV2ForMetadata, Is.False);
            Assert.That(service.UseV2ForProgress, Is.True);
            Assert.That(service.UseV2ForUserLookup, Is.True);
            Assert.That(service.EnableMultiSet, Is.True);
            Assert.That(service.EnableV1Fallback, Is.False);
            Assert.That(service.EnableApiLogging, Is.True);
        });
    }

    [Test]
    public void Properties_CanBeModifiedAtRuntime()
    {
        // Arrange
        var service = new FeatureFlagService();

        // Act
        service.UseV2ForProgress = true;
        service.EnableApiLogging = true;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.UseV2ForProgress, Is.True);
            Assert.That(service.EnableApiLogging, Is.True);
        });
    }

    #endregion

    #region Interface Contract Tests

    [Test]
    public void FeatureFlagService_ImplementsIFeatureFlagService()
    {
        // Act
        var service = new FeatureFlagService();

        // Assert
        Assert.That(service, Is.InstanceOf<IFeatureFlagService>());
    }

    [Test]
    public void InterfaceProperties_MatchImplementation()
    {
        // Arrange
        IFeatureFlagService service = new FeatureFlagService(
            useV2ForMetadata: true,
            useV2ForProgress: false);

        // Assert - interface properties should work
        Assert.Multiple(() =>
        {
            Assert.That(service.UseV2ForMetadata, Is.True);
            Assert.That(service.UseV2ForProgress, Is.False);
            Assert.That(service.EnableV1Fallback, Is.True);
        });
    }

    #endregion
}
