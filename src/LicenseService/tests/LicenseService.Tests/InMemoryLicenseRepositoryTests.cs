using System;
using System.Threading.Tasks;
using LicenseService.Models;
using LicenseService.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LicenseService.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="InMemoryLicenseRepository"/> class
    /// </summary>
    [TestClass]
    public class InMemoryLicenseRepositoryTests
    {
        private InMemoryLicenseRepository _repository;

        /// <summary>
        /// Initializes test fixtures before each test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _repository = new InMemoryLicenseRepository();
        }

        /// <summary>
        /// Tests that the CreateLicenseAsync method assigns an ID and returns the created license
        /// </summary>
        [TestMethod]
        [TestCategory("Repository")]
        [TestCategory("Create")]
        public async Task CreateLicenseAsync_ShouldAssignIdAndReturnLicense()
        {
            // Arrange
            var license = new License
            {
                SubscriptionKey = "test-key",
                AppGuid = "test-app-guid",
                StripeSubscriptionId = "test-stripe-id"
            };

            // Act
            var result = await _repository.CreateLicenseAsync(license);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);
            Assert.AreNotEqual(string.Empty, result.Id);
            Assert.AreEqual("test-key", result.SubscriptionKey);
        }

        /// <summary>
        /// Tests that the GetLicenseBySubscriptionKeyAsync method returns a license when it exists
        /// </summary>
        [TestMethod]
        [TestCategory("Repository")]
        [TestCategory("Read")]
        public async Task GetLicenseBySubscriptionKeyAsync_WhenLicenseExists_ShouldReturnLicense()
        {
            // Arrange
            var license = new License
            {
                SubscriptionKey = "test-key",
                AppGuid = "test-app-guid"
            };
            await _repository.CreateLicenseAsync(license);

            // Act
            var result = await _repository.GetLicenseBySubscriptionKeyAsync("test-key");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("test-key", result.SubscriptionKey);
            Assert.AreEqual("test-app-guid", result.AppGuid);
        }

        /// <summary>
        /// Tests that the GetLicenseBySubscriptionKeyAsync method returns null when the license doesn't exist
        /// </summary>
        [TestMethod]
        [TestCategory("Repository")]
        [TestCategory("Read")]
        [TestCategory("ErrorHandling")]
        public async Task GetLicenseBySubscriptionKeyAsync_WhenLicenseDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetLicenseBySubscriptionKeyAsync("non-existent-key");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that the GetLicenseByStripeSubscriptionIdAsync method returns a license when it exists
        /// </summary>
        [TestMethod]
        [TestCategory("Repository")]
        [TestCategory("Read")]
        [TestCategory("StripeIntegration")]
        public async Task GetLicenseByStripeSubscriptionIdAsync_WhenLicenseExists_ShouldReturnLicense()
        {
            // Arrange
            var license = new License
            {
                SubscriptionKey = "test-key",
                StripeSubscriptionId = "test-stripe-id"
            };
            await _repository.CreateLicenseAsync(license);

            // Act
            var result = await _repository.GetLicenseByStripeSubscriptionIdAsync("test-stripe-id");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("test-key", result.SubscriptionKey);
            Assert.AreEqual("test-stripe-id", result.StripeSubscriptionId);
        }

        /// <summary>
        /// Tests that the UpdateLicenseAsync method updates a license when it exists
        /// </summary>
        [TestMethod]
        [TestCategory("Repository")]
        [TestCategory("Update")]
        public async Task UpdateLicenseAsync_WhenLicenseExists_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            var license = new License
            {
                SubscriptionKey = "test-key",
                AppGuid = "original-app-guid"
            };
            var created = await _repository.CreateLicenseAsync(license);
            
            // Modify the license
            created.AppGuid = "updated-app-guid";

            // Act
            var updateResult = await _repository.UpdateLicenseAsync(created);
            var updated = await _repository.GetLicenseBySubscriptionKeyAsync("test-key");

            // Assert
            Assert.IsTrue(updateResult);
            Assert.IsNotNull(updated);
            Assert.AreEqual("updated-app-guid", updated.AppGuid);
        }

        /// <summary>
        /// Tests that the UpdateLicenseAsync method returns false when the license doesn't exist
        /// </summary>
        [TestMethod]
        [TestCategory("Repository")]
        [TestCategory("Update")]
        [TestCategory("ErrorHandling")]
        public async Task UpdateLicenseAsync_WhenLicenseDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var license = new License
            {
                Id = "non-existent-id",
                SubscriptionKey = "test-key"
            };

            // Act
            var result = await _repository.UpdateLicenseAsync(license);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that the DeleteLicenseAsync method returns true when the license exists and is deleted
        /// </summary>
        [TestMethod]
        [TestCategory("Repository")]
        [TestCategory("Delete")]
        public async Task DeleteLicenseAsync_WhenLicenseExists_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var license = new License
            {
                SubscriptionKey = "test-key"
            };
            var created = await _repository.CreateLicenseAsync(license);

            // Act
            var deleteResult = await _repository.DeleteLicenseAsync(created.Id);
            var retrieveResult = await _repository.GetLicenseBySubscriptionKeyAsync("test-key");

            // Assert
            Assert.IsTrue(deleteResult);
            Assert.IsNull(retrieveResult);
        }

        /// <summary>
        /// Tests that the DeleteLicenseAsync method returns false when the license doesn't exist
        /// </summary>
        [TestMethod]
        [TestCategory("Repository")]
        [TestCategory("Delete")]
        [TestCategory("ErrorHandling")]
        public async Task DeleteLicenseAsync_WhenLicenseDoesNotExist_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DeleteLicenseAsync("non-existent-id");

            // Assert
            Assert.IsFalse(result);
        }
    }
} 