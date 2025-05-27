using System;
using System.Threading.Tasks;
using LicenseService.Models;
using LicenseService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LicenseService.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="CryptoService"/> class
    /// </summary>
    [TestClass]
    public class CryptoServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private CryptoService _cryptoService;

        /// <summary>
        /// Initializes test fixtures before each test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            // Setup mock configuration
            _mockConfiguration = new Mock<IConfiguration>();
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s.Value).Returns("TestSigningKey1234567890ABCDEFGHIJKLMNOPQRST");
            
            _mockConfiguration.Setup(c => c["OfflineToken:SigningKey"])
                .Returns("TestSigningKey1234567890ABCDEFGHIJKLMNOPQRST");
            _mockConfiguration.Setup(c => c["OfflineToken:Issuer"])
                .Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["OfflineToken:Audience"])
                .Returns("TestAudience");

            // Create the service with mock dependencies
            _cryptoService = new CryptoService(_mockConfiguration.Object);
        }

        /// <summary>
        /// Tests that the SignTokenAsync method returns a valid JWT token
        /// </summary>
        [TestMethod]
        [TestCategory("Crypto")]
        [TestCategory("TokenSigning")]
        public async Task SignTokenAsync_ShouldReturnValidToken()
        {
            // Arrange
            var token = new OfflineToken
            {
                LicenseId = "test-license-id",
                AppGuid = "test-app-guid",
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14),
                Features = new System.Collections.Generic.List<string> { "feature1", "feature2" },
                Plan = "Pro"
            };

            // Act
            var signedToken = await _cryptoService.SignTokenAsync(token);

            // Assert
            Assert.IsNotNull(signedToken);
            Assert.IsTrue(signedToken.Length > 0);
            Assert.IsTrue(signedToken.Split('.').Length == 3); // JWT has 3 parts separated by dots
        }

        /// <summary>
        /// Tests that the VerifyTokenSignatureAsync method validates a token the service signed
        /// </summary>
        [TestMethod]
        [TestCategory("Crypto")]
        [TestCategory("TokenVerification")]
        public async Task VerifyTokenSignatureAsync_WithValidToken_ShouldReturnTrue()
        {
            // Arrange
            var token = new OfflineToken
            {
                LicenseId = "test-license-id",
                AppGuid = "test-app-guid",
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14),
                Features = new System.Collections.Generic.List<string> { "feature1", "feature2" },
                Plan = "Pro"
            };
            var signedToken = await _cryptoService.SignTokenAsync(token);

            // Act
            var isValid = await _cryptoService.VerifyTokenSignatureAsync(signedToken);

            // Assert
            Assert.IsTrue(isValid);
        }

        /// <summary>
        /// Tests that the VerifyTokenSignatureAsync method rejects an invalid token
        /// </summary>
        [TestMethod]
        [TestCategory("Crypto")]
        [TestCategory("TokenVerification")]
        [TestCategory("ErrorHandling")]
        public async Task VerifyTokenSignatureAsync_WithInvalidToken_ShouldReturnFalse()
        {
            // Arrange
            var invalidToken = "invalid.token.format";

            // Act
            var isValid = await _cryptoService.VerifyTokenSignatureAsync(invalidToken);

            // Assert
            Assert.IsFalse(isValid);
        }

        /// <summary>
        /// Tests that the VerifyTokenSignatureAsync method rejects a tampered token
        /// </summary>
        [TestMethod]
        [TestCategory("Crypto")]
        [TestCategory("TokenVerification")]
        [TestCategory("Security")]
        public async Task VerifyTokenSignatureAsync_WithTamperedToken_ShouldReturnFalse()
        {
            // Arrange
            var token = new OfflineToken
            {
                LicenseId = "test-license-id",
                AppGuid = "test-app-guid",
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14),
                Features = new System.Collections.Generic.List<string> { "feature1", "feature2" },
                Plan = "Pro"
            };
            var signedToken = await _cryptoService.SignTokenAsync(token);
            
            // Tamper with the token by changing a character in the payload section
            var parts = signedToken.Split('.');
            if (parts.Length >= 2)
            {
                // Change a character in the payload (middle) part
                var payload = parts[1];
                var tamperedPayload = payload.Substring(0, payload.Length - 1) + (payload[payload.Length - 1] == 'A' ? 'B' : 'A');
                var tamperedToken = $"{parts[0]}.{tamperedPayload}.{parts[2]}";

                // Act
                var isValid = await _cryptoService.VerifyTokenSignatureAsync(tamperedToken);

                // Assert
                Assert.IsFalse(isValid);
            }
            else
            {
                Assert.Fail("Token did not have the expected JWT format");
            }
        }
    }
} 