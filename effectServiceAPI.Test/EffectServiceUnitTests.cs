using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using auctionServiceAPI.Controllers;
using auctionServiceAPI.Services;
using effectServiceAPI.Model;

namespace effectServiceAPI.Tests.Services
{
    [TestFixture]
    public class EffectServiceUnitTests
    {
        private Mock<ILogger<EffectController>> _mockLogger;
        private Mock<IEffectService> _mockEffectService;
        private Mock<IConfiguration> _mockConfig;
        private EffectController _controller;

        [SetUp]
        public void Setup()
        {
            // Arrange: Initialiserer mocks og controller før hver test
            _mockLogger = new Mock<ILogger<EffectController>>();
            _mockEffectService = new Mock<IEffectService>();
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["EffectImagePath"]).Returns("/fake/path");

            _controller = new EffectController(_mockLogger.Object, _mockEffectService.Object, _mockConfig.Object);
        }

        [Test]
        public async Task GetAllEffects_ShouldReturnOkWithEffects()
        {
            // Arrange: Setup af mocked data
            var effects = new List<Effect>
            {
                new() { EffectId = Guid.NewGuid(), Title = "Effect 1" }
            };
            _mockEffectService.Setup(s => s.GetAllEffectsAsync()).ReturnsAsync(effects);

            // Act: Kald til controller-metoden
            var result = await _controller.GetAllEffects();

            // Assert: Kontroller at resultatet er korrekt
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            Assert.AreEqual(effects, okResult?.Value);
        }

        [Test]
        public async Task GetEffect_ShouldReturnEffect_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var effect = new Effect { EffectId = id, Title = "Test" };
            _mockEffectService.Setup(s => s.GetEffectAsync(id)).ReturnsAsync(effect);

            // Act
            var result = await _controller.GetEffect(id);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            Assert.AreEqual(effect, okResult?.Value);
        }

        [Test]
        public async Task GetEffect_ShouldReturnNotFound_WhenMissing()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockEffectService.Setup(s => s.GetEffectAsync(id)).ReturnsAsync((Effect?)null);

            // Act
            var result = await _controller.GetEffect(id);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task CreateEffect_ShouldReturnCreated_WhenValidEffect()
        {
            // Arrange
            var effect = new Effect
            {
                EffectId = Guid.NewGuid(),
                Title = "New Effect",
                Description = "Test Description"
            };

            _mockEffectService
                .Setup(s => s.CreateEffectAsync(It.IsAny<Effect>()))
                .ReturnsAsync(effect.EffectId);

            // Act
            var result = await _controller.CreateEffect(effect, null);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            Assert.AreEqual(nameof(_controller.GetEffect), createdResult?.ActionName);
            Assert.AreEqual(effect, createdResult?.Value);
        }

        [Test]
        public async Task TransferToAuction_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();
            var effect = new Effect
            {
                EffectId = id,
                EffectStatus = EffectStatus.InStock
            };

            _mockEffectService.Setup(s => s.GetEffectAsync(id)).ReturnsAsync(effect);
            _mockEffectService.Setup(s => s.TransferToAuctionAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _controller.TransferToAuction(id);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual("Effect successfully transferred to auction", okResult?.Value);
        }
        
        [Test]
        public async Task TransferToAuction_ShouldReturnBadRequest_WhenTransferFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var effect = new Effect
            {
                EffectId = id,
                EffectStatus = EffectStatus.Sold // Kan ikke overføres måske
            };

            _mockEffectService.Setup(s => s.GetEffectAsync(id)).ReturnsAsync(effect);
            _mockEffectService.Setup(s => s.TransferToAuctionAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _controller.TransferToAuction(id);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

    }
}
