using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LicenseService.Controllers;
using LicenseService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LicenseService.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="StripeWebhookController"/> class
    /// </summary>
    [TestClass]
    public class StripeWebhookControllerTests
    {
        private Mock<ILicenseService> _mockLicenseService;
        private Mock<ILogger<StripeWebhookController>> _mockLogger;
        private StripeWebhookController _controller;

        /// <summary>
        /// Initializes test fixtures before each test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _mockLicenseService = new Mock<ILicenseService>();
            _mockLogger = new Mock<ILogger<StripeWebhookController>>();
            
            _controller = new StripeWebhookController(_mockLicenseService.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Tests that HandleWebhook returns BadRequest when Stripe signature is missing
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("StripeWebhook")]
        [TestCategory("Security")]
        [TestCategory("ErrorHandling")]
        public async Task HandleWebhook_WithMissingStripeSignature_ReturnsBadRequest()
        {
            // Arrange - Setup controller context without signature header
            var httpContext = new DefaultHttpContext();
            var requestStream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
            httpContext.Request.Body = requestStream;
            httpContext.Request.ContentType = "application/json";
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that HandleWebhook returns 500 when webhook processing fails
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("StripeWebhook")]
        [TestCategory("ErrorHandling")]
        public async Task HandleWebhook_WhenProcessingFails_ReturnsInternalServerError()
        {
            // Arrange - Setup controller context with signature header
            var httpContext = new DefaultHttpContext();
            var requestStream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
            httpContext.Request.Body = requestStream;
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.Headers["Stripe-Signature"] = "test-signature";
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            // Setup license service to fail webhook processing
            _mockLicenseService.Setup(s => s.HandleStripeEventAsync("{}", "test-signature"))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that HandleWebhook returns 200 OK when webhook processing succeeds
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("StripeWebhook")]
        [TestCategory("Integration")]
        public async Task HandleWebhook_WhenProcessingSucceeds_ReturnsOk()
        {
            // Arrange - Setup controller context with signature header
            var httpContext = new DefaultHttpContext();
            var requestStream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
            httpContext.Request.Body = requestStream;
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.Headers["Stripe-Signature"] = "test-signature";
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            // Setup license service to succeed webhook processing
            _mockLicenseService.Setup(s => s.HandleStripeEventAsync("{}", "test-signature"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            dynamic value = okResult.Value;
            Assert.IsTrue(value.received);
        }

        /// <summary>
        /// Tests that HandleWebhook returns 500 when an exception occurs
        /// </summary>
        [TestMethod]
        [TestCategory("Controller")]
        [TestCategory("StripeWebhook")]
        [TestCategory("ErrorHandling")]
        [TestCategory("ExceptionHandling")]
        public async Task HandleWebhook_WhenExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange - Setup controller context with signature header
            var httpContext = new DefaultHttpContext();
            var requestStream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
            httpContext.Request.Body = requestStream;
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.Headers["Stripe-Signature"] = "test-signature";
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            // Setup license service to throw an exception
            _mockLicenseService.Setup(s => s.HandleStripeEventAsync("{}", "test-signature"))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
    }
} 