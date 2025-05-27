using System;
using System.Threading.Tasks;
using LicenseService.Interfaces;
using LicenseService.Models;
using LicenseService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LicenseService.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="LicenseServiceImpl"/> class
    /// </summary>
    [TestClass]
    public class LicenseServiceImplTests
    {
        private Mock<ILicenseRepository> _mockRepository;
        private Mock<ICryptoService> _mockCryptoService;
        private Mock<ILogger<LicenseServiceImpl>> _mockLogger;
        private Mock<IConfiguration> _mockConfiguration;
        private LicenseServiceImpl _licenseService;

        /// <summary>
        /// Initializes test fixtures before each test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _mockRepository = new Mock<ILicenseRepository>();
            _mockCryptoService = new Mock<ICryptoService>();
            _mockLogger = new Mock<ILogger<LicenseServiceImpl>>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup configuration
            _mockConfiguration.Setup(c => c["Stripe:SecretKey"]).Returns("test-stripe-key");
            _mockConfiguration.Setup(c => c.GetSection("License:MaxOfflineDays").Value).Returns("14");
            
            // Create service with mocked dependencies
            _licenseService = new LicenseServiceImpl(
                _mockRepository.Object,
                _mockCryptoService.Object,
                _mockLogger.Object,
                _mockConfiguration.Object
            );
        }

        /// <summary>
        /// Tests that ValidateLicenseAsync returns an invalid response when license is not found
        /// </summary>
        [TestMethod]
        [TestCategory("LicenseService")]
        [TestCategory("Validation")]
        [TestCategory("ErrorHandling")]
        public async Task ValidateLicenseAsync_WhenLicenseNotFound_ShouldReturnInvalidResponse()
        {
            // Arrange
            string subscriptionKey = "test-key";
            var request = new LicenseValidationRequest { AppGuid = "test-app-guid" };
            
            _mockRepository.Setup(r => r.GetLicenseBySubscriptionKeyAsync(subscriptionKey))
                .ReturnsAsync((License)null);

            // Act
            var result = await _licenseService.ValidateLicenseAsync(subscriptionKey, request);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("not_found", result.Status);
            _mockRepository.Verify(r => r.GetLicenseBySubscriptionKeyAsync(subscriptionKey), Times.Once);
        }

        /// <summary>
        /// Tests that ValidateLicenseAsync returns an invalid response when App GUID doesn't match
        /// </summary>
        [TestMethod]
        [TestCategory("LicenseService")]
        [TestCategory("Validation")]
        [TestCategory("Security")]
        public async Task ValidateLicenseAsync_WhenAppGuidMismatch_ShouldReturnInvalidResponse()
        {
            // Arrange
            string subscriptionKey = "test-key";
            var request = new LicenseValidationRequest { AppGuid = "wrong-app-guid" };
            
            var license = new License
            {
                Id = "test-id",
                SubscriptionKey = subscriptionKey,
                AppGuid = "test-app-guid",
                Status = LicenseStatus.Active
            };
            
            _mockRepository.Setup(r => r.GetLicenseBySubscriptionKeyAsync(subscriptionKey))
                .ReturnsAsync(license);

            // Act
            var result = await _licenseService.ValidateLicenseAsync(subscriptionKey, request);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("app_guid_mismatch", result.Status);
        }

        /// <summary>
        /// Tests that ValidateLicenseAsync returns a valid response for a valid license
        /// </summary>
        [TestMethod]
        [TestCategory("LicenseService")]
        [TestCategory("Validation")]
        public async Task ValidateLicenseAsync_WhenLicenseValid_ShouldReturnValidResponse()
        {
            // Arrange
            string subscriptionKey = "test-key";
            var request = new LicenseValidationRequest { AppGuid = "test-app-guid" };
            
            var license = new License
            {
                Id = "test-id",
                SubscriptionKey = subscriptionKey,
                AppGuid = "test-app-guid",
                Status = LicenseStatus.Active,
                Features = new System.Collections.Generic.List<string> { "feature1", "feature2" }
            };
            
            _mockRepository.Setup(r => r.GetLicenseBySubscriptionKeyAsync(subscriptionKey))
                .ReturnsAsync(license);
            
            _mockRepository.Setup(r => r.UpdateLicenseAsync(It.IsAny<License>()))
                .ReturnsAsync(true);

            // Act
            var result = await _licenseService.ValidateLicenseAsync(subscriptionKey, request);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual("active", result.Status);
            Assert.AreEqual(2, result.Features.Count);
            _mockRepository.Verify(r => r.UpdateLicenseAsync(It.IsAny<License>()), Times.Once);
        }

        /// <summary>
        /// Tests that ValidateLicenseAsync handles expired licenses correctly
        /// </summary>
        [TestMethod]
        [TestCategory("LicenseService")]
        [TestCategory("Validation")]
        [TestCategory("Expiration")]
        public async Task ValidateLicenseAsync_WhenLicenseExpired_ShouldReturnInvalidResponse()
        {
            // Arrange
            string subscriptionKey = "test-key";
            var request = new LicenseValidationRequest { AppGuid = "test-app-guid" };
            
            var license = new License
            {
                Id = "test-id",
                SubscriptionKey = subscriptionKey,
                AppGuid = "test-app-guid",
                Status = LicenseStatus.Active,
                ExpirationDate = DateTime.UtcNow.AddDays(-1) // Expired yesterday
            };
            
            _mockRepository.Setup(r => r.GetLicenseBySubscriptionKeyAsync(subscriptionKey))
                .ReturnsAsync(license);
            
            _mockRepository.Setup(r => r.UpdateLicenseAsync(It.IsAny<License>()))
                .ReturnsAsync(true);

            // Act
            var result = await _licenseService.ValidateLicenseAsync(subscriptionKey, request);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("expired", result.Status);
            
            // Verify the license status was updated to Expired
            _mockRepository.Verify(r => r.UpdateLicenseAsync(It.Is<License>(l => 
                l.Status == LicenseStatus.Expired)), Times.Once);
        }

        /// <summary>
        /// Tests that ValidateLicenseAsync returns an invalid response when feature not entitled
        /// </summary>
        [TestMethod]
        [TestCategory("LicenseService")]
        [TestCategory("Validation")]
        [TestCategory("FeatureEntitlement")]
        public async Task ValidateLicenseAsync_WhenFeatureNotEntitled_ShouldReturnInvalidResponse()
        {
            // Arrange
            string subscriptionKey = "test-key";
            var request = new LicenseValidationRequest 
            { 
                AppGuid = "test-app-guid",
                FeatureId = "premium-feature"
            };
            
            var license = new License
            {
                Id = "test-id",
                SubscriptionKey = subscriptionKey,
                AppGuid = "test-app-guid",
                Status = LicenseStatus.Active,
                Features = new System.Collections.Generic.List<string> { "basic-feature" }
            };
            
            _mockRepository.Setup(r => r.GetLicenseBySubscriptionKeyAsync(subscriptionKey))
                .ReturnsAsync(license);

            // Act
            var result = await _licenseService.ValidateLicenseAsync(subscriptionKey, request);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("feature_not_entitled", result.Status);
        }

        /// <summary>
        /// Tests that GenerateOfflineTokenAsync creates a valid token for valid licenses
        /// </summary>
        [TestMethod]
        [TestCategory("LicenseService")]
        [TestCategory("OfflineToken")]
        public async Task GenerateOfflineTokenAsync_WithValidLicense_ShouldCreateToken()
        {
            // Arrange
            string subscriptionKey = "test-key";
            var request = new OfflineTokenRequest 
            { 
                AppGuid = "test-app-guid",
                RequestedDurationDays = 7
            };
            
            var license = new License
            {
                Id = "test-id",
                SubscriptionKey = subscriptionKey,
                AppGuid = "test-app-guid",
                Status = LicenseStatus.Active,
                Features = new System.Collections.Generic.List<string> { "feature1", "feature2" }
            };
            
            _mockRepository.Setup(r => r.GetLicenseBySubscriptionKeyAsync(subscriptionKey))
                .ReturnsAsync(license);
            
            _mockRepository.Setup(r => r.UpdateLicenseAsync(It.IsAny<License>()))
                .ReturnsAsync(true);
                
            _mockCryptoService.Setup(c => c.SignTokenAsync(It.IsAny<OfflineToken>()))
                .ReturnsAsync("signed-token-value");
                
            _mockConfiguration.Setup(c => c.GetValue<int>("License:MaxOfflineDays", 14))
                .Returns(14);

            // Act
            var result = await _licenseService.GenerateOfflineTokenAsync(subscriptionKey, request);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("signed-token-value", result.Token);
            Assert.IsNotNull(result.ExpirationDate);
            
            // Verify the token was signed with correct data
            _mockCryptoService.Verify(c => c.SignTokenAsync(It.Is<OfflineToken>(t => 
                t.LicenseId == license.Id && 
                t.AppGuid == license.AppGuid && 
                t.Features.Count == 2)), Times.Once);
                
            // Verify license was updated with token issuance date
            _mockRepository.Verify(r => r.UpdateLicenseAsync(It.Is<License>(l => 
                l.LastOfflineTokenIssuedDate != null)), Times.Once);
        }

        /// <summary>
        /// Tests that GenerateOfflineTokenAsync returns failure when license not found
        /// </summary>
        [TestMethod]
        [TestCategory("LicenseService")]
        [TestCategory("OfflineToken")]
        [TestCategory("ErrorHandling")]
        public async Task GenerateOfflineTokenAsync_WhenLicenseNotFound_ShouldReturnFailure()
        {
            // Arrange
            string subscriptionKey = "test-key";
            var request = new OfflineTokenRequest { AppGuid = "test-app-guid" };
            
            _mockRepository.Setup(r => r.GetLicenseBySubscriptionKeyAsync(subscriptionKey))
                .ReturnsAsync((License)null);

            // Act
            var result = await _licenseService.GenerateOfflineTokenAsync(subscriptionKey, request);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNull(result.Token);
            Assert.IsNotNull(result.Message);
        }

        /// <summary>
        /// Tests that GenerateOfflineTokenAsync respects max offline days
        /// </summary>
        [TestMethod]
        [TestCategory("LicenseService")]
        [TestCategory("OfflineToken")]
        [TestCategory("Configuration")]
        public async Task GenerateOfflineTokenAsync_ShouldRespectMaxOfflineDays()
        {
            // Arrange
            string subscriptionKey = "test-key";
            var request = new OfflineTokenRequest 
            { 
                AppGuid = "test-app-guid",
                RequestedDurationDays = 30 // Exceeds max
            };
            
            var license = new License
            {
                Id = "test-id",
                SubscriptionKey = subscriptionKey,
                AppGuid = "test-app-guid",
                Status = LicenseStatus.Active
            };
            
            _mockRepository.Setup(r => r.GetLicenseBySubscriptionKeyAsync(subscriptionKey))
                .ReturnsAsync(license);
            
            _mockRepository.Setup(r => r.UpdateLicenseAsync(It.IsAny<License>()))
                .ReturnsAsync(true);
                
            _mockCryptoService.Setup(c => c.SignTokenAsync(It.IsAny<OfflineToken>()))
                .ReturnsAsync("signed-token-value");
                
            // Set max offline days to 14
            _mockConfiguration.Setup(c => c.GetValue<int>("License:MaxOfflineDays", 14))
                .Returns(14);

            // Act
            var result = await _licenseService.GenerateOfflineTokenAsync(subscriptionKey, request);
            
            DateTime expectedExpiry = DateTime.UtcNow.AddDays(14).Date;

            // Assert
            Assert.IsTrue(result.Success);
            
            // Verify token expires in 14 days (max) not 30 days (requested)
            Assert.IsTrue(result.ExpirationDate.HasValue);
            Assert.AreEqual(expectedExpiry.Date, result.ExpirationDate.Value.Date);
            
            // Verify correct expiry was used in the token
            _mockCryptoService.Verify(c => c.SignTokenAsync(It.Is<OfflineToken>(t => 
                t.ExpiresAt.Date == expectedExpiry.Date)), Times.Once);
        }
    }
} 