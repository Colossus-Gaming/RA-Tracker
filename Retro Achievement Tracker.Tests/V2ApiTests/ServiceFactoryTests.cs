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
    public void Constructor_WithNullApiKey_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new ServiceFactory(TestUsername, null!));
        
        Assert.That(ex!.ParamName, Is.EqualTo("apiKey"));
    }

    [Test]
    public void Constructor_WithEmptyApiKey_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new ServiceFactory(TestUsername, ""));
        
        Assert.That(ex!.ParamName, Is.EqualTo("apiKey"));
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

    #region ProgressProvider Tests

    [Test]
    public void GetProgressProvider_WithV1Only_ReturnsV1Provider()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForProgress: false);
        using var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        // Act
        var provider = factory.GetProgressProvider();

        // Assert
        Assert.That(provider, Is.InstanceOf<V1AchievementProgressProvider>());
    }

    [Test]
    public void GetProgressProvider_WithV2Enabled_ReturnsV2Provider()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForProgress: true);
        using var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        // Act
        var provider = factory.GetProgressProvider();

        // Assert
        Assert.That(provider, Is.InstanceOf<V2AchievementProgressProvider>());
    }

    [Test]
    public void GetProgressProvider_CalledTwice_ReturnsSameInstance()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForProgress: false);
        using var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        // Act
        var provider1 = factory.GetProgressProvider();
        var provider2 = factory.GetProgressProvider();

        // Assert - should return cached instance
        Assert.That(provider2, Is.SameAs(provider1));
    }

    #endregion

    #region ResetServices Tests

    [Test]
    public void ResetServices_ClearsCache_NewInstancesCreated()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForMetadata: true, useV2ForProgress: false);
        using var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        var metadata1 = factory.GetMetadataService();
        var progress1 = factory.GetProgressProvider();

        // Act
        factory.ResetServices();

        var metadata2 = factory.GetMetadataService();
        var progress2 = factory.GetProgressProvider();

        // Assert - new instances should be created after reset
        Assert.That(metadata2, Is.Not.SameAs(metadata1));
        Assert.That(progress2, Is.Not.SameAs(progress1));
    }

    [Test]
    public void ResetServices_AllowsFeatureFlagChanges()
    {
        // Arrange
        var featureFlags = new FeatureFlagService(useV2ForProgress: false);
        using var factory = new ServiceFactory(TestUsername, TestApiKey, featureFlags);

        var provider1 = factory.GetProgressProvider();
        Assert.That(provider1, Is.InstanceOf<V1AchievementProgressProvider>());

        // Change feature flag and reset
        featureFlags.UseV2ForProgress = true;
        factory.ResetServices();

        // Act
        var provider2 = factory.GetProgressProvider();

        // Assert - new provider should be V2
        Assert.That(provider2, Is.InstanceOf<V2AchievementProgressProvider>());
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
        factory.GetProgressProvider();

        // Act
        factory.Dispose();

        // Assert - accessing services after dispose should throw
        Assert.Throws<ObjectDisposedException>(() => factory.GetMetadataService());
        Assert.Throws<ObjectDisposedException>(() => factory.GetProgressProvider());
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
    public void DefaultConstructor_SetsConservativeDefaults()
    {
        // Act
        var service = new FeatureFlagService();

        // Assert - default values for safe rollout
        Assert.Multiple(() =>
        {
            Assert.That(service.UseV2ForMetadata, Is.True, "V2 should be enabled for metadata by default");
            Assert.That(service.UseV2ForProgress, Is.False, "V2 should be disabled for progress by default");
            Assert.That(service.UseV2ForUserLookup, Is.False, "V2 should be disabled for user lookup by default");
            Assert.That(service.EnableMultiSet, Is.False, "Multi-set should be disabled by default");
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
