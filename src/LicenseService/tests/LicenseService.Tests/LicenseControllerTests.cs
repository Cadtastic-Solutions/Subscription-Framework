using System;
using System.Threading.Tasks;
using LicenseService.Controllers;
using LicenseService.Interfaces;
using LicenseService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LicenseService.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="LicenseController"/> class
    /// </summary>
    [TestClass]
    public class LicenseControllerTests
    {
        private Mock<ILicenseService> _mockLicenseService;
        private Mock<ILogger<LicenseController>> _mockLogger;
        private LicenseController _controller;

        /// <summary>
        /// Initializes test fixtures before each test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _mockLicenseService = new Mock<ILicenseService>();
            _mockLogger = new Mock<ILogger<LicenseController>>();
            
            _controller = new LicenseController(_mockLicenseService.Object, _mockLogger.Object);
            
            // Setup controller context with headers
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Ocp-Apim-Subscription-Key"] = "test-key";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Tests that ValidateLicense returns BadRequest when subscription key is missing
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("LicenseValidation")]
        [TestCategory("ErrorHandling")]
        public async Task ValidateLicense_WithMissingSubscriptionKey_ReturnsBadRequest()
        {
            // Arrange
            var controller = new LicenseController(_mockLicenseService.Object, _mockLogger.Object);
            var request = new LicenseValidationRequest { AppGuid = "test-app-guid" };

            // Act
            var result = await controller.ValidateLicense(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that ValidateLicense returns 403 Forbidden when license is invalid
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("LicenseValidation")]
        [TestCategory("ErrorHandling")]
        public async Task ValidateLicense_WithInvalidLicense_ReturnsForbidden()
        {
            // Arrange
            var request = new LicenseValidationRequest { AppGuid = "test-app-guid" };
            
            var response = new LicenseValidationResponse
            {
                IsValid = false,
                Status = "not_found",
                Message = "License key not found"
            };
            
            _mockLicenseService.Setup(s => s.ValidateLicenseAsync("test-key", request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ValidateLicense(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(StatusCodes.Status403Forbidden, objectResult.StatusCode);
            Assert.AreEqual(response, objectResult.Value);
        }

        /// <summary>
        /// Tests that ValidateLicense returns 200 OK with response when license is valid
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("LicenseValidation")]
        public async Task ValidateLicense_WithValidLicense_ReturnsOkWithResponse()
        {
            // Arrange
            var request = new LicenseValidationRequest { AppGuid = "test-app-guid" };
            
            var response = new LicenseValidationResponse
            {
                IsValid = true,
                Status = "active",
                Message = "License is valid",
                Features = new System.Collections.Generic.List<string> { "feature1", "feature2" }
            };
            
            _mockLicenseService.Setup(s => s.ValidateLicenseAsync("test-key", request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ValidateLicense(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.AreEqual(response, okResult.Value);
        }

        /// <summary>
        /// Tests that GetOfflineToken returns BadRequest when subscription key is missing
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("OfflineToken")]
        [TestCategory("ErrorHandling")]
        public async Task GetOfflineToken_WithMissingSubscriptionKey_ReturnsBadRequest()
        {
            // Arrange
            var controller = new LicenseController(_mockLicenseService.Object, _mockLogger.Object);
            var request = new OfflineTokenRequest { AppGuid = "test-app-guid" };

            // Act
            var result = await controller.GetOfflineToken(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that GetOfflineToken returns 403 Forbidden when token generation fails
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("OfflineToken")]
        [TestCategory("ErrorHandling")]
        public async Task GetOfflineToken_WhenGenerationFails_ReturnsForbidden()
        {
            // Arrange
            var request = new OfflineTokenRequest { AppGuid = "test-app-guid" };
            
            var response = new OfflineTokenResponse
            {
                Success = false,
                Message = "License key not found"
            };
            
            _mockLicenseService.Setup(s => s.GenerateOfflineTokenAsync("test-key", request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetOfflineToken(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result.Result;
            Assert.AreEqual(StatusCodes.Status403Forbidden, objectResult.StatusCode);
            Assert.AreEqual(response, objectResult.Value);
        }

        /// <summary>
        /// Tests that GetOfflineToken returns 200 OK with token when generation succeeds
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("OfflineToken")]
        public async Task GetOfflineToken_WhenGenerationSucceeds_ReturnsOkWithToken()
        {
            // Arrange
            var request = new OfflineTokenRequest { AppGuid = "test-app-guid" };
            
            var response = new OfflineTokenResponse
            {
                Success = true,
                Token = "signed-token-value",
                ExpirationDate = DateTime.UtcNow.AddDays(14),
                Message = "Offline token valid until..."
            };
            
            _mockLicenseService.Setup(s => s.GenerateOfflineTokenAsync("test-key", request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetOfflineToken(request);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            Assert.AreEqual(response, okResult.Value);
        }
    }
} 